﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementGrid : MonoBehaviour
{
    public static MovementGrid instance;

    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private Tile gridTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap movementGrid;
    [Space]
    [SerializeField] private int gridLength;
    [SerializeField] private Color mouseTintColor;
    [SerializeField] private Color pathfindingColor;

    private GameObject crosshair;
    private Vector3Int crosshairPos = Vector3Int.zero;
    private Vector3Int tempTilePos;

    // For A* Pathfinding, to access the tile on the grid by index, add startX and startY
    private GridNode[,] pathfindingGrid;
    private List<Vector3Int> pathfindingTiles = new List<Vector3Int>();
    private int gridStartX;
    private int gridStartY;
    private int distanceX;
    private int distanceY;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        crosshair = GameObject.Find("Crosshair");
    }

    // Since we do not have actual movement yet, using the update for testing purposes. :)
    void Update()
    {
        MouseInGrid();
        if(Input.GetKeyDown(KeyCode.G))
        {
            crosshairPos = new Vector3Int((int)Mathf.Floor(crosshair.transform.position.x), (int)Mathf.Floor(crosshair.transform.position.y), 0);
            GenerateGrid(crosshairPos);
        }
    }

    // Paints the tile on which the mouse is hovering.
    private void MouseInGrid()
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
        Vector3Int precisePos = new Vector3Int((int)Mathf.Ceil(mousePos.x)*-1, (int)Mathf.Ceil(mousePos.y-2f)*-1, 0);

        // Let's color the pathfinding tiles and execute the pathfinding method.
        if(groundTilemap.HasTile(precisePos))
        {
            groundTilemap.SetColor(precisePos, mouseTintColor);
            tempTilePos = precisePos;

            // Pathfinding!
            List<GridNode> pathToCoord = Pathfinding(crosshairPos, precisePos);
            foreach(GridNode tile in pathToCoord)
            {
                Vector3Int coord = new Vector3Int(tile.position.x, tile.position.y, 0);
                groundTilemap.SetColor(coord, pathfindingColor);
                pathfindingTiles.Add(coord);
            }
        }

        
    }

    // Diamond-shape grid generation.
    private void GenerateGrid(Vector3Int coord)
    {
        // Clearing previous tiles
        movementGrid.ClearAllTiles();

        // How long should the grid be (across)
        int maxGridLength = gridLength*2;

        // Creating the top and then the bottom of the diamond by constantly increasing/decreasing the amount of tiles to spawn.
        int tileCount = 1;
        for(int i = -gridLength; i <= 0; ++i)
        {
            if(tileCount == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y+gridLength, 0);
                if(!wallTilemap.HasTile(tilePos))
                    if(groundTilemap.HasTile(tilePos))
                        movementGrid.SetTile(tilePos, gridTile);
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
                            movementGrid.SetTile(tilePos, gridTile);
                }
            }
            
            tileCount += 2;
        }
        tileCount -= 4;
        for(int i = -1; i >= -gridLength; --i)
        {
            if(tileCount == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y-gridLength, 0);
                if(!wallTilemap.HasTile(tilePos))
                    if(groundTilemap.HasTile(tilePos))
                        movementGrid.SetTile(tilePos, gridTile);
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
                            movementGrid.SetTile(tilePos, gridTile);
                }
            }
            tileCount -= 2;
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

    // A*
    private List<GridNode> Pathfinding(Vector3Int startCoord, Vector3Int endCoord)
    {
        // Empty path.
        List<GridNode> path = new List<GridNode>();

        // Some checks if the end coordinate is out of bounds or if hovering above a non-movementGrid tile
        Vector3Int start = new Vector3Int(Mathf.Abs(gridStartX)+startCoord.x, Mathf.Abs(gridStartY)+startCoord.y, 0);
        Vector3Int end = new Vector3Int(Mathf.Abs(gridStartX)+endCoord.x, Mathf.Abs(gridStartY)+endCoord.y, 0);
        if(end.x < 0 || end.x > pathfindingGrid.GetLength(0) || end.y < 0 || end.y > pathfindingGrid.GetLength(1))
        {
            return path;
        }
        if(!CanMove(endCoord))
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
            
            for(int i = 0; i < openSet.Count; ++i)
            {
                // If the F cost of a set member is lower than our current node's, that is gonna be our next node.
                if(openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // If we have reached the end, we can safely retrace the path and exit.
                if(currentNode == endNode)
                {
                    path = RetracePath(startNode, endNode);
                    return path;
                }

                // Getting all neighbours from our current node.
                foreach(GridNode neighbour in GetNeighbours(currentNode))
                {
                    // The node does not interest us if we already have evaluated it or if it's not even walkable.
                    if(!neighbour.isWalkable || closedSet.Contains(neighbour))
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
        if(movementGrid.HasTile(coord))
        {
            return true;
        }
        else
        {
            return false;
        }
            
    }
}