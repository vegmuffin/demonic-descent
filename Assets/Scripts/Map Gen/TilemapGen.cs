﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGen : MonoBehaviour
{
    [HideInInspector] public static TilemapGen instance = default;
    
    [SerializeField] private Tilemap wallTilemap = default;
    [SerializeField] private Tilemap groundTilemap = default;
    [SerializeField] private Tilemap transparentTilemap = default;
    [Space]
    [SerializeField] private Tile straightTileTop = default;
    [SerializeField] private Tile straightTileSides = default;
    [SerializeField] private Tile cornerTileSides = default;
    [SerializeField] private Tile cornerTileTop = default;
    [SerializeField] private Tile groundTile = default;
    [SerializeField] private Tile transparentTile = default;
    [Space]
    private int roomWidth;
    private int roomHeight;
    private int offsetBetweenRooms;
    private int intersectionWidth;

    private int testX;
    private int testY;

    private void Awake()
    {
        instance = this;
        roomWidth = RoomManager.instance.roomWidth;
        roomHeight = RoomManager.instance.roomHeight;
        offsetBetweenRooms = RoomManager.instance.offsetBetweenRooms;
        intersectionWidth = RoomManager.instance.intersectionWidth;
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
            Vector2Int expandedCoord = new Vector2Int((int)coord.x*(roomWidth+offsetBetweenRooms)*2, (int)coord.y*(roomHeight+offsetBetweenRooms)*2);

            Room room = GenerateRoom(coord, expandedCoord, map);

            if(expandedCoord.x == 0 && expandedCoord.y == 0)
                RoomManager.instance.currentRoom = room;

            // For better management, lets generate different tilemap parts separately.
            GenerateGround(expandedCoord);
            GenerateWalls(expandedCoord);
            GenerateIntersections(expandedCoord);

            // Expanded coordinate bounds. Got a lot of null reference errors before when hovering above a non-existent grid, so 50 is just extending the grid margins.
            int expCoordMinX = expandedCoord.x - roomWidth - 50;
            int expCoordMinY = expandedCoord.y - roomHeight - 50;
            int expCoordMaxX = expandedCoord.x + roomWidth + 50;
            int expCoordMaxY = expandedCoord.x + roomHeight + 50;

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

        MovementManager.instance.PopulateGrid(startX, startY, endX, endY);
    }

    private void GenerateGround(Vector2Int coord)
    {
        // Simple square grid generation.
        int startX = coord.x - roomWidth + 1;
        int startY = coord.y - roomHeight + 1;
        int endX = coord.x + roomWidth;
        int endY = coord.y + roomHeight;
        for(int x = startX; x < endX; ++x)
        {
            for(int y = startY; y < endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                Tile tile = Tiles.instance.GetGroundTile();
                groundTilemap.SetTile(tilePos, tile);
                transparentTilemap.SetTile(tilePos, transparentTile);
            }
        }
    }

    // Primary function for generating all wall tiles.
    private void GenerateWalls(Vector2Int coord)
    {
        int startX = coord.x - roomWidth;
        int startY = coord.y - roomHeight;
        int endX = coord.x + roomWidth;
        int endY = coord.y + roomHeight;
        for(int x = startX; x <= endX; ++x)
        {
            for(int y = startY; y <= endY; ++y)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // There are edge cases where we have to paint corner tiles instead.
                if(x == startX || x == endX)
                {
                    Tile tile = CornerTile(x, y, startX, startY, endX, endY);

                    if(!tile && y != endY)
                    {
                        if(x == startX)
                            tile = Tiles.instance.GetLeftWall();
                        else if(x == endX)
                            tile = Tiles.instance.GetRightWall();
                    }
                    else if(y == endY)
                    {
                        if(x == startX)
                            wallTilemap.SetTile(tilePos, Tiles.instance.GetLeftWall());
                        else if(x == endX)
                            wallTilemap.SetTile(tilePos, Tiles.instance.GetRightWall());

                        tilePos = new Vector3Int(tilePos.x, tilePos.y+1, 0);
                        wallTilemap.SetTile(tilePos, Tiles.instance.GetBrickWall());
                    }

                    wallTilemap.SetTile(tilePos, tile);
                } 
                else if(y == startY || y == endY)
                {
                    Vector3Int tilePosBottom = new Vector3Int(x, startY, 0);
                    Vector3Int tilePosTop = new Vector3Int(x, endY+1, 0);

                    wallTilemap.SetTile(tilePosBottom, Tiles.instance.GetBottomWall());
                    wallTilemap.SetTile(tilePosTop, Tiles.instance.GetTopWall());

                    tilePosTop = new Vector3Int(x, endY, 0);
                    wallTilemap.SetTile(tilePosTop, Tiles.instance.GetBrickWall());

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
        Vector3Int intersectionCoord = new Vector3Int((int)coord.x-roomWidth-offsetBetweenRooms, (int)coord.y, 0);
        Vector3Int checkCoord = new Vector3Int(intersectionCoord.x-offsetBetweenRooms, intersectionCoord.y, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "left");

        // Right
        intersectionCoord = new Vector3Int((int)coord.x+roomWidth+offsetBetweenRooms, (int)coord.y, 0);
        checkCoord = new Vector3Int(intersectionCoord.x+offsetBetweenRooms, intersectionCoord.y, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "right");

        // Bottom
        intersectionCoord = new Vector3Int((int)coord.x, (int)coord.y-roomHeight-offsetBetweenRooms, 0);
        checkCoord = new Vector3Int(intersectionCoord.x, intersectionCoord.y-offsetBetweenRooms, 0);
        if(wallTilemap.HasTile(checkCoord) && !groundTilemap.HasTile(intersectionCoord))
            Intersection(intersectionCoord, "bottom");

        // Top
        intersectionCoord = new Vector3Int((int)coord.x, (int)coord.y+roomHeight+offsetBetweenRooms, 0);
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
                    Tile tile = Tiles.instance.GetGroundTile();
                    Vector3Int groundTilePos = new Vector3Int(x, y, 0);
                    groundTilemap.SetTile(groundTilePos, tile);
                    transparentTilemap.SetTile(groundTilePos, transparentTile);

                    // Eliminating walls that were previously generated. Easier to do than putting more confusing code into wall tilemap generation.
                    if(x == coord.x-offsetBetweenRooms || x == coord.x+offsetBetweenRooms)
                        wallTilemap.SetTile(groundTilePos, null);
                }
            }

            // Painting walls on the side of the intersection for better immersion.
            for(int i = coord.x-offsetBetweenRooms; i <= coord.x+offsetBetweenRooms; ++i)
            {
                Tile topTile = Tiles.instance.GetTopWall();
                Tile botTile = Tiles.instance.GetBottomWall();
                wallTilemap.SetTile(new Vector3Int(i, coord.y+stepWidth+1, 0), topTile);
                wallTilemap.SetTile(new Vector3Int(i, coord.y-stepWidth-1, 0), botTile);
            }

            // Edge cases for corners.

            // Bottom left
            Vector3Int tilePos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y-stepWidth-1, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetBottomLeftCorner(false));

            // Bottom right
            tilePos = new Vector3Int(coord.x+offsetBetweenRooms, coord.y-stepWidth-1, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetBottomRightCorner(false));

            // Top left
            tilePos = new Vector3Int(coord.x-offsetBetweenRooms, coord.y+stepWidth+1, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetTopLeftCorner(false));

            // Top right
            tilePos = new Vector3Int(coord.x+offsetBetweenRooms, coord.y+stepWidth+1, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetTopRightCorner(false));           

        } 
        // Bottom / Top intersection generation. Everything is the same but flipped, since length becomes width and vice versa.
        else if(where == "bottom" || where == "top")
        {
            for(int x = coord.x-stepWidth; x <= coord.x+stepWidth; ++x)
            {
                for(int y = coord.y-offsetBetweenRooms; y <= coord.y+offsetBetweenRooms; ++y)
                {
                    Tile tile = Tiles.instance.GetGroundTile();
                    Vector3Int groundTilePos = new Vector3Int(x, y, 0);
                    groundTilemap.SetTile(groundTilePos, tile);
                    transparentTilemap.SetTile(groundTilePos, transparentTile);

                    // Have to account for brick wall as well.
                    if(y == coord.y-offsetBetweenRooms || y == coord.y-offsetBetweenRooms+1 || y == coord.y+offsetBetweenRooms)
                        wallTilemap.SetTile(groundTilePos, null);
                }
            }

            for(int i = coord.y-offsetBetweenRooms; i <= coord.y+offsetBetweenRooms; ++i)
            {
                Tile leftTile = Tiles.instance.GetLeftWall();
                Tile rightTile = Tiles.instance.GetRightWall();
                wallTilemap.SetTile(new Vector3Int(coord.x+stepWidth+1, i, 0), rightTile);
                wallTilemap.SetTile(new Vector3Int(coord.x-stepWidth-1, i, 0), leftTile);
            }

            Vector3Int tilePos = new Vector3Int(coord.x-stepWidth-1, coord.y-offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetBrickWall());
            tilePos = new Vector3Int(coord.x-stepWidth-1, coord.y-offsetBetweenRooms+1, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetTopRightCorner(false));

            tilePos = new Vector3Int(coord.x+stepWidth+1, coord.y-offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetBrickWall());
            tilePos = new Vector3Int(coord.x+stepWidth+1, coord.y-offsetBetweenRooms+1, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetTopLeftCorner(false));

            tilePos = new Vector3Int(coord.x-stepWidth-1, coord.y+offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetBottomRightCorner(false));

            tilePos = new Vector3Int(coord.x+stepWidth+1, coord.y+offsetBetweenRooms, 0);
            wallTilemap.SetTile(tilePos, Tiles.instance.GetBottomLeftCorner(false));
            
        }

    }

    // Checks if the tile should be a corner given the coordinates. Returns null if it isn't.
    private Tile CornerTile(int x, int y, int startX, int startY, int endX, int endY)
    {
        if(y == startY && x == startX)
            return Tiles.instance.GetBottomLeftCorner(true);
        else if(y == endY && x == startX)
            return Tiles.instance.GetTopLeftCorner(true);
        else if(y == endY && x == endX)
            return Tiles.instance.GetTopRightCorner(true);
        else if(y == startY && x == endX)
            return Tiles.instance.GetBottomRightCorner(true);
        else
            return null;
    }

    private Room GenerateRoom(Vector2 coord, Vector2 expandedCoord, List<Vector2> map)
    {
        // Checking for intersections.
        bool bi = false;
        bool li = false;
        bool ti = false;
        bool ri = false;
        Vector2[] neighbourRooms = new Vector2[4]{new Vector2(coord.x, coord.y-1), new Vector2(coord.x-1, coord.y), new Vector2(coord.x, coord.y+1), new Vector2(coord.x+1, coord.y)};
        foreach(Vector2 newCoord in map)
        {
            if(newCoord == neighbourRooms[0] && !bi)
            {
                bi = true;
                continue;
            } else if(newCoord == neighbourRooms[1] && !li)
            {
                li = true;
                continue;
            } else if(newCoord == neighbourRooms[2] && !ti)
            {
                ti = true;
                continue;
            } else if(newCoord == neighbourRooms[3] && !ri)
            {
                ri = true;
                continue;
            }
        }

        Room room = new Room(false, expandedCoord, bi, li, ti, ri);
        RoomManager.instance.allRooms.Add(room);
        return room;
    }

}
