using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Room
{
    public bool isExplored;
    public Vector2 position;

    private bool bIntersection = false;
    private bool lIntersection = false;
    private bool tIntersection = false;
    private bool rIntersection = false;

    public GameObject preset;

    public List<GameObject> enemyList;
    public List<Vector3Int> blockingCoords;

    public Tilemap roomTilemap;

    public Room(bool isExplored, Vector2 position, bool bi, bool li, bool ti, bool ri)
    {
        enemyList = new List<GameObject>();
        this.isExplored = isExplored;
        this.position = position;

        bIntersection = bi;
        lIntersection = li;
        tIntersection = ti;
        rIntersection = ri;

        // Proceed further only if it's not a starting room.
        if(position != Vector2.zero)
        {
            // Getting preset from PresetManager.
            preset = PresetManager.instance.InstantiatePreset(li, ti, ri, bi, position);

            Transform presetTransform = preset.transform;
            for(int i = 0; i < presetTransform.childCount; ++i)
            {
                Transform child = presetTransform.GetChild(i);

                if(child.gameObject.name == "RoomGrid")
                {
                    roomTilemap = child.GetChild(0).GetComponent<Tilemap>();
                }

                if(child.tag == "Enemy")
                {
                    var randomUnit = child.GetComponent<RandomUnit>();
                    GameObject roomEnemy = randomUnit.GenerateRandomUnit();
                    enemyList.Add(roomEnemy.transform.GetChild(0).gameObject);

                    // Dispose of the questionmark.
                    randomUnit.Dispose();
                }
            }
        }

        blockingCoords = GetBlockingCoordinates();
    }

    private List<Vector3Int> GetBlockingCoordinates()
    {
        // List that we will return at the end
        List<Vector3Int> coordList = new List<Vector3Int>();

        int roomWidth = RoomManager.instance.roomWidth;
        int roomHeight = RoomManager.instance.roomHeight;
        int intersectionWidth = RoomManager.instance.intersectionWidth;

        if(bIntersection)
        {
            int startingY = (int)position.y - roomHeight;
            int startingX = (int)position.x - Mathf.FloorToInt(intersectionWidth/2);
            int endX = startingX + intersectionWidth;

            for(int x = startingX; x < endX; ++x)
            {
                Vector3Int coord = new Vector3Int(x, startingY, 0);
                coordList.Add(coord);
            }
        }
        if(tIntersection)
        {
            int startingY = (int)position.y + roomHeight;
            int startingX = (int)position.x - Mathf.FloorToInt(intersectionWidth/2);
            int endX = startingX + intersectionWidth;

            for(int x = startingX; x < endX; ++x)
            {
                Vector3Int coord = new Vector3Int(x, startingY, 0);
                coordList.Add(coord);
            }
        }
        if(rIntersection)
        {
            int startingX = (int)position.x + roomWidth;
            int startingY = (int)position.y + Mathf.FloorToInt(intersectionWidth/2);
            int endY = startingY - intersectionWidth;

            for(int y = startingY; y > endY; --y)
            {
                Vector3Int coord = new Vector3Int(startingX, y, 0);
                coordList.Add(coord);
            }
        }
        if(lIntersection)
        {
            int startingX = (int)position.x - roomWidth;
            int startingY = (int)position.y + Mathf.FloorToInt(intersectionWidth/2);
            int endY = startingY - intersectionWidth;

            for(int y = startingY; y > endY; --y)
            {
                Vector3Int coord = new Vector3Int(startingX, y, 0);
                coordList.Add(coord);
            }
        }

        return coordList;
    }
    
}