using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(Dust))]
[RequireComponent(typeof(PlayerVisuals))]
[RequireComponent(typeof(AttackItem))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [SerializeField] private Color mouseTintColor = default;
    [SerializeField] private Color pathfindingColor = default;

    [HideInInspector] public Transform player;
    private UnitMovement playerMovement;
    private Unit playerUnit;
    private bool isHoveringAboveEnemy = false;
    private Bounds enemyBounds;

    private List<Vector3Int> pathfindingTiles = new List<Vector3Int>();
    private Tilemap tilemap;
    private Tilemap movementTilemap;
    private int gridOffsetX = 0;
    private int gridOffsetY = 0;
    private int currentGridX = 0;
    private int currentGridY = 0;
    private bool isCurrentWalkable = false;
    private string whichEnemyTriangle = "";
    private GameObject target;

    private Vector3Int tempTilePos = Vector3Int.zero;
    private Vector3 tempPos = Vector3.zero;
    [HideInInspector] public Vector3Int playerPos;


    private void Awake()
    {
        instance = this;

        player = GameObject.Find("Player").transform;
        playerMovement = player.GetComponent<UnitMovement>();
        playerUnit = player.GetComponent<Unit>();

        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        movementTilemap = GameObject.Find("MovementTilemap").GetComponent<Tilemap>();

        playerPos = new Vector3Int((int)player.position.x, (int)player.position.y, 0);
    }

    private void Update()
    {
        PlayerHover();
        PlayerMove();
    }

    /* Method that drives everything related to mouse-on-tile hovering in the game. Reacts to when hovering above
    enemies, unwalkable tiles or just when hovering on the ground. Creates pathfinding indicators. */
    private void PlayerHover()
    {
        if(playerMovement.isMoving)
            return;
        
        if(CombatManager.instance.initiatingCombatState)
            return;
        
        if(CursorManager.instance.inUse)
            return;

        // This only needs to fire when exploring or when it's our turn in combat.
        bool combatState = GameStateManager.instance.CheckState("COMBAT");
        bool exploringState = GameStateManager.instance.CheckState("EXPLORING");
        if(exploringState || (combatState && CombatManager.instance.whoseTurn == "Player"))
        {
            /* Getting the precise Vector3Int of the mouse position, since game tiles are all exactly 1x1. Additionally we
            need the exact floating position for when we are hovering above an enemy and checking on which side the mouse is. */
            Vector3 precisePos = Vector3.zero;
            Vector3Int mousePos = CursorManager.instance.GetTileBelowCursor(ref precisePos);

            // This gets fired when the mouse is crossing the border between two tiles.
            if(tempTilePos != mousePos)
                OnNewTileHover(mousePos, combatState);

            /* OnNewTileHover() updates the 'isHoveringAboveEnemy' bool, which tells us whether we need to continue checking additional
            things every frame. */
            if(!isHoveringAboveEnemy && !isCurrentWalkable)
                return;
            else if(isHoveringAboveEnemy && tempPos != precisePos)
            {
                tempPos = precisePos;

                // Since we are hovering above an enemy, check on which of the 4 sides of the enemy the mouse is.
                string tempTriangle = whichEnemyTriangle;
                whichEnemyTriangle = CursorManager.instance.GetMouseEnemyTriangle(enemyBounds, precisePos);

                // Previous side was different, update from which side we intend to attack.
                if(tempTriangle != whichEnemyTriangle)
                {
                    ClearPathfindingTiles();

                    // Getting the end position based on which side we are attacking from.
                    mousePos = GetAttackEndTile(whichEnemyTriangle, mousePos);

                    // Getting some needed variables for the pathfinding method. If we are exploring, speed does not matter, we walk freely.
                    int speed = 0;
                    bool isExploring = true;
                    if(GameStateManager.instance.CheckState("COMBAT"))
                    {
                        isExploring = false;
                        speed = playerUnit.currentCombatPoints;
                    }

                    // Executing the pathfinding method and coloring the ground to indicate the path.
                    List<GridNode> path = MovementManager.instance.Pathfinding(playerPos, mousePos, speed, isExploring, movementTilemap);
                    int endIndex = 0;
                    for(int i = path.Count-1; i >= endIndex; --i)
                    {
                        Vector3Int coord = new Vector3Int(path[i].position.x, path[i].position.y, 0);
                        tilemap.SetColor(coord, pathfindingColor);
                        pathfindingTiles.Add(coord);
                    }

                    // Time to check what sort of attacking situation is this and update the cursor hovering points as well.
                    float dist = GetRealDistance(playerPos, tempTilePos);
                    int hoveringPoints = 0;

                    // Situation 1: we are not right by the enemy and pathfinding did not generate tiles. Cannot attack.
                    if(dist > 1 && pathfindingTiles.Count == 0)
                    {
                        CursorManager.instance.SetCursor("CANNOT", string.Empty);
                        if(combatState)
                            CursorManager.instance.DisableHoveringPoints();
                    }
                    // Situation 2: we are either exploring or there are enough combat points to walk along the path and attack. Can attack.
                    else if(pathfindingTiles.Count + 2 <= playerUnit.currentCombatPoints || exploringState)
                    {
                        CursorManager.instance.SetCursor("ATTACK", whichEnemyTriangle);
                        hoveringPoints = pathfindingTiles.Count + 2;
                        playerUnit.hoveringCombatPoints = hoveringPoints;
                        if(combatState)
                        {
                            playerUnit.hoveringCombatPoints = hoveringPoints;
                            CursorManager.instance.EnableHoveringPoints();
                        }
                    }
                    // Situation 3: (might be other cases as well) there are not enough combat points to perform both moving and attacking. Cannot attack.
                    else
                    {
                        CursorManager.instance.SetCursor("CANNOT", string.Empty);
                        if(combatState)
                            CursorManager.instance.DisableHoveringPoints();
                        ClearPathfindingTiles();
                    }
                }
            }
        }
    }

    // Updates the hover logic when crossing tiles (highlighting tile, checking for enemies, generating path).
    private void OnNewTileHover(Vector3Int mousePos, bool combatState)
    {
        // Now with free camera movement it's easy to have mouse hovering above non-existent tiles.
        if (!CameraManager.instance.IsPointInsideMap(mousePos))
        {
            isHoveringAboveEnemy = false;
            return;
        }

        // Reset color on the mouse-hovering tile.
        tilemap.SetColor(tempTilePos, Color.white);

        tempTilePos = mousePos;
        ClearPathfindingTiles();

        // Getting the exact pathfinding grid position for the new tile.
        currentGridX = mousePos.x + gridOffsetX;
        currentGridY = mousePos.y + gridOffsetY;
        GridNode tile = MovementManager.instance.pathfindingGrid[currentGridX, currentGridY];
        isCurrentWalkable = tile.isWalkable;
        
        // If the tile is not walkable, maybe there is an enemy on top of that tile which we want to attack.
        if(!isCurrentWalkable)
        {
            GameObject potentialEnemy = tile.unitOnTop;
            if(potentialEnemy)
            {
                if(potentialEnemy.CompareTag("Enemy"))
                {
                    enemyBounds = potentialEnemy.GetComponent<BoxCollider2D>().bounds;
                    isHoveringAboveEnemy = true;
                    target = potentialEnemy;
                    whichEnemyTriangle = "";
                }
                else
                    isHoveringAboveEnemy = false;
            }
            else
                isHoveringAboveEnemy = false;
        }
        else
        {
            // Tile is walkable, highlight mouse position accordingly.
            isHoveringAboveEnemy = false;
            tilemap.SetColor(mousePos, mouseTintColor);
        }

        // If we are not hovering above an enemy, try to find a path to the hovering tile.
        if(!isHoveringAboveEnemy)
        {
            // Getting needed variables for the pathfinding method.
            int speed = 0;
            bool isExploring = true;
            if(GameStateManager.instance.CheckState("COMBAT"))
            {
                isExploring = false;
                speed = playerUnit.currentCombatPoints;
            }

            // Executing the pathfinding method and coloring the path to the end (if there is one).
            List<GridNode> path = MovementManager.instance.Pathfinding(playerPos, mousePos, speed, isExploring, movementTilemap);
            int endIndex = 0;
            for(int i = path.Count-1; i >= endIndex; --i)
            {
                Vector3Int coord = new Vector3Int(path[i].position.x, path[i].position.y, 0);
                tilemap.SetColor(coord, pathfindingColor);
                pathfindingTiles.Add(coord);
            }

            // Updating the cursor state as well as the hovering combat points. There are two scenarios here.
            int hoveringPoints = 0;

            // Scenario 1: the pathfinding method returned us a full path to the end. Can move to the target.
            if(pathfindingTiles.Count > 0)
            {
                CursorManager.instance.SetCursor("MOVE", string.Empty);
                hoveringPoints = pathfindingTiles.Count;
                playerUnit.hoveringCombatPoints = hoveringPoints;
                if(combatState)
                    CursorManager.instance.EnableHoveringPoints();
            }
            // Scenario 2: the pathfinding method did not return any path. Cannot move.
            else
            {
                CursorManager.instance.SetCursor("CANNOT", string.Empty);
                if(combatState)
                    CursorManager.instance.DisableHoveringPoints();
            }

        }

    }

    /* Method that drives the logic when click-to-move is registered. Most of the logic was done when hovering so
    there aren't many things more that we need to do. */
    private void PlayerMove()
    {
        if(CursorManager.instance.inUse)
            return;

        // Check for the mouse click as well as the validity of the path.
        bool isAttacking = CursorManager.instance.CheckState("ATTACK");
        if(Input.GetKeyDown(KeyCode.Mouse0) && (isAttacking || pathfindingTiles.Count != 0))
        {
            bool exploringState = GameStateManager.instance.CheckState("EXPLORING");
            bool combatState = GameStateManager.instance.CheckState("COMBAT");
            
            // Once again, this is only valid if we are exploring or its our turn in combat.
            if(exploringState || (combatState && CombatManager.instance.whoseTurn == "Player"))
            {
                // If exploring, change state to 'moving', else just clear the generated attack grid.
                if(!combatState)
                    GameStateManager.instance.ChangeState("MOVING");
                else
                    CombatManager.instance.movementTilemap.ClearAllTiles();

                // Copy the pathfinding tiles into an array because when using pathfindingTiles.Count, it leads to issues if clicking very fast.
                Vector3Int[] path = new Vector3Int[pathfindingTiles.Count];
                pathfindingTiles.CopyTo(path);
                ClearPathfindingTiles();

                // Clening up on the UI side a bit.
                CursorManager.instance.SetCursor("DEFAULT", string.Empty);
                UIManager.instance.skipButton.interactable = false;
                UIManager.instance.waitButton.interactable = false;

                /* Registering our target and starting to move. The target may something we hovered on in the past, but 
                if we are hovering on an enemy, the variable is always up to date. */
                playerUnit.currentTarget = target;
                playerMovement.StartMoving(path, path.Length, isAttacking);
            }
            
        }
    }

    public void ClearPathfindingTiles()
    {
        if(pathfindingTiles.Count == 0)
            return;

        foreach(Vector3Int pos in pathfindingTiles)
        {
            tilemap.SetColor(pos, Color.white);
        }
        pathfindingTiles.Clear();
    }

    // This gets called when hovering over an UI component. Without this in place, tile doesn't reset and it needs to be re-hovered.
    public void ResetTilePos()
    {
        // Tile is never used since in a 2D world, Z is always 0.
        tempTilePos = new Vector3Int(0, 0, 1);
    }

    // Returns the end tile near the target in one of the four cardinal directions.
    private Vector3Int GetAttackEndTile(string whichTriangle, Vector3Int currentEndTile)
    {
         switch(whichTriangle)
         {
            case "bottom":
                return new Vector3Int(currentEndTile.x, currentEndTile.y - 1, 0);
            case "left":
                return new Vector3Int(currentEndTile.x - 1, currentEndTile.y, 0);
            case "top":
                return new Vector3Int(currentEndTile.x, currentEndTile.y + 1, 0);
            case "right":
                return new Vector3Int(currentEndTile.x + 1, currentEndTile.y, 0);
         }

         return Vector3Int.zero;
    }

    // Used when checking if player is right by an enemy.
    private float GetRealDistance(Vector3 pointA, Vector3 pointB)
    {
        return (pointA-pointB).sqrMagnitude;
    }

    public void SetGridOffset(int x, int y)
    {
        gridOffsetX = x;
        gridOffsetY = y;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Room"))
        {
            Room r = col.GetComponent<RoomSetter>().room;
            RoomManager.instance.SetRoom(r);
        }
    }
}
