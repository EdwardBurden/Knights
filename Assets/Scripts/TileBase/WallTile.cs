using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(menuName = "Wall")]
public class WallTile : Tile
{
	private const float SMALL_WALL_CHANCE = 0.1f;
	private const float MID_WALL_CHANCE = 0.4f;
	private const float HIGH_WALL_CHANCE = 0.8f;

	public Sprite[] m_Sprites;
	public Sprite m_Preview;
	// This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
	public override void RefreshTile(Vector3Int location, ITilemap tilemap)
	{
		for (int yd = -1; yd <= 1; yd++)
		{
			for (int xd = -1; xd <= 1; xd++)
			{
				Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
				if (HasWallTile(tilemap, position))
					tilemap.RefreshTile(position);
			}
		}
	}
	// This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
	// As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
	public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
	{
		int mask = GetMask(location);
		int index = GetIndex((byte)mask);
		if (index >= 0 && index < m_Sprites.Length)
		{
			tileData.sprite = m_Sprites[index];
			tileData.color = Color.white;
			var m = tileData.transform;
			//m.SetTRS(Vector3.zero, GetRotation((byte)mask), Vector3.one);
			tileData.transform = m;
			tileData.flags = TileFlags.LockTransform;
			tileData.colliderType = ColliderType.None;
		}
		else
		{
			Debug.LogWarning("Not enough sprites in RoadTile instance");
		}
	}

	private int GetMask(Vector3Int location)
	{
		TileMapGrid tileMapGrid = FindObjectOfType<TileMapGrid>();
		int mask = 0;
		int binary = 1;
		for (int yd = -1; yd <= 1; yd++)
		{
			for (int xd = -1; xd <= 1; xd++)
			{
				if (xd == 0 && yd == 0)
				{
					continue;
				}
				Vector3Int position = location + new Vector3Int(xd, yd, 0);
				if (tileMapGrid.HasCorridorTile(position))
				{
					return 0; //temp for now
				}
				mask += tileMapGrid.HasFloorTile(location + new Vector3Int(xd, yd, 0)) ? binary : 0;
				binary *= 2;
			}
		}
		return mask;

	}

	// This determines if the Tile at the position is the same RoadTile.
	private bool HasWallTile(ITilemap tilemap, Vector3Int position)
	{
		return tilemap.GetTile(position) == this;
	}
	// The following determines which sprite to use based on the number of adjacent RoadTiles
	private int GetIndex(byte mask)
	{
		switch (mask)
		{
			case 208: return 1; //Bottom Corner
			case 104: return 1; //Right Corner
			case 22: return 1; //Left Corner
			case 11: return 1; //Top Corner
			case 0: return 2; //Invisible
			default: return 0; //remove
		}
		return -1;
	}
}