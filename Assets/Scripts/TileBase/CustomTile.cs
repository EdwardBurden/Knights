using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

[CreateAssetMenu(menuName = "Custom")]
public class CustomTile : TileBase
{
	public Sprite sprite;
	public GameObject m_Preview;


#if UNITY_EDITOR
	// The following is a helper that adds a menu item to create a CustomTile Asset
	[MenuItem("Assets/Create/RoadTile")]
	public static void CreateRoadTile()
	{
		string path = EditorUtility.SaveFilePanelInProject("Save Road Tile", "New Road Tile", "Asset", "Save Road Tile", "Assets");
		if (path == "")
			return;
		AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CustomTile>(), path);
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		Debug.Log("GetTileAnimationData");
		return base.GetTileAnimationData(position, tilemap, ref tileAnimationData);
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		tileData.sprite = sprite;
		tileData.gameObject = m_Preview;
		tileData.color = Color.white;
		tileData.flags = TileFlags.LockTransform;
		tileData.colliderType = ColliderType.None;
	}

	public override void RefreshTile(Vector3Int position, ITilemap tilemap)
	{
		Debug.Log("RefreshTile");
		base.RefreshTile(position, tilemap);
	}

	public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
	{
		Debug.Log("StartUp");
		return base.StartUp(position, tilemap, go);
	}
#endif


}
