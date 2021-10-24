using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TileGridPlacer : MonoBehaviour //call here to place stuff
{
	public Tilemap floorTileMapPrefab;
	public Tilemap wallTileMapPrefab;
	public Tilemap colliderMapPrefab;
	public Tilemap backgroundMapPrefab;

	public Dictionary<int, Tilemap> floorMaps = new Dictionary<int, Tilemap>();
	public Tilemap wallMap;
	private Tilemap colliderMap;
	private GridData gridData;
	private GridPalette gridPalette;

	Vector2Int gridStart => Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);

	public bool HasRoomTile(Vector3Int position)
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

	public bool HasCorrdiorTile(Vector3Int position)
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

	public void Init(GridData gridData)
	{
		ClearGrid();
		this.gridData = gridData;
		gridPalette = Resources.Load<GridPalette>(gridData.paletteId);
		for (int i = 0; i < gridData.mapDepth; i++)
		{
			Tilemap tilemap = CreateFloorMap(i);
			floorMaps.Add(i, tilemap);
		}
		colliderMap = Instantiate(colliderMapPrefab, this.transform);
		colliderMap.name = "Collider";

		wallMap = Instantiate(wallTileMapPrefab, this.transform);
		wallMap.name = "Wall";
	}

	private Tilemap CreateFloorMap(int order)
	{
		Tilemap tilemap = Instantiate(floorTileMapPrefab, this.transform);
		tilemap.name = "Floor_" + order.ToString();
		return tilemap;
	}

	private void ClearGrid()
	{
		floorMaps.Clear();
		List<Transform> maps = transform.Cast<Transform>().ToList();
		foreach (Transform map in maps)
		{
#if UNITY_EDITOR
			DestroyImmediate(map.gameObject);
#else
				Destroy(map.gameObject);
#endif
		}
	}

	public Dictionary<int, BoundsInt> PlaceRooms()
	{
		Dictionary<int, BoundsInt> roomsPositions = new Dictionary<int, BoundsInt>();
		foreach (RoomData room in gridData.rooms)
		{
			floorMaps.TryGetValue(room.depth, out Tilemap tilemap);
			roomsPositions.Add(room.roomID, PlaceRoom(tilemap, room));
			//DrawCorridor(tilemap, room);
		}
		return roomsPositions;//todo replace
	}

	private BoundsInt PlaceRoom(Tilemap tilemap, RoomData room)
	{
		Vector3Int gridPosition = (Vector3Int)(room.position + gridStart);
		BoundsInt roomBounds = new BoundsInt(gridPosition.x, gridPosition.y, 0, room.width, room.height, 1);
		TileBase[] tileArray = new TileBase[roomBounds.size.x * roomBounds.size.y];
		for (int i = 0; i < tileArray.Length; i++)
		{
			tileArray[i] = gridPalette.roomTile;
		}
		tilemap.SetTilesBlock(roomBounds, tileArray);
		return roomBounds;
	}

	public void PlaceCorridors()
	{
		foreach (RoomData room in gridData.rooms)
		{
			floorMaps.TryGetValue(room.depth, out Tilemap tilemap);
			foreach (CorridorData corridor in room.corridors)
			{
				PlaceCorridor(tilemap, corridor);
			}
		}
	}

	private void PlaceCorridor(Tilemap tilemap, CorridorData corridor)
	{
		foreach (Vector2Int position in corridor.corridorPositions)
		{
			Vector3Int gridPosition = (Vector3Int)(position + gridStart);
			tilemap.SetTile(gridPosition, gridPalette.corridorTile);
		}
	}

	public void PlaceWalls()
	{
		List<RoomData> orderedRooms = gridData.rooms.OrderByDescending(x => Vector2.Distance(x.position, gridStart)).ToList();
		foreach (RoomData room in orderedRooms)
		{
			Vector2Int roomCorner = (room.position + gridStart) + new Vector2Int(-1, -1);
			PlaceWall(roomCorner.x, roomCorner.y + room.height + 1, room.width + 2, 1);
			PlaceWall( roomCorner.x, roomCorner.y, room.width + 2, 1);
			PlaceWall( roomCorner.x, roomCorner.y + 1, 1, room.height);
			PlaceWall( roomCorner.x + room.width + 1, roomCorner.y + 1, 1, room.height);
		}
	}

	private void PlaceWall( int x, int y, int width, int length)
	{
		BoundsInt wallBounds = new BoundsInt(x, y, 0, width, length, 1);
		foreach (Vector3Int wallPosition in wallBounds.allPositionsWithin)
		{
			if (!Helper.TileExistsOnAnyDepth(floorMaps, wallPosition))
			{
				wallMap.SetTile(wallPosition, gridPalette.wallTile);
			}
		}
	}


	public void PlaceDoors()
	{
		List<Vector2Int> doorPositions = new List<Vector2Int>();
		foreach (RoomData item in gridData.rooms)
		{
			doorPositions.AddRange(gridData.GetRoomDoorPositions(item.roomID));
		}
		doorPositions = doorPositions.Distinct().ToList();
		foreach (Vector2Int item in doorPositions)
		{
			Vector3Int gridPosition = (Vector3Int)(item + gridStart);
			wallMap.SetTile(gridPosition, gridPalette.doorTile);
		}
	}

	public void PlaceColliders()
	{
		for (int i = 0; i < gridData.mapWidth; i++)
		{
			for (int j = 0; j < gridData.mapHeight; j++)
			{
				Vector3Int gridPosition = new Vector3Int(gridStart.x + i, gridStart.y + j, 0);
				if (!floorMaps.Any(x => x.Value.HasTile(gridPosition)))
				{
					colliderMap.SetTile(gridPosition, gridPalette.colliderTile);
				}
			}
		}
	}

	public Tilemap PlaceBackground()
	{
		Tilemap backgroundMap = GameObject.Instantiate(backgroundMapPrefab, this.transform);
		BoundsInt boundsInt = new BoundsInt(gridStart.x, gridStart.y, 0, gridData.mapWidth, gridData.mapHeight, 1);
		TileBase[] tileArray = new TileBase[boundsInt.size.x * boundsInt.size.y];
		for (int index = 0; index < tileArray.Length; index++)
		{
			tileArray[index] = gridPalette.voidTile;
		}

		backgroundMap.SetTilesBlock(boundsInt, tileArray);
		return backgroundMap;
	}
}
