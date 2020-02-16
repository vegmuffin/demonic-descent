using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGen : MonoBehaviour
{
    [HideInInspector] public static TilemapGen instance;
    [Header("Only odd numbers for intersection width to retain symmetry")]
    [SerializeField] private int offsetBetweenRooms;
    [SerializeField] private int intersectionWidth;
    [Space]
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap transparentTilemap;
    [Space]
    [SerializeField] private Tile straightTileTop;
    [SerializeField] private Tile straightTileSides;
    [SerializeField] private Tile cornerTileSides;
    [SerializeField] private Tile cornerTileTop;
    [SerializeField] private Tile groundTile;
    [SerializeField] private Tile transparentTile;
    [Space]
    [SerializeField] private int leftBound;
    [SerializeField] private int rightBound;
    [SerializeField] private int topBound;
    [SerializeField] private int bottomBound;

    private int testX;
    private int testY;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
            TestingPathfinding();
    }

    public void Generate(List<Vector2> map)
    {
        // For pathfinding grid generation.
        int startX = 0;
        int startY = 0;
        int endX = 0;
        int endY = 0;

        foreach(Vector2 coord in map)
        {
            // Since the layout is generated on a 1x1 scale, we have to expand the actual coordinates to fit our room needs.
            Vector2Int expandedCoord = new Vector2Int((int)coord.x*(rightBound+offsetBetweenRooms)*2, (int)coord.y*(topBound+offsetBetweenRooms)*2);

            // For better management, lets generate different tilemap parts separately.
            GenerateGround(expandedCoord);
            GenerateWalls(expandedCoord);
            GenerateIntersections(expandedCoord);

            // Expanded coordinate bounds
            int expCoordMinX = expandedCoord.x - rightBound - 1;
            int expCoordMinY = expandedCoord.y - topBound - 1;
            int expCoordMaxX = expandedCoord.x + rightBound + 1;
            int expCoordMaxY = expandedCoord.x + topBound + 1;

            // Checking if we have update our min / max values.
            if(expCoordMinX < startX)
                startX = expCoordMinX;
            if(expCoordMinY < startY)
                startY = expCoordMinY;
            if(expCoordMaxX > endX)
                endX = expCoordMaxX;
            if(expCoordMaxY > endY)
                endY = expCoordMaxY;
        }
        testX = startX;
        testY = startY;

        MovementGrid.instance.PopulateGrid(startX, startY, endX, endY);
    }

    private void GenerateGround(Vector2Int coord)
    {
        // Simple square grid generation.
        int startX = coord.x - rightBound + 1;
        int startY = coord.y - topBound + 1;
        int endX = coord.x + rightBound;
        int endY = coord.y + topBound;
        for(int x = startX; x < endX; ++x)
        {
            for(int y = startY; y < endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(tilePos, groundTile);
                transparentTilemap.SetTile(tilePos, transparentTile);
            }
        }
    }

    // Primary function for generating all wall tiles.
    private void GenerateWalls(Vector2Int coord)
    {
        int startX = coord.x - rightBound;
        int startY = coord.y - topBound;
        int endX = coord.x + rightBound;
        int endY = coord.y + topBound;
        for(int x = startX; x <= endX; ++x)
        {
            for(int y = startY; y <= endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                Tile whichTile = null;

                // There are edge cases where we have to paint corner tiles instead and mess with tile transform matrixes.
                if(x == startX || x == endX)
                {
                    Vector3 rot = Vector3.zero;
                    bool corner = IsTileCorner(x, y, startX, endX, startY, endY, ref rot, ref whichTile);

                    if(corner)
                    {
                        wallTilemap.SetTile(tilePos, whichTile);
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rot), Vector3.one);
                        wallTilemap.SetTransformMatrix(tilePos, matrix);
                    }
                    else
                    {
                        wallTilemap.SetTile(tilePos, straightTileTop);
                    }
                } 
                else
                {
                    Vector3Int tilePosBottom = new Vector3Int(x, startY, 0);
                    Vector3Int tilePosTop = new Vector3Int(x, endY, 0);

                    wallTilemap.SetTile(tilePosBottom, straightTileSides);
                    wallTilemap.SetTile(tilePosTop, straightTileSides);
                    break;
                }
            }
        }
    }

    // It's better to paint intersections separately, since their width and length are also modifiable.
    private void GenerateIntersections(Vector2 coord)
    {
        // intersectionCoord will get the middle coordinate between rooms. We will paint various tiles around that.
        // We can also check if there is already an intersection there to avoid pointless computing power.

        // Left
        Vector3Int intersectionCoord = new Vector3Int((int)coord.x-rightBound-offsetBetweenRooms, (int)coord.y, 0);
        Vector3Int checkCoord = new Vector3Int(intersectionCoord.x-offsetBetweenRooms, intersectionCoord.y, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "left");

        // Right
        intersectionCoord = new Vector3Int((int)coord.x+rightBound+offsetBetweenRooms, (int)coord.y, 0);
        checkCoord = new Vector3Int(intersectionCoord.x+offsetBetweenRooms, intersectionCoord.y, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "right");

        // Bottom
        intersectionCoord = new Vector3Int((int)coord.x, (int)coord.y-topBound-offsetBetweenRooms, 0);
        checkCoord = new Vector3Int(intersectionCoord.x, intersectionCoord.y-offsetBetweenRooms, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "bottom");

        // Top
        intersectionCoord = new Vector3Int((int)coord.x, (int)coord.y+topBound+offsetBetweenRooms, 0);
        checkCoord = new Vector3Int(intersectionCoord.x, intersectionCoord.y+offsetBetweenRooms, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "top");
    }

    // Painting the intersection itself.
    private void Intersection(Vector3Int coord, string where)
    {
        // Helper variables that will tell us from which tile to begin painting and where to end, based on serialized variables.
        int stepLength = (int)Mathf.Floor(offsetBetweenRooms/2);
        int stepWidth = (int)Mathf.Floor(intersectionWidth/2);

        // Left / Right intersection generation.
        if(where == "left" || where == "right")
        {
            for(int x = coord.x-offsetBetweenRooms; x <= coord.x+offsetBetweenRooms; ++x)
            {
                for(int y = coord.y-stepWidth; y <= coord.y+stepWidth; ++y)
                {
                    // Painting ground tiles
                    Vector3Int groundTilePos = new Vector3Int(x, y, 0);
                    groundTilemap.SetTile(groundTilePos, groundTile);
                    transparentTilemap.SetTile(groundTilePos, transparentTile);

                    // Eliminating walls that were previously generated. Easier to do than putting more confusing code into wall tilemap generation.
                    if(x == coord.x-offsetBetweenRooms || x == coord.x+offsetBetweenRooms)
                        wallTilemap.SetTile(groundTilePos, null);
                }
            }

            // Painting walls on the side of the intersection for better immersion.
            for(int i = coord.x-offsetBetweenRooms; i <= coord.x+offsetBetweenRooms; ++i)
            {
                wallTilemap.SetTile(new Vector3Int(i, coord.y+stepWidth+1, 0), straightTileSides);
                wallTilemap.SetTile(new Vector3Int(i, coord.y-stepWidth-1, 0), straightTileSides);
            }

            // Edge cases for corners.
            Vector3Int tilePos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y-stepWidth-1, 0);
            wallTilemap.SetTile(tilePos, cornerTileTop);
            groundTilemap.SetTile(tilePos, groundTile);

            tilePos = new Vector3Int(coord.x+offsetBetweenRooms, coord.y-stepWidth-1, 0);
            wallTilemap.SetTile(tilePos, cornerTileTop);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
            wallTilemap.SetTransformMatrix(tilePos, matrix);
            groundTilemap.SetTile(tilePos, groundTile);

            tilePos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y+stepWidth+1, 0);
            wallTilemap.SetTile(tilePos, cornerTileSides);

            tilePos = new Vector3Int(coord.x+offsetBetweenRooms, coord.y+stepWidth+1, 0);
            wallTilemap.SetTile(tilePos, cornerTileSides);
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
            wallTilemap.SetTransformMatrix(tilePos, matrix);               

        } 
        // Bottom / Top intersection generation. Everything is the same but flipped, since length becomes width and vice versa.
        else if(where == "bottom" || where == "top")
        {
            for(int x = coord.x-stepWidth; x <= coord.x+stepWidth; ++x)
            {
                for(int y = coord.y-offsetBetweenRooms; y <= coord.y+offsetBetweenRooms; ++y)
                {
                    Vector3Int groundTilePos = new Vector3Int(x, y, 0);
                    groundTilemap.SetTile(groundTilePos, groundTile);
                    transparentTilemap.SetTile(groundTilePos, transparentTile);

                    if(y == coord.y-offsetBetweenRooms || y == coord.y+offsetBetweenRooms)
                        wallTilemap.SetTile(groundTilePos, null);
                }
            }

            for(int i = coord.y-offsetBetweenRooms; i <= coord.y+offsetBetweenRooms; ++i)
            {
                wallTilemap.SetTile(new Vector3Int(coord.x+stepWidth+1, i, 0), straightTileTop);
                wallTilemap.SetTile(new Vector3Int(coord.x-stepWidth-1, i, 0), straightTileTop);
            }

            Vector3Int tilePos = new Vector3Int(coord.x-stepWidth-1, coord.y-offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, cornerTileSides);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
            wallTilemap.SetTransformMatrix(tilePos, matrix);

            tilePos = new Vector3Int(coord.x+stepWidth+1, coord.y-offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, cornerTileSides);

            tilePos = new Vector3Int(coord.x-stepWidth-1, coord.y+offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, cornerTileTop);
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
            wallTilemap.SetTransformMatrix(tilePos, matrix);
            groundTilemap.SetTile(tilePos, groundTile);

            tilePos = new Vector3Int(coord.x+stepWidth+1, coord.y+offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, cornerTileTop);
            groundTilemap.SetTile(tilePos, groundTile);
            
        }

    }

    // Checks if a tile at a given position should be a corner, returns a boolean as well as returns how it should be rotated.
    private bool IsTileCorner(int x, int y, int startX, int endX, int startY, int endY, ref Vector3 rot, ref Tile whichTile)
    {
        if(y == startY && x == startX)
        {
            whichTile = cornerTileSides;
            return true;
        }
        else if(y == endY && x == startX)
        {
            whichTile = cornerTileTop;
            return true;
        }
        else if(y == startY && x == endX)
        {
            whichTile = cornerTileSides;
            rot = new Vector3(0, 180f, 0f);
            return true;
        }
        else if(y == endY && x == endX)
        {
            rot = new Vector3(0, 180f, 0f);
            whichTile = cornerTileTop;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TestingPathfinding()
    {
        for(int x = -2; x <= 2; ++x)
        {
            Vector3Int tilePos = new Vector3Int(x, 2, 0);
            int accessX = tilePos.x + Mathf.Abs(testX);
            int accessY = tilePos.y + Mathf.Abs(testY);
            GridNode node = MovementGrid.instance.pathfindingGrid[accessX, accessY];
            node.isWalkable = false;
            
            wallTilemap.SetTile(tilePos, straightTileSides);
        }
    }

}
