using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGen : MonoBehaviour
{
    [HideInInspector] public static TilemapGen instance;
    [SerializeField] private int offsetBetweenRooms;
    [Space]
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [Space]
    [SerializeField] private Tile straightTileTop;
    [SerializeField] private Tile straightTileSides;
    [SerializeField] private Tile cornerTileSides;
    [SerializeField] private Tile cornerTileTop;
    [SerializeField] private Tile groundTile;
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
        for(int x = coord.x-1; x <= coord.x+1; ++x)
        {
            for(int y = coord.y-1; y <= coord.y+1; ++y)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }
        
        if(where == "left" || where == "right")
        {
            Vector3Int pos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y, 0);
            Vector3Int minusPos = new Vector3Int(pos.x, pos.y-1, 0);
            Vector3Int plusPos = new Vector3Int(pos.x, pos.y+1, 0);
            Vector3Int pos2 = new Vector3Int(coord.x+offsetBetweenRooms, coord.y, 0);
            Vector3Int minusPos2 = new Vector3Int(pos2.x, pos.y-1, 0);
            Vector3Int plusPos2 = new Vector3Int(pos2.x, pos.y+1, 0);
            wallTilemap.SetTile(pos, null);
            wallTilemap.SetTile(minusPos, null);
            wallTilemap.SetTile(plusPos, null);
            wallTilemap.SetTile(pos2, null);
            wallTilemap.SetTile(minusPos2, null);
            wallTilemap.SetTile(plusPos2, null);
            groundTilemap.SetTile(pos, groundTile);
            groundTilemap.SetTile(minusPos, groundTile);
            groundTilemap.SetTile(plusPos, groundTile);
            groundTilemap.SetTile(pos2, groundTile);
            groundTilemap.SetTile(minusPos2, groundTile);
            groundTilemap.SetTile(plusPos2, groundTile);            
        } else if(where == "bottom" || where == "top")
        {
            Vector3Int pos = new Vector3Int(coord.x, coord.y-offsetBetweenRooms, 0);
            Vector3Int minusPos = new Vector3Int(pos.x-1, pos.y, 0);
            Vector3Int plusPos = new Vector3Int(pos.x+1, pos.y, 0);
            Vector3Int pos2 = new Vector3Int(coord.x, coord.y+offsetBetweenRooms, 0);
            Vector3Int minusPos2 = new Vector3Int(pos2.x-1, pos2.y, 0);
            Vector3Int plusPos2 = new Vector3Int(pos2.x+1, pos2.y, 0);
            wallTilemap.SetTile(pos, null);
            wallTilemap.SetTile(minusPos, null);
            wallTilemap.SetTile(plusPos, null);
            wallTilemap.SetTile(pos2, null);
            wallTilemap.SetTile(minusPos2, null);
            wallTilemap.SetTile(plusPos2, null);
            groundTilemap.SetTile(pos, groundTile);
            groundTilemap.SetTile(minusPos, groundTile);
            groundTilemap.SetTile(plusPos, groundTile);
            groundTilemap.SetTile(pos2, groundTile);
            groundTilemap.SetTile(minusPos2, groundTile);
            groundTilemap.SetTile(plusPos2, groundTile);
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
