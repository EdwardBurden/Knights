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

	public Sprite[] cornerSprites;

	public Sprite[] tr_sprites;
	public Sprite[] tl_sprites;
	public Sprite[] br_sprites;
	public Sprite[] bl_sprites;

	//public Sprite[] m_Sprites;
	//public Sprite m_Preview;
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
		//REMOVE THIS ONKLY FOR TESTING



		Sprite sprite = GetSprite((byte)mask);
		if (sprite != null)
		{
			tileData.sprite = sprite;
			tileData.color = Color.white;
			var m = tileData.transform;
			//m.SetTRS(Vector3.zero, GetRotation((byte)mask), Vector3.one);
			tileData.transform = m;
			tileData.flags = TileFlags.LockTransform;
			tileData.colliderType = ColliderType.None;
		}
		else
		{
			Debug.LogWarning("Missing Sprite");
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
	private Sprite GetSprite(byte mask)
	{

		switch (mask)
		{
			case 7:
			case 6:
			case 3:
				return SelectWallSprite(tl_sprites); //TopLeft

			case 224:
			case 192:
			case 96:
				return SelectWallSprite(br_sprites); ; //BottomRight
			case 41:
			case 40:
			case 9:
				return SelectWallSprite(tr_sprites); ; //TopRight
			case 148:
			case 144:
			case 20:
				return SelectWallSprite(bl_sprites); ; //BottomLeft

			case 1:
				return cornerSprites[0]; //Top Corner

			case 32:
				return cornerSprites[1]; //Corner

			case 128:
				return cornerSprites[2]; //Corner

			case 4:
				return cornerSprites[3]; //Corner



			default: return null; //null
							   //default: return 1; //Temp for now blue floor to show is wall
		}

	}

	private Sprite SelectWallSprite(Sprite[] sprites)
	{
		return sprites[0]; //implement random using griddata seed  later 

	}
}