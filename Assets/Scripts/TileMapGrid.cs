using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;

public class TileMapGrid : MonoBehaviour
{
	public TileBase roomTile;
	public TileBase corridorTile;
	public Tilemap tileMapPrefab;

	private GridData gridData;
	private Dictionary<int, Tilemap> depthTilemaps = new Dictionary<int, Tilemap>();

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

		foreach (TileMapRoom room in gridData.rooms)
		{
			if (!depthTilemaps.TryGetValue(room.depth, out Tilemap tilemap))
			{
				tilemap = CreateNewTileMap(room.depth);
				depthTilemaps.Add(room.depth, tilemap);
			}
			AddRoomToTileMap(tilemap, room);
		}
		Tilemap corridorMap = CreateNewTileMap(gridData.mapDepth + 1);
		foreach (TileMapRoom room in gridData.rooms)
		{
			Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
			Vector2Int roomCenterInt = new Vector2Int((int)room.centerPosition.x, (int)room.centerPosition.y);
			Vector3Int gridPosition = (Vector3Int)(roomCenterInt + new Vector2Int(startPos[0], startPos[1]));
			corridorMap.SetTile(gridPosition, corridorTile);



			foreach (var item in room.connectedRooms)
			{
				TileMapRoom destination = gridData.rooms.FirstOrDefault(rooom => rooom.roomID == item);
				//Vector2Int destCenterInt = new Vector2Int((int)destination.centerPosition.x, (int)destination.centerPosition.y);
				//Vector3Int destPosition = (Vector3Int)(destCenterInt + new Vector2Int(startPos[0], startPos[1]));


				float x = (destination.centerPosition.x + room.centerPosition.x) / 2.0f;
				float y = (destination.centerPosition.y + room.centerPosition.y) / 2.0f;
				Vector2Int midpoint = new Vector2Int((int)x, (int)y);
				//corridorMap.SetTile(gridPosition, corridorTile);

				Vector3Int posss = (Vector3Int)(midpoint + new Vector2Int(startPos[0], startPos[1]));
				corridorMap.SetTile(posss, corridorTile);



		
			}

			//determine faces to end point
			//pick one
			//loop in face direction to end
			//move in other direction if needed

		}
	}

	private bool IsRoomFacingRoom(TileMapRoom sourceRoom, TileMapRoom destinationRoom, Vector2 direction)
	{
		return true;

	}

	private void AddCorridorToTileMap(Tilemap tilemap, TileMapCorridor corridor)
	{
		foreach (Vector3Int pos in corridor.positions)
		{
			Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
			Vector3Int gridPosition = (Vector3Int)(pos + (Vector3Int)startPos);

			tilemap.SetTile(gridPosition, corridorTile);
			tilemap.SetTileFlags(gridPosition, TileFlags.None);
			//tilemap.SetColor(gridPosition, color);
		}
	}

	private void AddRoomToTileMap(Tilemap tilemap, TileMapRoom room)
	{
		//Color color = new Color((float)room.depth / (float)gridData.mapDepth, 0, 0);
		Color color = new Color(room.connectedRooms.Count, 0, 0);
		//color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1, 1);
		if (room.isMain)
		{
			//color = Color.green;
		}
		for (int x = 0; x < room.width; x++)
		{
			for (int y = 0; y < room.height; y++)
			{
				Vector2Int startPos = Helper.GetStartPosition(gridData.startGridPosition, gridData.startposition, gridData.mapHeight, gridData.mapWidth);
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
