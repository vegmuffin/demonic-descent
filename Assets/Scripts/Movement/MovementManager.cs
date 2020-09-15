using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementManager : MonoBehaviour
{
    public static MovementManager instance;

    [SerializeField] private Tile movementTile = default;
    public GameObject dustParticle;
    public float dustDepleteSpeed;
    public AnimationCurve unitSpeedCurve; // unitSpeed represents how fast the movement executes, not how many tiles can the unit move.

    [HideInInspector] public GridNode[,] pathfindingGrid;
    private List<Vector3Int> pathfindingTiles = new List<Vector3Int>();
    private int gridOffsetX;
    private int gridOffsetY;
    private int distanceX;
    private int distanceY;

    private Tilemap tilemap;

    [HideInInspector] public GameObject player;

    private void Awake()
    {
        instance = this;
        player = GameObject.Find("Player");
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    }

    // Populating the entire pathfinding grid.
    public void PopulateGrid(int startX, int startY, int endX, int endY)
    {
        /* Since arrays cannot have negative indices, it is impossible to store coordinates as incides. Hence, we create a
        min X and min Y offset, so the most 'bottom-left' existing coordinate is (0, 0). */
        gridOffsetX = Mathf.Abs(startX);
        gridOffsetY = Mathf.Abs(startY);
        PlayerController.instance.SetGridOffset(gridOffsetX, gridOffsetY);

        // Getting dimension sizes for both axes to initialize the pathfinding grid.
        distanceX = Mathf.Abs(endX - startX);
        distanceY = Mathf.Abs(endY - startY);
        pathfindingGrid = new GridNode[distanceX, distanceY];

        /* Costly operation but it runs only one time. Creates GridNode classes for each tile. GridNode contains information
        such as position, walkability, pathfinding helpers and what unit is standing on top of it.  */
        for(int x = 0; x < distanceX; ++x)
        {
            for(int y = 0; y < distanceY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x+startX, y+startY, 0);
                pathfindingGrid[x, y] = new GridNode(false, (Vector2Int)tilePos, x, y);

                if(tilemap.GetTile(tilePos) is GroundTile)
                {
                    pathfindingGrid[x, y] = new GridNode(true, (Vector2Int)tilePos, x, y);
                }
            }
        }

        // After placing presets, check for obstacles and update tile walkability for tiles that have those obstacles.
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
                        UpdateTileWalkability(worldPosInt, false, null);
                    }
                }
            }

            ShopManager.instance.InvalidateShopTile();
        }

        // Updating the starting tile of the player. This is just to be safe, not to hardcode the starting (0, 0) position of the player.
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            Vector3Int playerPos = new Vector3Int((int)player.transform.position.x + gridOffsetX, (int)player.transform.position.y + gridOffsetY, 0);
            GridNode tile = pathfindingGrid[playerPos.x, playerPos.y];
            tile.isWalkable = false;
            tile.unitOnTop = player;
        }
        // Doing the same for all enemies.
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Vector3Int enemyPos = new Vector3Int((int)enemy.transform.position.x + gridOffsetX, (int)enemy.transform.position.y + gridOffsetY, 0);
            GridNode tile = pathfindingGrid[enemyPos.x, enemyPos.y];
            tile.isWalkable = false;
            tile.unitOnTop = enemy;
        }
    }

    // Diamond shape grid generation.
    public void GenerateAttackGrid(Vector3Int coord, int speed, Tilemap tilemap, Color tileColor)
    {
        // Getting square bounds around the specified coordinate.
        int startX = coord.x - speed;
        int startY = coord.y - speed;
        int endX = coord.x + speed;
        int endY = coord.y + speed;

        /* Previously there was a code that specifically created a diamond-shaped grid, but there is no short, clean algorithm
        to do that, so I ditched it in favour of just creating a square-shaped grid and then invalidating all unreachable tiles. */
        for(int x = startX; x <= endX; ++x)
        {
            for(int y = startY; y <= endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePos, movementTile);
                tilemap.SetColor(tilePos, tileColor);
            }
        }

        // Invalidating unreachable tiles (executing pathfinding there and seeing if it returns any path).
        for(int x = startX; x <= endX; ++x)
        {
            for(int y = startY; y <= endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                List<GridNode> path = Pathfinding(coord, tilePos, speed, false, tilemap);
                if(path.Count == 0)
                    tilemap.SetTile(tilePos, null);
            }
        }
    }

    // A* pathfinding algorithm modified to fit this project.
    public List<GridNode> Pathfinding(Vector3Int startCoord, Vector3Int endCoord, int gridSpeed, bool exploring, 
                                      Tilemap whichTilemap)
    {

        // Empty path.
        List<GridNode> path = new List<GridNode>();

        // Some checks if the end coordinate is out of bounds or if hovering above a non-movementTilemap tile
        Vector3Int start = new Vector3Int(gridOffsetX+startCoord.x, gridOffsetY+startCoord.y, 0);
        Vector3Int end = new Vector3Int(gridOffsetX+endCoord.x, gridOffsetY+endCoord.y, 0);
        if(end.x < 0 || end.x > pathfindingGrid.GetLength(0) || end.y < 0 || end.y > pathfindingGrid.GetLength(1))
        {
            return path;
        }
        if(!CanMove(endCoord, whichTilemap) && !exploring)
        {
            return path;
        }

        int gridX = gridOffsetX + endCoord.x;
        int gridY = gridOffsetY + endCoord.y;
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

    public void UpdateTileWalkability(Vector3Int pos, bool walkability, GameObject unitOnTop)
    {
        int gridX = gridOffsetX + pos.x;
        int gridY = gridOffsetY + pos.y;
        pathfindingGrid[gridX, gridY].isWalkable = walkability;
        pathfindingGrid[gridX, gridY].unitOnTop = unitOnTop;
    }
}
