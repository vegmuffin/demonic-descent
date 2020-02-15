using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementGrid : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private Tile gridTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap grid;
    [Space]
    [SerializeField] private int gridLength;
    [SerializeField] private Color mouseTintColor;

    private GameObject crosshair;
    private Vector3Int tempTilePos;

    // Start is called before the first frame update
    void Start()
    {
        crosshair = GameObject.Find("Crosshair");
    }

    // Update is called once per frame
    void Update()
    {
        MouseInGrid();
        if(Input.GetKeyDown(KeyCode.G))
        {
            Vector3Int crosshairPos = new Vector3Int((int)Mathf.Floor(crosshair.transform.position.x), (int)Mathf.Floor(crosshair.transform.position.y), 0);
            GenerateGrid(crosshairPos);
        }
    }

    // Paints the tile on which the mouse is hovering.
    private void MouseInGrid()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.z));
        Vector3Int precisePos = new Vector3Int((int)Mathf.Ceil(mousePos.x)*-1, (int)Mathf.Ceil(mousePos.y-2f)*-1, 0);
        if(groundTilemap.HasTile(precisePos))
        {
            groundTilemap.SetColor(tempTilePos, Color.white);
            groundTilemap.SetColor(precisePos, mouseTintColor);
            tempTilePos = precisePos;
        }
    }

    // Diamond-shape grid generation.
    private void GenerateGrid(Vector3Int coord)
    {
        // Clearing previous tiles
        grid.ClearAllTiles();

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
                        grid.SetTile(tilePos, gridTile);
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
                            grid.SetTile(tilePos, gridTile);
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
                        grid.SetTile(tilePos, gridTile);
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
                            grid.SetTile(tilePos, gridTile);
                }
            }
            tileCount -= 2;
        }
    }

    // A* Pathfinding TODO
    private List<Vector3Int> Pathfinding(Vector3Int coord)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        
        
        return path;
    }
}
