using UnityEngine;

public class GridNode
{
    public bool isWalkable;
    public Vector2Int position;
    public int gridX;
    public int gridY;

    // G cost is the distance from the starting node, H cost is the distance from the end node.
    public int gCost = 0;
    public int hCost = 0;

    // For tracing the path back from the end node to the start.
    public GridNode parent;

    public GameObject unitOnTop;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public GridNode(bool isWalkable, Vector2Int position, int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.position = position;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
