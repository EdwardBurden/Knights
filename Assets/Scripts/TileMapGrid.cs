using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;

[ExecuteInEditMode]
public class TileMapGrid : MonoBehaviour
{
	public TileBase roomTile;
	public TileBase corridorTile;
	public TileBase colliderTile;
	public Tilemap tileMapPrefab;
	public Tilemap colliderMapPrefab;

	private GridData gridData;
	private Dictionary<int, Tilemap> depthTilemaps = new Dictionary<int, Tilemap>();
	private Tilemap colliderMap;
	private IEnumerator coroutine;

	public void LoadGrid(GridData gridData)
	{
		this.gridData = gridData;
		depthTilemaps.Clear();
		var tempList = transform.Cast<Transform>().ToList();
		foreach (Transform map in tempList)
		{
#if UNITY_EDITOR
			DestroyImmediate(map.gameObject);
#else
	Destroy(map.gameObject);
#endif
		}

		colliderMap = Instantiate(colliderMapPrefab, this.transform);
		colliderMap.ClearAllTiles();
		foreach (TileMapRoom room in gridData.rooms)
		{
			if (!depthTilemaps.TryGetValue(room.depth, out Tilemap tilemap))
			{
				tilemap = CreateNewTileMap(room.depth);
				depthTilemaps.Add(room.depth, tilemap);
			}
			AddRoomToTileMap(tilemap, room, colliderMap);
		}

		DrawCorridors();
		foreach (KeyValuePair<int, Tilemap> tilemap in depthTilemaps.ToList())
		{
			tilemap.Value.RefreshAllTiles();
		}
	}



	private void DrawCorridors()
	{
		foreach (TileMapRoom room in gridData.rooms)
		{
			Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
			Vector2Int roomCenterInt = new Vector2Int((int)room.centerPosition.x, (int)room.centerPosition.y);
			Vector3Int gridPosition = (Vector3Int)(roomCenterInt + new Vector2Int(startPos[0], startPos[1]));

			foreach (var item in room.connectedRooms)
			{
				TileMapRoom destination = gridData.rooms.FirstOrDefault(rooom => rooom.roomID == item);

				if (!depthTilemaps.TryGetValue(room.depth, out Tilemap corridorMap))
				{
					corridorMap = CreateNewTileMap(room.depth);
					depthTilemaps.Add(room.depth, corridorMap);
				}


				Vector2 differnce = destination.centerPosition - room.centerPosition;
				Vector2Int differnceInt = Vector2Int.RoundToInt(differnce);
				int xDirection = 0;
				if (differnceInt.x != 0)
				{
					xDirection = differnceInt.x > 0 ? 1 : -1;
				}
				int yDirection = 0;
				if (differnceInt.y != 0)
				{
					yDirection = differnceInt.y > 0 ? 1 : -1;
				}

				Vector2Int direction = new Vector2Int(xDirection, yDirection);

				for (int i = 0; i < (int)Mathf.Abs(differnceInt.x); i++)
				{
					Vector2Int posisionyy = new Vector2Int((int)(room.centerPosition.x + (i * direction.x)), (int)(room.centerPosition.y));
					Vector3Int gridposss = (Vector3Int)(posisionyy + new Vector2Int(startPos[0], startPos[1]));
					if (!TileExistsOnAnyDepth(gridposss))
					{
						corridorMap.SetTile(gridposss, corridorTile);
					}
				}

				for (int j = 0; j < (int)Mathf.Abs(differnceInt.y); j++)
				{
					Vector2Int posisionyy = new Vector2Int((int)(room.centerPosition.x + differnceInt.x), (int)(room.centerPosition.y + (j * direction.y)));
					Vector3Int gridposss = (Vector3Int)(posisionyy + new Vector2Int(startPos[0], startPos[1]));
					if (!TileExistsOnAnyDepth(gridposss))
					{
						corridorMap.SetTile(gridposss, corridorTile);
					}
				}
				corridorMap.RefreshAllTiles();
			}

		}
	}

	private bool TileExistsOnAnyDepth(Vector3Int position)
	{
		foreach (KeyValuePair<int, Tilemap> tilemap in depthTilemaps.ToList())
		{
			if (tilemap.Value.HasTile(position))
			{
				return true;
			}
		}
		return false;
	}

	private void AddRoomToTileMap(Tilemap tilemap, TileMapRoom room, Tilemap collider)
	{
		Color color = new Color((float)room.depth / (float)gridData.mapDepth, 0, 0);
		//Color color = new Color(room.connectedRooms.Count, 0, 0);
		//color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1, 1);
		if (room.isMain)
		{
			color = Color.gray;
		}

		for (int x = 0; x < room.width; x++)
		{
			for (int y = 0; y < room.height; y++)
			{
				Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth); //look at tielbase creatign surrounding colliders on creation
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(startPos[0] + x, startPos[1] + y));

				tilemap.SetTile(gridPosition, roomTile);
				tilemap.SetTileFlags(gridPosition, TileFlags.None);
				tilemap.SetColor(gridPosition, color);
			}
		}
	}

	private Tilemap CreateNewTileMap(int order)
	{
		Tilemap tilemap = Instantiate(tileMapPrefab, this.transform);
		tilemap.name = order.ToString();
		tilemap.GetComponent<TilemapRenderer>().sortingOrder = order;
		tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

}
