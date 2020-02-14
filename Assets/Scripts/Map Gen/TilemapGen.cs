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
    [SerializeField] private Tile gridTile;
    [Space]
    [SerializeField] private int leftBound;
    [SerializeField] private int rightBound;
    [SerializeField] private int topBound;
    [SerializeField] private int bottomBound;


    private void Awake()
    {
        instance = this;
    }

    public void Generate(List<Vector2> map)
    {
        foreach(Vector2 coord in map)
        {
            Vector2Int expandedCoord = new Vector2Int((int)coord.x*(rightBound+offsetBetweenRooms)*2, (int)coord.y*(topBound+offsetBetweenRooms)*2);
            GenerateGround(expandedCoord);
            GenerateWalls(expandedCoord);
            GenerateIntersections(expandedCoord);
        }
    }

    private void GenerateGround(Vector2Int coord)
    {
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
                transparentTilemap.SetTile(tilePos, gridTile);
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

    private void GenerateIntersections(Vector2 coord)
    {
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

    // Ufff I'm not proud of this... Will change if the intersection size will be a variable.
    private void Intersection(Vector3Int coord, string where)
    {
        int stepLength = (int)Mathf.Floor(offsetBetweenRooms/2);
        int stepWidth = (int)Mathf.Floor(intersectionWidth/2);
        int doubleStepWidth = stepWidth*2-1; // I have to get better at naming.

        // Left / Right intersection generation.
        if(where == "left" || where == "right")
        {
            for(int x = coord.x-offsetBetweenRooms; x <= coord.x+offsetBetweenRooms; ++x)
            {
                for(int y = coord.y-stepWidth; y <= coord.y+stepWidth; ++y)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    groundTilemap.SetTile(tilePos, groundTile);
                    transparentTilemap.SetTile(tilePos, gridTile);

                    if(x == coord.x-offsetBetweenRooms || x == coord.x+offsetBetweenRooms)
                    {
                        wallTilemap.SetTile(tilePos, null);
                    }
                }
            }

            for(int i = coord.x-offsetBetweenRooms; i <= coord.x+offsetBetweenRooms; ++i)
            {
                wallTilemap.SetTile(new Vector3Int(i, coord.y+stepWidth, 0), straightTileSides);
                wallTilemap.SetTile(new Vector3Int(i, coord.y-stepWidth, 0), straightTileSides);
                if(i == coord.x+offsetBetweenRooms)
                {
                    Vector3Int tilePos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y-stepWidth, 0);
                    wallTilemap.SetTile(tilePos, cornerTileTop);
                    groundTilemap.SetTile(tilePos, groundTile);

                    tilePos = new Vector3Int(coord.x+offsetBetweenRooms, coord.y-stepWidth, 0);
                    wallTilemap.SetTile(tilePos, cornerTileTop);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
                    wallTilemap.SetTransformMatrix(tilePos, matrix);
                    groundTilemap.SetTile(tilePos, groundTile);

                    tilePos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y+stepWidth, 0);
                    wallTilemap.SetTile(tilePos, cornerTileSides);

                    tilePos = new Vector3Int(coord.x+offsetBetweenRooms, coord.y+stepWidth, 0);
                    wallTilemap.SetTile(tilePos, cornerTileSides);
                    matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
                    wallTilemap.SetTransformMatrix(tilePos, matrix);
                }
            }               

        } 
        // Top / Bottom intersection generation.
        else if(where == "bottom" || where == "top")
        {
            for(int x = coord.x-stepWidth; x <= coord.x+stepWidth; ++x)
            {
                for(int y = coord.y-offsetBetweenRooms; y <= coord.y+offsetBetweenRooms; ++y)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    groundTilemap.SetTile(tilePos, groundTile);
                    transparentTilemap.SetTile(tilePos, gridTile);

                    if(y == coord.y-offsetBetweenRooms || y == coord.y+offsetBetweenRooms)
                    {
                        wallTilemap.SetTile(tilePos, null);
                    }
                }
            }
            for(int i = coord.y-offsetBetweenRooms; i <= coord.y+offsetBetweenRooms; ++i)
            {
                wallTilemap.SetTile(new Vector3Int(coord.x+stepWidth, i, 0), straightTileTop);
                wallTilemap.SetTile(new Vector3Int(coord.x-stepWidth, i, 0), straightTileTop);
                if(i == coord.y+offsetBetweenRooms)
                {
                    Vector3Int tilePos = new Vector3Int(coord.x-stepWidth, coord.y-offsetBetweenRooms, 0);
                    wallTilemap.SetTile(tilePos, cornerTileSides);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
                    wallTilemap.SetTransformMatrix(tilePos, matrix);

                    tilePos = new Vector3Int(coord.x+stepWidth, coord.y-offsetBetweenRooms, 0);
                    wallTilemap.SetTile(tilePos, cornerTileSides);

                    tilePos = new Vector3Int(coord.x-stepWidth, coord.y+offsetBetweenRooms, 0);
                    wallTilemap.SetTile(tilePos, cornerTileTop);
                    matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 180f, 0)), Vector3.one);
                    wallTilemap.SetTransformMatrix(tilePos, matrix);
                    groundTilemap.SetTile(tilePos, groundTile);

                    tilePos = new Vector3Int(coord.x+stepWidth, coord.y+offsetBetweenRooms, 0);
                    wallTilemap.SetTile(tilePos, cornerTileTop);
                    groundTilemap.SetTile(tilePos, groundTile);
                }
            }
            
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

}
