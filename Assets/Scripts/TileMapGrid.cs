using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class TileMapGrid : MonoBehaviour
{
	public Tilemap floorTileMapPrefab;
	public Tilemap wallTileMapPrefab;
	public Tilemap colliderMapPrefab;

	public GridData gridData;
	private Dictionary<int, Tilemap> floorMaps = new Dictionary<int, Tilemap>();
	private Tilemap colliderMap;
	private Tilemap wallMap;
	private GridPalette gridPalette;
	public Vector3 GetStartRoomPosition()
	{
		Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
		Vector2Int center = Vector2Int.RoundToInt(gridData.startRoom.centerPosition);
		Vector3Int gridPosition = (Vector3Int)(startPos + center);
		Vector3 positon = floorMaps[gridData.startRoom.depth].CellToWorld(gridPosition);
		return positon;
	}

	public bool HasFloorTile(Vector3Int position)
	{
		foreach (KeyValuePair<int, Tilemap> tilemap in floorMaps.ToList())
		{
			if (tilemap.Value.GetTile(position) == gridPalette.roomTile)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCorridorTile(Vector3Int position)
	{
		foreach (KeyValuePair<int, Tilemap> tilemap in floorMaps.ToList())
		{
			if (tilemap.Value.GetTile(position) == gridPalette.corridorTile)
			{
				return true;
			}
		}
		return false;
	}

	public void LoadGrid(GridData data)
	{
		gridData = data;
		Random.InitState(gridData.seed);
		floorMaps.Clear();
		var tempList = transform.Cast<Transform>().ToList();
		foreach (Transform map in tempList)
		{
#if UNITY_EDITOR
			DestroyImmediate(map.gameObject);
#else
	Destroy(map.gameObject);
#endif
		}
		gridPalette = Resources.Load<GridPalette>(data.paletteId);
		CreateFloorMaps();

		colliderMap = CreateColliderMap(0);
		wallMap = CreateWallMap(0);
		DrawCorridors();
		DrawRooms();



		foreach (KeyValuePair<int, Tilemap> tilemap in floorMaps.ToList())
		{
			tilemap.Value.RefreshAllTiles();
		}
		wallMap.RefreshAllTiles();
	}

	private void CreateFloorMaps()
	{
		foreach (RoomData room in gridData.rooms)
		{
			if (!floorMaps.TryGetValue(room.depth, out Tilemap tilemap))
			{
				tilemap = CreateFloorMap(room.depth);
				floorMaps.Add(room.depth, tilemap);
			}
		}
	}

	private void DrawCorridors()
	{
		foreach (CorridorData corridor in gridData.corridors)
		{
			Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
			RoomData startRoom = gridData.GetRoom(corridor.startRoomID);
			RoomData endRoom = gridData.GetRoom(corridor.endRoomID);
			floorMaps.TryGetValue(startRoom.depth, out Tilemap corridorMap);
			foreach (Vector2 position in corridor.corridorPositions)
			{
				AddCorridorToTile(corridorMap, Vector2Int.RoundToInt(position));
			}
		}
	}

	private void AddCorridorToTile(Tilemap corridorMap, Vector2Int position)
	{
		Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
		Vector3Int gridPosition = (Vector3Int)(position + startPos);

		if (!TileExistsOnAnyDepth(floorMaps, gridPosition))
		{
			corridorMap.SetTile(gridPosition, gridPalette.corridorTile);
		}
		AddSurroundingCollider(gridPosition);
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
					bool exist = TileExistsOnAnyDepth(floorMaps, gridPosition);
					if (!exist)
					{
						colliderMap.SetTile(gridPosition, gridPalette.colliderTile);
					}
					else
					{
						colliderMap.SetTile(gridPosition, null);
					}
				}
			}
		}
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
		foreach (RoomData room in gridData.rooms)
		{
			floorMaps.TryGetValue(room.depth, out Tilemap tilemap);
			DrawRoom(tilemap, room);
		}
	}

	private void DrawRoom(Tilemap tilemap, RoomData room)
	{
		for (int x = -1; x < room.width + 1; x++)
		{
			for (int y = -1; y < room.height + 1; y++)
			{
				Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(startPos[0] + x, startPos[1] + y));

				if (!(x == -1 || y == -1 || x == room.width || y == room.height))
				{
					tilemap.SetTile(gridPosition, gridPalette.roomTile);
					if (TileExistsOnAnyDepth(floorMaps, gridPosition))
					{
						colliderMap.SetTile(gridPosition, null);
					}
					if ((x == 0 || y == 0 || x == room.width - 1 || y == room.height - 1))
					{
						wallMap.SetTile(gridPosition, gridPalette.wallTile);
					}
					//colour
					if (room.isMain)
					{
						tilemap.SetColor(gridPosition, Color.blue);
					}
					if (room.isStart)
					{
						tilemap.SetColor(gridPosition, Color.red);
					}

					if (room.isEnd)
					{
						tilemap.SetColor(gridPosition, Color.yellow);
					}
					//colour
				}
				else if (!TileExistsOnAnyDepth(floorMaps, gridPosition))
				{
					colliderMap.SetTile(gridPosition, gridPalette.colliderTile);
				}
			}
		}
	}

	private Tilemap CreateFloorMap(int order)
	{
		Tilemap tilemap = Instantiate(floorTileMapPrefab, this.transform);
		tilemap.name = "Floor_" + order.ToString();
		//tilemap.GetComponent<TilemapRenderer>().sortingOrder = order;
		//tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

	private Tilemap CreateWallMap(int order)
	{
		Tilemap tilemap = Instantiate(wallTileMapPrefab, this.transform);
		tilemap.name = "Wall_" + order.ToString();
		//tilemap.GetComponent<TilemapRenderer>().sortingOrder = order;
		//tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

	private Tilemap CreateColliderMap(int order)
	{
		Tilemap tilemap = Instantiate(colliderMapPrefab, this.transform);
		tilemap.name = "Collider_" + order.ToString();
		//tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

}
