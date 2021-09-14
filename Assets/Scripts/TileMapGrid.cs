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
	private Dictionary<int, Tilemap> rendererDepthMaps = new Dictionary<int, Tilemap>();
	private Tilemap colliderMap;

	private IEnumerator coroutine;

	public void LoadGrid(GridData gridData)
	{
		this.gridData = gridData;
		rendererDepthMaps.Clear();
		var tempList = transform.Cast<Transform>().ToList();
		foreach (Transform map in tempList)
		{
#if UNITY_EDITOR
			DestroyImmediate(map.gameObject);
#else
	Destroy(map.gameObject);
#endif
		}
		colliderMap = CreateNewColliderDepthMap(0);
		DrawRooms();
		DrawCorridors();

		foreach (KeyValuePair<int, Tilemap> tilemap in rendererDepthMaps.ToList())
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

			foreach (int connectedRoom in room.connectedRooms)
			{
				rendererDepthMaps.TryGetValue(room.depth, out Tilemap corridorMap);
				TileMapRoom destination = gridData.rooms.FirstOrDefault(rooom => rooom.roomID == connectedRoom);
				DrawCorridor(room, destination, corridorMap, startPos);
			}
		}
	}

	private void DrawCorridor(TileMapRoom source, TileMapRoom destination, Tilemap corridorMap, Vector2Int startPos)
	{
		Vector2Int direction = GetCorridorDirectionFromRooms(source, destination, out Vector2Int difference);
		Vector2Int sourcePosition = Vector2Int.RoundToInt(source.centerPosition);



		for (int i = 0; i < (int)Mathf.Abs(difference.x); i++)
		{
			int x = (int)(sourcePosition.x + (i * direction.x));
			int y = (int)(sourcePosition.y);

			Vector2Int position = new Vector2Int(x, y);
			Vector3Int gridPosition = (Vector3Int)(position + new Vector2Int(startPos[0], startPos[1]));
			corridorMap.SetTile(gridPosition, corridorTile);
			AddSurroundingCollider(gridPosition);

		}

		for (int j = 0; j < (int)Mathf.Abs(difference.y); j++)
		{
			int x = (int)(sourcePosition.x + difference.x);
			int y = (int)(sourcePosition.y + (j * direction.y));

			Vector2Int position = new Vector2Int(x, y);
			Vector3Int gridPosition = (Vector3Int)(position + new Vector2Int(startPos[0], startPos[1]));
			corridorMap.SetTile(gridPosition, corridorTile);
			AddSurroundingCollider(gridPosition);
		}
	}

	private void AddSurroundingCollider(Vector3Int position)
	{
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (!(i == 0 && j == 0))
				{
					Vector3Int gridPosition = new Vector3Int(i, j, 0) + position;
					bool exist = TileExistsOnAnyDepth(rendererDepthMaps, gridPosition);
					if (!exist)
					{
						colliderMap.SetTile(gridPosition, colliderTile);

					}
					else
					{
						colliderMap.SetTile(gridPosition, null);
					}
				}
			}
		}

	}

	private Vector2Int GetCorridorDirectionFromRooms(TileMapRoom source, TileMapRoom destination, out Vector2Int difference)
	{
		Vector2Int sourcePosition = Vector2Int.RoundToInt(source.centerPosition);
		Vector2Int destinationPosition = Vector2Int.RoundToInt(destination.centerPosition);
		difference = destinationPosition - sourcePosition;
		int xDirection = 0;
		if (difference.x != 0)
		{
			xDirection = difference.x > 0 ? 1 : -1;
		}
		int yDirection = 0;
		if (difference.y != 0)
		{
			yDirection = difference.y > 0 ? 1 : -1;
		}

		return new Vector2Int(xDirection, yDirection);

	}

	private bool TileExistsOnAnyDepth(Dictionary<int, Tilemap> depths, Vector3Int position)
	{
		foreach (KeyValuePair<int, Tilemap> tilemap in depths.ToList())
		{
			if (tilemap.Value.GetTile(position) != null)
			{
				return true;
			}
		}
		return false;
	}

	private void DrawRooms()
	{
		foreach (TileMapRoom room in gridData.rooms)
		{
			if (!rendererDepthMaps.TryGetValue(room.depth, out Tilemap tilemap))
			{
				tilemap = CreateNewRendererDepthMap(room.depth);
				rendererDepthMaps.Add(room.depth, tilemap);

			}
			DrawRoom(tilemap, room);
		}

	}

	private void DrawRoom(Tilemap tilemap, TileMapRoom room)
	{
		for (int x = -1; x < room.width + 1; x++)
		{
			for (int y = -1; y < room.height + 1; y++)
			{
				Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(startPos[0] + x, startPos[1] + y));

				if (!(x == -1 || y == -1 || x == room.width || y == room.height))
				{
					tilemap.SetTile(gridPosition, roomTile);
					if (TileExistsOnAnyDepth(rendererDepthMaps, gridPosition))
					{
						colliderMap.SetTile(gridPosition, null);
					}
				}
				else if(!TileExistsOnAnyDepth(rendererDepthMaps, gridPosition))
				{	
					colliderMap.SetTile(gridPosition, colliderTile);
				}
			}
		}


	}



	private Tilemap CreateNewRendererDepthMap(int order)
	{
		Tilemap tilemap = Instantiate(tileMapPrefab, this.transform);
		tilemap.name = "Renderer_" + order.ToString();
		tilemap.GetComponent<TilemapRenderer>().sortingOrder = order;
		tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

	private Tilemap CreateNewColliderDepthMap(int order)
	{
		Tilemap tilemap = Instantiate(colliderMapPrefab, this.transform);
		tilemap.name = "Collider_" + order.ToString();
		tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

}
