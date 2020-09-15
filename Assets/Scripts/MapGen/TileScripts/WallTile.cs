using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class WallTile : TileBase
{
    public Sprite[] sprites;
    public Sprite preview;

    [MenuItem("Assets/Create/CustomTiles/WallTile")]
    public static void CreateTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Tile", "New Tile", "Asset", "Save Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WallTile>(), path);
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        int index = GetIndex(tilemap, location);

        tileData.sprite = preview;
        if (index >= 0)
            tileData.sprite = sprites[index];

        tileData.color = Color.white;
        tileData.transform.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.zero);
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = Tile.ColliderType.None;
    }

    private int GetIndex(ITilemap tilemap, Vector3Int location)
    {
        int index = -1;

        // All possible directions
        Vector3Int bottom = new Vector3Int(0, -1, 0);
        Vector3Int left = new Vector3Int(-1, 0, 0);
        Vector3Int top = new Vector3Int(0, 1, 0);
        Vector3Int right = new Vector3Int(1, 0, 0);
        Vector3Int topleft = new Vector3Int(-1, 1, 0);
        Vector3Int botleft = new Vector3Int(-1, -1, 0);
        Vector3Int topright = new Vector3Int(1, 1, 0);
        Vector3Int botright = new Vector3Int(1, -1, 0);

        // Edge cases
        if (HasGroundTile(tilemap, location + top) && HasGroundTile(tilemap, location + right))
            index = 8;
        else if (HasGroundTile(tilemap, location + top) && HasGroundTile(tilemap, location + left))
            index = 9;
        else if (HasGroundTile(tilemap, location + bottom) && HasGroundTile(tilemap, location + left))
            index = 10;
        else if (HasGroundTile(tilemap, location + bottom) && HasGroundTile(tilemap, location + right))
            index = 11;

        if(index == -1)
        {
            if (HasGroundTile(tilemap, location + bottom))
                index = 1;
            else if (HasGroundTile(tilemap, location + left))
                index = 3;
            else if (HasGroundTile(tilemap, location + top))
                index = 5;
            else if (HasGroundTile(tilemap, location + right))
                index = 7;
            else if (HasGroundTile(tilemap, location + topleft))
                index = 4;
            else if (HasGroundTile(tilemap, location + botleft))
                index = 2;
            else if (HasGroundTile(tilemap, location + topright))
                index = 6;
            else if (HasGroundTile(tilemap, location + botright))
                index = 0;
        }

        return index;
    }

    private bool HasGroundTile(ITilemap tilemap, Vector3Int position)
    {
        TileBase t = tilemap.GetTile(position);
        if(t)
        {
            return IsTileOfType<GroundTile>(t);
        }
        return false;
    }

    private bool IsTileOfType<T>(TileBase targetTile) where T : TileBase
    {
        return targetTile is T;
    }
}
