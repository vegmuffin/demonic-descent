using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementManager : MonoBehaviour
{
    public static MovementManager instance;

    [SerializeField] private Camera mainCamera = default;
    [SerializeField] private Tile movementTile = default;
    [Space]
    [SerializeField] private Color mouseTintColor = default;
    [SerializeField] private Color pathfindingColor = default;
    // unitSpeed represents how fast the movement executes, not how many tiles can the unit move.
    public int unitSpeed;

    // For A* Pathfinding, to access the tile on the grid by index, add startX and startY
    [HideInInspector] public GridNode[,] pathfindingGrid;
    private List<Vector3Int> pathfindingTiles = new List<Vector3Int>();
    private int gridStartX;
    private int gridStartY;
    private int distanceX;
    private int distanceY;
    private Vector3Int tempTilePos = Vector3Int.zero;
    private Vector3 tempPos = Vector3.zero;
    private Tilemap wallTilemap;
    private Tilemap groundTilemap;
    private Tilemap movementTilemap;

    [HideInInspector] public GameObject player;
    private UnitMovement playerMovement;
    private Unit playerUnit;

    private bool isHoveringAboveEnemy = false;
    private Bounds enemyBounds;

    private void Awake()
    {
        instance = this;
        mainCamera.orthographicSize = 10;
        player = GameObject.Find("Player");
        movementTilemap = GameObject.Find("MovementTilemap").GetComponent<Tilemap>();
        groundTilemap = GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        playerMovement = player.GetComponent<UnitMovement>();
        playerUnit = player.GetComponent<Unit>();
    }

    private void Update()
    {
        PlayerHover();
        PlayerMove();
    }

    // This gets called when hovering over an UI component. Without this in place, tile doesn't reset and it needs to be re-hovered.
    public void ResetTilePos()
    {
        // Tile is never used since in a 2D world, Z is always 0.
        tempTilePos = new Vector3Int(0, 0, 1);
    }

    // If there is an available path to the target tile, move. Used by both EXPLORING and COMBAT states. And of course, if it's not moving lol.
    private void PlayerMove()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && (pathfindingTiles.Count != 0 || CursorManager.instance.currentState == CursorManager.CursorStates.ATTACK) && !playerMovement.isMoving)
        {
            if(CursorManager.instance.inUse)
                return;

            if(GameStateManager.instance.CheckState("COMBAT"))
                if(CombatManager.instance.currentUnit.isAttacking)
                    return;

            Vector3 playerPos = player.transform.position;
            if(GetRealDistance(playerPos, tempTilePos) > 1 && pathfindingTiles.Count == 0)
                return;
            
            // Proceed fruther only if we are in EXPLORING state or it's our turn when in the COMBAT state. 
            if(GameStateManager.instance.CheckState("EXPLORING") || (GameStateManager.instance.CheckState("COMBAT") && CombatManager.instance.whoseTurn == "Player" && playerUnit.CanAct()))
            {
                // If EXPLORING, alter the game states a bit, since MOVING is also a state (might be used later). Else, clear the movement grid.
                if(!GameStateManager.instance.CheckState("COMBAT"))
                {
                    GameStateManager.instance.ChangeState("MOVING");
                }
                else
                {
                    CombatManager.instance.movementTilemap.ClearAllTiles();
                }

                // Clearing previous tiles and starting the movement.
                foreach(Vector3Int pos in pathfindingTiles)
                    groundTilemap.SetColor(pos, Color.white);

                playerMovement.remainingMoves = 0;
                playerMovement.isMoving = true;

                // Copy the pathfinding tiles into an array because when using pathfindingTiles.Count, it leads to issues if clicking very fast.
                Vector3Int[] path = new Vector3Int[pathfindingTiles.Count];
                pathfindingTiles.CopyTo(path);
                pathfindingTiles.Clear();

                bool isAttacking = false;
                GameObject target = null;
                if(CursorManager.instance.currentState == CursorManager.CursorStates.ATTACK)
                {
                    isAttacking = true;
                    Vector2 overlapCirclePos = new Vector2(tempTilePos.x+0.5f, tempTilePos.y+0.5f);
                    target = Physics2D.OverlapCircle(overlapCirclePos, 0.33f).gameObject;
                }
                Vector3Int currentPosition = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, 0);

                CursorManager.instance.SetCursor("DEFAULT");

                StartCoroutine(playerMovement.MoveAlongPath(currentPosition, path, path.Length, isAttacking, target));
            }
            
        }
    }

    // Paints the tile on which the mouse is hovering.
    private void PlayerHover()
    {
        if((GameStateManager.instance.CheckState("EXPLORING") || GameStateManager.instance.CheckState("COMBAT")) && !CombatManager.instance.initiatingCombatState && !playerMovement.isMoving)
        {
            if(CursorManager.instance.inUse)
            {
                ClearPathfindingTiles();
                return;
            }

            // Getting the precise Vector3Int of the mouse position.
            Vector3 precisePos = Vector3.zero;
            Vector3Int mousePos = CursorManager.instance.GetTileBelowCursor(ref precisePos);

            if(tempTilePos != mousePos || (isHoveringAboveEnemy && tempPos != precisePos))
            {
                // Reset color on the mouse-hovering tile.
                groundTilemap.SetColor(tempTilePos, Color.white);
                tempTilePos = mousePos;
                tempPos = precisePos;

                // If there are some painted pathfinding tiles, clear them.
                ClearPathfindingTiles();

                // Getting the exact grid position for the new tile.
                int gridX = mousePos.x + Mathf.Abs(gridStartX);
                int gridY = mousePos.y + Mathf.Abs(gridStartY);
                bool isPosWalkable = pathfindingGrid[gridX, gridY].isWalkable;

                bool cursorAttackState = CursorManager.instance.CheckState("ATTACK");

                if(pathfindingTiles.Count == 0 && !cursorAttackState)
                {
                    CursorManager.instance.SetCursor("DEFAULT");
                    CursorManager.instance.DisableHoveringPoints();
                }

                // Let's color the pathfinding tiles and execute the pathfinding method.
                if(isPosWalkable || cursorAttackState)
                {
                    groundTilemap.SetColor(mousePos, mouseTintColor);

                    // Proceed fruther only if we are in EXPLORING state or it's our turn when in the COMBAT state.
                    if((GameStateManager.instance.CheckState("COMBAT") && CombatManager.instance.whoseTurn == "Player") || GameStateManager.instance.CheckState("EXPLORING"))
                    {
                        // Pathfinding! Getting needed variables.
                        Vector3Int playerPos = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, 0);
                        int speed = 0;
                        bool isExploring = true;
                        if(GameStateManager.instance.CheckState("COMBAT"))
                        {
                            isExploring = false;
                            speed = playerUnit.currentCombatPoints;
                        }

                        bool isAttacking = false;
                        if(cursorAttackState)
                        {
                            isAttacking = true;
                            isHoveringAboveEnemy = true;

                            Vector2 overlapCirclePos = new Vector2(tempTilePos.x+0.5f, tempTilePos.y+0.5f);
                            Collider2D[] cols = Physics2D.OverlapCircleAll(overlapCirclePos, 0.33f);
                            GameObject enemy = null;
                            foreach(Collider2D col in cols)
                            {
                                if(col.gameObject.tag == "Enemy")
                                {
                                    enemy = col.gameObject;
                                    break;
                                }
                            }
                            enemyBounds = enemy.GetComponent<BoxCollider2D>().bounds;
                            
                            string whichQuadrant = CursorManager.instance.GetMouseEnemyQuadrant(enemyBounds, precisePos);
                            mousePos = GetAttackEndTile(whichQuadrant, mousePos);
                            CursorManager.instance.SetAttackCursorDir(whichQuadrant);
                        }
                        else
                        {
                            isHoveringAboveEnemy = false;
                        }
                            
                        
                        List<GridNode> pathToCoord = Pathfinding(playerPos, mousePos, speed, isExploring, movementTilemap, false, isAttacking);
                        int endIndex = 0;
                        for(int i = pathToCoord.Count-1; i >= endIndex; --i)
                        {
                            Vector3Int coord = new Vector3Int(pathToCoord[i].position.x, pathToCoord[i].position.y, 0);
                            groundTilemap.SetColor(coord, pathfindingColor);
                            pathfindingTiles.Add(coord);
                        }

                        if(!cursorAttackState && pathfindingTiles.Count > 0)
                        {
                            CursorManager.instance.SetCursor("MOVE");
                        }

                        if(GameStateManager.instance.CheckState("COMBAT"))
                        {
                            // How many combat points we're about to consume:
                            int hoveringCombatPoints = 0;
                            
                            hoveringCombatPoints +=  pathfindingTiles.Count;   // Each tile costs 1 combat point.
                            if(cursorAttackState)
                                hoveringCombatPoints += 2; // Attacking costs 2.

                            playerUnit.hoveringCombatPoints = hoveringCombatPoints;
                            
                            if(pathfindingTiles.Count > 0)
                                CursorManager.instance.EnableHoveringPoints();
                        }
                    }
                }
            }
            
        }
    }

    private void ClearPathfindingTiles()
    {
        if(pathfindingTiles.Count == 0)
            return;

        foreach(Vector3Int pos in pathfindingTiles)
        {
            groundTilemap.SetColor(pos, Color.white);
        }
        pathfindingTiles.Clear();
    }

    // Populating grid (bounds are lowest X/Y existing tile and highest X/Y existing tile).
    public void PopulateGrid(int startX, int startY, int endX, int endY)
    {
        gridStartX = startX;
        gridStartY = startY;
        distanceX = Mathf.Abs(endX - startX);
        distanceY = Mathf.Abs(endY - startY);
        pathfindingGrid = new GridNode[distanceX, distanceY];

        // This will be costly, but it only runs once per layout
        for(int x = 0; x < distanceX; ++x)
        {
            for(int y = 0; y < distanceY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x+startX, y+startY, 0);
                pathfindingGrid[x, y] = new GridNode(false, (Vector2Int)tilePos, x, y);

                // Later on I will probably have a method that checks if a given tile can be walkable.
                if(groundTilemap.HasTile(tilePos) && !wallTilemap.HasTile(tilePos))
                    pathfindingGrid[x, y] = new GridNode(true, (Vector2Int)tilePos, x, y);
            }
        }

        // Invalidate room preset obstacle tiles.
        foreach(Room room in RoomManager.instance.allRooms)
        {
            if(room.position.x == 0 && room.position.y == 0)
                continue;
            
            Tilemap roomTilemap = room.roomTilemap;
            BoundsInt tilemapBounds = roomTilemap.cellBounds;
            
            for(int x = tilemapBounds.min.x; x <= tilemapBounds.max.x; ++x)
            {
                for(int y = tilemapBounds.min.y; y <= tilemapBounds.max.y; ++y)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    if(roomTilemap.HasTile(cellPos))
                    {
                        Vector3 worldPos = roomTilemap.GetCellCenterWorld(cellPos);
                        Vector3Int worldPosInt = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), 0);
                        UpdateTileWalkability(worldPosInt, false);
                    }
                }
            }
        }

        // Checking for all enemies and units
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            Vector3Int playerPos = new Vector3Int((int)player.transform.position.x + Mathf.Abs(startX), (int)player.transform.position.y + Mathf.Abs(startY), 0);
            pathfindingGrid[playerPos.x, playerPos.y].isWalkable = false;
        }
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Vector3Int enemyPos = new Vector3Int((int)enemy.transform.position.x + Mathf.Abs(startX), (int)enemy.transform.position.y + Mathf.Abs(startY), 0);
            pathfindingGrid[enemyPos.x, enemyPos.y].isWalkable = false;
        }
    }

    // Diamond-shape grid generation.
    public void GenerateGrid(Vector3Int coord, int speed, Tilemap tilemap, Color tileColor)
    {
        // Clearing previous tiles
        tilemap.ClearAllTiles();

        // Creating the top and then the bottom of the diamond by constantly increasing/decreasing the amount of tiles to spawn.
        int tileCount = 1;
        for(int i = -speed; i <= 0; ++i)
        {
            if(tileCount == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y+speed, 0);
                if(!wallTilemap.HasTile(tilePos))
                {
                    if(groundTilemap.HasTile(tilePos))
                    {
                        tilemap.SetTile(tilePos, movementTile);
                        tilemap.SetColor(tilePos, tileColor);
                    }   
                }  
            }      
            else
            {
                int step = (tileCount-1)/2;
                int yCoord = coord.y+Mathf.Abs(i);
                for(int j = -step; j <= step; ++j)
                {
                    Vector3Int tilePos = new Vector3Int(coord.x+j, yCoord, 0);
                    if(!wallTilemap.HasTile(tilePos))
                    {
                        if(groundTilemap.HasTile(tilePos))
                        {
                            tilemap.SetTile(tilePos, movementTile);
                            tilemap.SetColor(tilePos, tileColor);
                        }   
                    }
                        
                }
            }
            
            tileCount += 2;
        }
        tileCount -= 4;
        for(int i = -1; i >= -speed; --i)
        {
            if(tileCount == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y-speed, 0);
                if(!wallTilemap.HasTile(tilePos))
                {
                    if(groundTilemap.HasTile(tilePos))
                    {
                        tilemap.SetTile(tilePos, movementTile);
                        tilemap.SetColor(tilePos, tileColor);
                    }   
                }
            }
            else
            {
                int step = (tileCount-1)/2;
                int yCoord = coord.y-Mathf.Abs(i);
                for(int j = -step; j <= step; ++j)
                {
                    Vector3Int tilePos = new Vector3Int(coord.x+j, yCoord, 0);
                    if(!wallTilemap.HasTile(tilePos))
                    {
                        if(groundTilemap.HasTile(tilePos))
                        {
                            tilemap.SetTile(tilePos, movementTile);
                            tilemap.SetColor(tilePos, tileColor);
                        }
                    }
                        
                }
            }
            tileCount -= 2;
        }

        int startX = coord.x - speed;
        int startY = coord.y - speed;
        int endX = coord.x + speed;
        int endY = coord.y + speed;
        for(int x = startX; x <= endX; ++x)
        {
            for(int y = startY; y <= endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                List<GridNode> path = Pathfinding(coord, tilePos, speed, false, tilemap, true, false);
                if(path.Count == 0)
                    tilemap.SetTile(tilePos, null);
            }
        }
    }

    // A*
    public List<GridNode> Pathfinding(Vector3Int startCoord, Vector3Int endCoord, int gridSpeed, bool exploring, 
                                      Tilemap whichTilemap, bool gridGeneration, bool isAttacking)
    {
        // Empty path.
        List<GridNode> path = new List<GridNode>();

        // Some checks if the end coordinate is out of bounds or if hovering above a non-movementTilemap tile
        Vector3Int start = new Vector3Int(Mathf.Abs(gridStartX)+startCoord.x, Mathf.Abs(gridStartY)+startCoord.y, 0);
        Vector3Int end = new Vector3Int(Mathf.Abs(gridStartX)+endCoord.x, Mathf.Abs(gridStartY)+endCoord.y, 0);
        if(end.x < 0 || end.x > pathfindingGrid.GetLength(0) || end.y < 0 || end.y > pathfindingGrid.GetLength(1))
        {
            return path;
        }
        if(!CanMove(endCoord, whichTilemap) && !exploring)
        {
            return path;
        }

        int gridX = Mathf.Abs(gridStartX) + endCoord.x;
        int gridY = Mathf.Abs(gridStartY) + endCoord.y;
        if(!pathfindingGrid[gridX, gridY].isWalkable)
            return path;
        
        GridNode startNode = pathfindingGrid[start.x, start.y];
        GridNode endNode = pathfindingGrid[end.x, end.y];

        // The Open list contains nodes for which we have already calculated the F cost (the lowest F cost node can/will be the next current node)
        List<GridNode> openSet = new List<GridNode>();
        // The Closed list contains all the visited nodes (the whole path along with other visited nodes is in this list)
        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        openSet.Add(startNode);

        bool foundPath = false;

        while(openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];
            Vector3Int nodePos = new Vector3Int(currentNode.position.x, currentNode.position.y, 0);
            
            if(!foundPath)
            {
                for(int i = 0; i < openSet.Count; ++i)
                {
                    // If the F cost of a set member is lower than our current node's, that is gonna be our next node.
                    if(openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                        currentNode = openSet[i];
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
            }

            // If we have reached the end, we can safely retrace the path and exit. Unless it exceeds our speed.
            if(currentNode == endNode)
            {
                path = RetracePath(startNode, endNode);

                if(!exploring)
                {
                    int speedCounter = 0;
                    foreach(GridNode node in path)
                        ++speedCounter;
                    if(speedCounter > gridSpeed)
                        return new List<GridNode>();
                }
                return path;
            }

            // Getting all neighbours from our current node.
            foreach(GridNode neighbour in GetNeighbours(currentNode))
            {
                // The node does not interest us if we already have evaluated it or if it's not even walkable.
                if((!neighbour.isWalkable) || closedSet.Contains(neighbour) || (!whichTilemap.HasTile((Vector3Int)neighbour.position) && !exploring))
                {
                    if(neighbour.position == (Vector2Int)endCoord)
                    {
                        neighbour.parent = currentNode;
                        if(openSet.Count == 0)
                            return new List<GridNode>();
                        openSet[0] = neighbour;
                        foundPath = true;

                        break;
                    }
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                /* If the new path to neighbour is shorter than the old path or if the neighbour hasn't been evaluated, calculate the F cost
                and set the parent to current node (to trace path later).*/
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if(!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        

        // If we have reached here, that means we haven't found a path to the end node, which simply means we are returning an empty list.
        return path;
    }

    // Walking back from the end node by going to the parent (recursively, I guess).
    private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        return path;
    }

    private int GetDistance(GridNode nodeA, GridNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(dstX > dstY)
            return 14*dstY + 10*(dstX-dstY);
        return 14*dstX + 10*(dstY-dstX);
    }

    private float GetRealDistance(Vector3 pointA, Vector3 pointB)
    {
        return (pointA-pointB).sqrMagnitude;
    }

    // Getting the valid neighbours. For this particular implementation, we do not need the diagonal tiles as we are only moving in cardinal directions.
    private List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        for(int x = -1; x <= 1; ++x)
        {
            for(int y = -1; y <= 1; ++y)
            {
                if((x == 0 && y == 0) || (x == -1 && y == -1) || (x == 1 && y == -1) || (x == -1 && y == 1) || (x == 1 && y == 1))
                    continue;
                
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < distanceX && checkY >= 0 && checkY < distanceY)
                    neighbours.Add(pathfindingGrid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    // Checking if there's a movement tile in a given coordinate.
    private bool CanMove(Vector3Int coord, Tilemap whichTilemap)
    {
        if(whichTilemap.HasTile(coord))
            return true;
        else
            return false;   
    }

    public void UpdateTileWalkability(Vector3Int pos, bool walkability)
    {
        int gridX = Mathf.Abs(gridStartX) + pos.x;
        int gridY = Mathf.Abs(gridStartY) + pos.y;
        pathfindingGrid[gridX, gridY].isWalkable = walkability;
    }

    private Vector3Int GetAttackEndTile(string whichQuadrant, Vector3Int currentEndTile)
    {
         switch(whichQuadrant)
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
}
