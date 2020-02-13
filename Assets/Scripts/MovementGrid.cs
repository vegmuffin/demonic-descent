using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementGrid : MonoBehaviour
{
    [SerializeField] private Tile gridTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap grid;
    [Space]
    [SerializeField] private int gridLength;
    

    private GameObject crosshair;

    // Start is called before the first frame update
    void Start()
    {
        crosshair = GameObject.Find("Crosshair");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            Vector3Int crosshairPos = new Vector3Int((int)Mathf.Floor(crosshair.transform.position.x), (int)Mathf.Floor(crosshair.transform.position.y), 0);
            GenerateGrid(crosshairPos);
        }
    }

    // Diamond-shape grid generation.
    private void GenerateGrid(Vector3Int coord)
    {
        // Clearing previous tiles
        grid.ClearAllTiles();

        int maxGridLength = gridLength*2;
        List<int> possibleValues = new List<int>();
        for(int i = -gridLength; i < maxGridLength; ++i)
            possibleValues.Add(i);

        int counter = 1;
        for(int i = -gridLength; i <= 0; ++i)
        {
            if(counter == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y+gridLength, 0);
                if(!wallTilemap.HasTile(tilePos))
                    if(groundTilemap.HasTile(tilePos))
                        grid.SetTile(tilePos, gridTile);
            }      
            else
            {
                int step = (counter-1)/2;
                int yCoord = coord.y+Mathf.Abs(i);
                for(int j = -step; j <= step; ++j)
                {
                    Vector3Int tilePos = new Vector3Int(coord.x+j, yCoord, 0);
                    if(!wallTilemap.HasTile(tilePos))
                        if(groundTilemap.HasTile(tilePos))
                            grid.SetTile(tilePos, gridTile);
                }
            }
            
            counter += 2;
        }
        counter -= 4;
        for(int i = -1; i >= -gridLength; --i)
        {
            if(counter == 1)
            {
                Vector3Int tilePos = new Vector3Int(coord.x, coord.y-gridLength, 0);
                if(!wallTilemap.HasTile(tilePos))
                    if(groundTilemap.HasTile(tilePos))
                        grid.SetTile(tilePos, gridTile);
            }
            else
            {
                int step = (counter-1)/2;
                int yCoord = coord.y-Mathf.Abs(i);
                for(int j = -step; j <= step; ++j)
                {
                    Vector3Int tilePos = new Vector3Int(coord.x+j, yCoord, 0);
                    if(!wallTilemap.HasTile(tilePos))
                        if(groundTilemap.HasTile(tilePos))
                            grid.SetTile(tilePos, gridTile);
                }
            }
            counter -= 2;
        }
    }
}
