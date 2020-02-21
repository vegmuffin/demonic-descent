using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementManager : MonoBehaviour
{
    public static MovementManager instance;

    [SerializeField] private Camera mainCamera = default;
    [Space]
    [SerializeField] private Tile movementTile = default;
    [SerializeField] private Tilemap wallTilemap = default;
    [SerializeField] private Tilemap groundTilemap = default;
    [SerializeField] private Tilemap movementTilemap = default;
    [Space]
    [SerializeField] private Color mouseTintColor = default;
    [SerializeField] private Color availablePathColor = default;
    [SerializeField] private Color pathfindingColor = default;
    public int unitSpeed;

    // For A* Pathfinding, to access the tile on the grid by index, add startX and startY
    [HideInInspector] public GridNode[,] pathfindingGrid;
    private List<Vector3Int> pathfindingTiles = new List<Vector3Int>();
    private int gridStartX;
    private int gridStartY;
    private int distanceX;
    private int distanceY;
    private Vector3Int tempTilePos;

    // For now keep references, when there's no combat system present.
    [SerializeField] private GameObject player = default;

    private void Awake()
    {
        instance = this;
        mainCamera.orthographicSize = 10;
    }

    // test
    // Since we do not have actual movement yet, using the update for testing purposes. :)
    void Update()
    {
        MouseInGrid();
        PlayerMove();
    }

    private void PlayerMove()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && GameStateManager.instance.gameState == GameStateManager.GameStates.EXPLORING)
        {
            player.GetComponent<UnitMovement>().remainingMoves = pathfindingTiles.Count;
            player.GetComponent<UnitMovement>().StartCoroutine(player.GetComponent<UnitMovement>().MoveAlongPath(pathfindingTiles));
            GameStateManager.instance.previousGameState = GameStateManager.instance.gameState;
            GameStateManager.instance.gameState = GameStateManager.GameStates.MOVING;

            foreach(Vector3Int pos in pathfindingTiles)
                groundTilemap.SetColor(pos, Color.white);
        }
    }

    // Paints the tile on which the mouse is hovering.
    private void MouseInGrid()
    {
        if(GameStateManager.instance.gameState == GameStateManager.GameStates.EXPLORING)
        {
            // If there are some painted pathfinding tiles, clear them.
            if(pathfindingTiles.Count > 0)
            {
                foreach(Vector3Int pos in pathfindingTiles)
                {
                    groundTilemap.SetColor(pos, Color.white);
                }
                pathfindingTiles.Clear();
            }

            // Reset color on the mouse-hovering tile.
            groundTilemap.SetColor(tempTilePos, Color.white);

            // Getting the precise Vector3Int of the mouse position.
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.z));
            Vector3Int precisePos = new Vector3Int((int)(Mathf.Ceil(mousePos.x-1f)), (int)(Mathf.Ceil(mousePos.y-1f)), 0);

            // Let's color the pathfinding tiles and execute the pathfinding method.
            if(groundTilemap.HasTile(precisePos))
            {
                groundTilemap.SetColor(precisePos, mouseTintColor);
                tempTilePos = precisePos;

                // Pathfinding! Getting needed variables.
                Vector3Int playerPos = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, 0);
                bool isExploring = false;
                if(GameStateManager.instance.gameState == GameStateManager.GameStates.EXPLORING)
                    isExploring = true;
                int speed = player.GetComponent<UnitMovement>().speed;

                List<GridNode> pathToCoord = Pathfinding(playerPos, precisePos, speed, isExploring);

                foreach(GridNode tile in pathToCoord)
                {
                    Vector3Int coord = new Vector3Int(tile.position.x, tile.position.y, 0);
                    groundTilemap.SetColor(coord, pathfindingColor);
                    pathfindingTiles.Add(coord);
                }
            }
        }
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

                // Later on I will probably have a method that checks if a given tile can be walkable.
                bool isWalkable = true;
                if(!groundTilemap.HasTile(tilePos) || wallTilemap.HasTile(tilePos))
                    isWalkable = false;
                pathfindingGrid[x, y] = new GridNode(isWalkable, (Vector2Int)tilePos, x, y);
            }
        }
    }

    // Diamond-shape grid generation.
    private void GenerateGrid(Vector3Int coord, int speed)
    {
        // Clearing previous tiles
        movementTilemap.ClearAllTiles();

        // How long should the grid be (across)
        int maxspeed = speed*2;

        // Creating the top and then the bottom of the diamond by constantly increasing/decreasing the amount of tiles to spawn.
        int tileCount = 1;
        for(int i = -speed; i <= 0; ++i)
        {
            if(tileCount == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y+speed, 0);
                if(!wallTilemap.HasTile(tilePos))
                    if(groundTilemap.HasTile(tilePos))
                    {
                        movementTilemap.SetTile(tilePos, movementTile);
                        movementTilemap.SetColor(tilePos, availablePathColor);
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
                        if(groundTilemap.HasTile(tilePos))
                        {
                            movementTilemap.SetTile(tilePos, movementTile);
                            movementTilemap.SetColor(tilePos, availablePathColor);
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
                    if(groundTilemap.HasTile(tilePos))
                    {
                        movementTilemap.SetTile(tilePos, movementTile);
                        movementTilemap.SetColor(tilePos, availablePathColor);
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
                        if(groundTilemap.HasTile(tilePos))
                        {
                            movementTilemap.SetTile(tilePos, movementTile);
                            movementTilemap.SetColor(tilePos, availablePathColor);
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
                List<GridNode> path = Pathfinding(coord, tilePos, speed, true);
                if(path.Count == 0)
                    movementTilemap.SetTile(tilePos, null);
            }
        }
    }

    // A*
    public List<GridNode> Pathfinding(Vector3Int startCoord, Vector3Int endCoord, int gridSpeed, bool exploring)
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
        if(!CanMove(endCoord) && !exploring)
        {
            return path;
        }
        
        GridNode startNode = pathfindingGrid[start.x, start.y];
        GridNode endNode = pathfindingGrid[end.x, end.y];

        // The Open list contains nodes for which we have already calculated the F cost (the lowest F cost node can/will be the next current node)
        List<GridNode> openSet = new List<GridNode>();
        // The Closed list contains all the visited nodes (the whole path along with other visited nodes is in this list)
        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        openSet.Add(startNode);


        while(openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];
            Vector3Int nodePos = new Vector3Int(currentNode.position.x, currentNode.position.y, 0);

            //Debug.Log("Node position: " + endNode.position + ", is it walkable? " + endNode.isWalkable + ", fCost: " + endNode.fCost);
            
            for(int i = 0; i < openSet.Count; ++i)
            {
                // If the F cost of a set member is lower than our current node's, that is gonna be our next node.
                if(openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

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
                        else
                            return path;
                    }
                    else
                        return path;
                }

                // Getting all neighbours from our current node.
                foreach(GridNode neighbour in GetNeighbours(currentNode))
                {
                    // The node does not interest us if we already have evaluated it or if it's not even walkable.
                    if(!neighbour.isWalkable || closedSet.Contains(neighbour) || (!movementTilemap.HasTile((Vector3Int)neighbour.position) && !exploring))
                    {
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
    private bool CanMove(Vector3Int coord)
    {
        if(movementTilemap.HasTile(coord))
        {
            return true;
        }
        else
        {
            return false;
        }
            
    }
}
