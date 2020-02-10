using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGen : MonoBehaviour
{
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

    private void Start()
    {
        GenerateGround();
        GenerateWalls();
    }

    private void GenerateGround()
    {
        for(int x = leftBound + 1; x < rightBound; ++x)
        {
            for(int y = bottomBound + 1; y < topBound; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(tilePos, groundTile);
            }
        }
    }

    // Primary function for generating all wall tiles.
    private void GenerateWalls()
    {
        for(int x = leftBound; x <= rightBound; ++x)
        {
            for(int y = bottomBound; y <= topBound; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                Tile whichTile = null;
                if(x == leftBound || x == rightBound)
                {
                    Vector3 rot = Vector3.zero;
                    bool corner = IsTileCorner(x, y, ref rot, ref whichTile);

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
                    Vector3Int tilePosBottom = new Vector3Int(x, bottomBound, 0);
                    Vector3Int tilePosTop = new Vector3Int(x, topBound, 0);

                    wallTilemap.SetTile(tilePosBottom, straightTileSides);
                    wallTilemap.SetTile(tilePosTop, straightTileSides);
                    break;
                }
            }
        }
    }

    // Checks if a tile at a given position should be a corner, returns a boolean as well as returns how it should be rotated.
    private bool IsTileCorner(int x, int y, ref Vector3 rot, ref Tile whichTile)
    {
        if(y == bottomBound && x == leftBound)
        {
            whichTile = cornerTileSides;
            return true;
        }
        else if(y == topBound && x == leftBound)
        {
            whichTile = cornerTileTop;
            return true;
        }
        else if(y == bottomBound && x == rightBound)
        {
            whichTile = cornerTileSides;
            rot = new Vector3(0, 180f, 0f);
            return true;
        }
        else if(y == topBound && x == rightBound)
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
