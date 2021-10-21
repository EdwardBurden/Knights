using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(menuName = "Wall")]
public class WallTile : Tile
{
	/*private const float SMALL_WALL_CHANCE = 0.1f;
	private const float MID_WALL_CHANCE = 0.4f;
	private const float HIGH_WALL_CHANCE = 0.8f;

	public Sprite test;

	public Sprite[] cornerSprites;

	public Sprite[] tr_sprites;
	public Sprite[] tl_sprites;
	public Sprite[] br_sprites;
	public Sprite[] bl_sprites;

	public override void RefreshTile(Vector3Int location, ITilemap tilemap)
	{
		for (int yd = -1; yd <= 1; yd++)
		{
			for (int xd = -1; xd <= 1; xd++)
			{
				Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, 0);
				if (Helper.TileExistsInTileMap((Vector2Int)location, tilemap, this, 10)) ;
					tilemap.RefreshTile(position);
			}
		}
	}

	public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
	{
		int mask = GetMask(location);
	
		Sprite sprite = GetSprite((byte)mask);
		if (sprite != null)
		{
			tileData.sprite = sprite;
			tileData.color = Color.white;
			var m = tileData.transform;
			//set to y pos + 0 if top wall,+ 1 if bottom wall
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
				mask += tileMapGrid.HasFloorTile(new Vector3Int(location.x, location.y, 0) + new Vector3Int(xd, yd, 0)) ? binary : 0;
				binary *= 2;
			}
		}
		return mask;

	}

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



			default: return test; //null
								  //default: return 1; //Temp for now blue floor to show is wall
		}

	}

	private Sprite SelectWallSprite(Sprite[] sprites)
	{
		return sprites[0]; //implement random using griddata seed  later 

	}
	*/
}