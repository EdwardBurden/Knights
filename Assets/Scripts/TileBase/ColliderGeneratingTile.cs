using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ColliderGeneratingTile : Tile
{
	/*public TileBase roomCollider;
	private Tilemap _colliderMap;
	private Tilemap colliderMap
	{
		get
		{
			if (_colliderMap == null)
			{
				_colliderMap = GameObject.FindGameObjectWithTag("ColliderMap").GetComponent<Tilemap>();
			}
			return _colliderMap;
		}
	}


	public override void RefreshTile(Vector3Int position, ITilemap tilemap)
	{
		for (int yd = -1; yd <= 1; yd++)
		{
			for (int xd = -1; xd <= 1; xd++)
			{
				Vector3Int gridPosition = new Vector3Int(position.x + xd, position.y + yd, position.z);
				if (HasTile(tilemap, gridPosition))
				{
					base.RefreshTile(position, tilemap);
				}
			}
		}
	}

	private bool HasTile(ITilemap tilemap, Vector3Int position)
	{
		return tilemap.GetTile(position) != null;
	}

	private bool HasCollider(Vector3Int position)
	{
		return colliderMap.GetTile(position) != null;
	}

	private void AddCollider(Vector3Int position)
	{
		colliderMap.SetTile(position, roomCollider);
	}

	private void RemoveCollider(Vector3Int position)
	{
		colliderMap.SetTile(position, null);
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		for (int yd = -1; yd <= 1; yd++)
		{
			for (int xd = -1; xd <= 1; xd++)
			{
				Vector3Int gridPosition = new Vector3Int(position.x + xd, position.y + yd, position.z);
				if (HasTile(tilemap, gridPosition))
				{
					RemoveCollider(gridPosition);
				}
				else if (!HasCollider(gridPosition))
				{
					AddCollider(gridPosition);
				}

			}
		}
		base.GetTileData(position, tilemap, ref tileData);
	}
	*/
}
