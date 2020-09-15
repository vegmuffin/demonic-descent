using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class GroundTile : TileBase
{
    public Sprite preview;

    [MenuItem("Assets/Create/CustomTiles/GroundTile")]
    public static void CreateTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Tile", "New Tile", "Asset", "Save Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GroundTile>(), path);
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = preview;
        tileData.color = Color.white;
        tileData.transform.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.zero);
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = Tile.ColliderType.None;
    }
}
