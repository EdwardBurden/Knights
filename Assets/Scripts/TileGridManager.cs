using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(TileMapGenerator))]
[RequireComponent(typeof(TileMapGridExporter))]
public class TileGridManager : MonoBehaviour //make singleton //call here to get/change/manage stuff
{
	private TileGridPlacer tileGridPlacer;
	private TileMapGenerator tileMapGenerator;
	private TileMapGridExporter tileMapGridExporter;

	private Dictionary<int, BoundsInt> roomTiles;
	[HideInInspector]
	public Vector3 startRoomPosition;

	private void Awake()
	{
		tileGridPlacer = GetComponent<TileGridPlacer>();
		tileMapGenerator = GetComponent<TileMapGenerator>();
		tileMapGridExporter = GetComponent<TileMapGridExporter>();
	}

	public void CreateNewMap()
	{
		GridData gridData = tileMapGenerator.GenerateData();
		//Random.InitState(gridData.seed); for later 

		tileGridPlacer.Init(gridData);
		tileGridPlacer.PlaceBackground();
		roomTiles = tileGridPlacer.PlaceRooms();
		tileGridPlacer.PlaceCorridors();
		tileGridPlacer.PlaceWalls();
		tileGridPlacer.PlaceDoors();
		//roomTiles = tileGridPlacer.PlaceCorridors(gridData);
		//roomTiles = tileGridPlacer.PlaceWalls(gridData);
		//roomTiles = tileGridPlacer.PlaceDoors(gridData);
		//etc	
		tileGridPlacer.PlaceColliders();

		startRoomPosition = GetStartRoomPosition(gridData);
	}

	private Vector3 GetStartRoomPosition(GridData gridData)
	{
		Vector2Int startPos = Helper.GetGridStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
		Vector2Int center = Vector2Int.RoundToInt(gridData.startRoom.centerPosition);
		Vector3Int gridPosition = (Vector3Int)(startPos + center);
		Vector3 positon = tileGridPlacer.floorMaps[gridData.startRoom.depth].CellToWorld(gridPosition);
		return positon;
	}


}
