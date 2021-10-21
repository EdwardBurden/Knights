using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;
using Random = UnityEngine.Random;
[Obsolete]
[ExecuteInEditMode]
public class TileMapGrid : MonoBehaviour
{
	public Tilemap floorTileMapPrefab;
	public Tilemap wallTileMapPrefab;
	public Tilemap colliderMapPrefab;
	public Tilemap backgroundMapPrefab;

	public GridData gridData;
	private Dictionary<int, Tilemap> floorMaps = new Dictionary<int, Tilemap>();
	private Tilemap colliderMap;
	private Tilemap wallMap;
	private Tilemap backgroundMap;
	private GridPalette gridPalette;

	private Dictionary<Vector3Int, int> wallPositonsToRoomIds = new Dictionary<Vector3Int, int>();

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

	public void DrawDoors()
	{
		List<Vector2Int> doorPositions = new List<Vector2Int>();
		foreach (var item in gridData.rooms)
		{
			doorPositions.AddRange(gridData.GetRoomDoorPositions(item.roomID));
		}
		doorPositions = doorPositions.Distinct().ToList();
		foreach (var item in doorPositions)
		{
			Vector2Int startPos = Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
			Vector3Int gridPosition = (Vector3Int)(item + startPos);
			wallMap.SetTile(gridPosition, gridPalette.doorTile);
		}


	}

	private void DrawBackground()
	{
		Vector2Int startPos = Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
		BoundsInt boundsInt = new BoundsInt(startPos.x, startPos.y, 0, gridData.mapWidth, gridData.mapHeight, 1);
		TileBase[] tileArray = new TileBase[boundsInt.size.x * boundsInt.size.y];
		for (int index = 0; index < tileArray.Length; index++)
		{
			tileArray[index] = gridPalette.voidTile;
		}

		backgroundMap.SetTilesBlock(boundsInt, tileArray);

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
		wallPositonsToRoomIds.Clear();
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
		backgroundMap = CreateBackgroundMap(0);


		DrawBackground();
		DrawRooms();
		DrawDoors();


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

	private void DrawCorridor(Tilemap tilemap, RoomData roomData)
	{
		foreach (CorridorData corridor in roomData.corridors)
		{
			Vector2Int startPos = Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
			RoomData startRoom = gridData.GetRoom(corridor.startRoomID);
			RoomData endRoom = gridData.GetRoom(corridor.endRoomID);
			foreach (Vector2Int position in corridor.corridorPositions)
			{
				AddCorridorToTile(tilemap, position);
			}

		}
	}

	private void AddCorridorToTile(Tilemap corridorMap, Vector2Int position)
	{
		Vector2Int startPos = Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
		Vector3Int gridPosition = (Vector3Int)(position + startPos);

		if (!TileExistsOnAnyDepth(floorMaps, gridPosition))
		{
			corridorMap.SetTile(gridPosition, gridPalette.corridorTile);
			PlaceOrRemoveColliderIfFloorFound(gridPosition);
			wallMap.SetTile(gridPosition, null);
		}
		AddSurroundingCollider(gridPosition);
	}

	private void AddSurroundingCollider(Vector3Int position)
	{
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (!(i == 0 && j == 0))
				{
					Vector3Int gridPosition = new Vector3Int(i, j, 0) + position;
					PlaceOrRemoveColliderIfFloorFound(gridPosition);

				}
			}
		}
	}

	private void PlaceOrRemoveColliderIfFloorFound(Vector3Int gridPosition)
	{
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

	private void PlaceOrRemoveWallIfFloorFound(Vector3Int gridPosition, int roomID)
	{
		bool exist = TileExistsOnAnyDepth(floorMaps, gridPosition);
		if (!exist)
		{
			wallMap.SetTile(gridPosition, gridPalette.wallTile);
			wallPositonsToRoomIds.Add(gridPosition, roomID); //not gonna work because your putting them all on 0, maybe assign z pos now, then apply when refresh is activated
		}
		else
		{
			wallMap.SetTile(gridPosition, null);
			wallPositonsToRoomIds.Remove(gridPosition);
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
			DrawCorridor(tilemap, room);
		}
	}

	private void DrawRoom(Tilemap tilemap, RoomData room)
	{
		for (int x = -1; x < room.width + 1; x++)
		{
			for (int y = -1; y < room.height + 1; y++)
			{
				Vector2Int startPos = Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(startPos[0] + x, startPos[1] + y));

				if (!(x == -1 || y == -1 || x == room.width || y == room.height))
				{
					tilemap.SetTile(gridPosition, gridPalette.roomTile);
					colliderMap.SetTile(gridPosition, null);




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
				else
				{
					PlaceOrRemoveColliderIfFloorFound(gridPosition);
					int order = x == -1 || y == -1 ? 1 : 0;
					PlaceOrRemoveWallIfFloorFound(gridPosition, room.roomID);
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

	private Tilemap CreateBackgroundMap(int order)
	{
		Tilemap tilemap = Instantiate(backgroundMapPrefab, this.transform);
		tilemap.name = "Background_" + order.ToString();
		//tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

}
