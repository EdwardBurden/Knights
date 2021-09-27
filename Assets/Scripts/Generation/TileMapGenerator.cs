using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;
using Random = UnityEngine.Random;

//ONLY DO TILEMAP GENERATIO HERE
//MOVE ALL GRID PLACING INTO NEW TileMapGrid
//HOLD GRID DATA IN TileMapGrid

[ExecuteInEditMode]
public class TileMapGenerator : MonoBehaviour
{
	//on inspector
	public Vector2Int startposition;
	public StartGridPosition startGridPositionType = StartGridPosition.Top;
	public bool topDepthAtStart;
	public int mapWidth;
	public int mapHeight;
	public int mapDepth;
	public float includeExtraEdgesPercent;
	public int corridorWidth;
	public Vector3Int widths;
	public Vector3Int heights;
	public float standard_divation;
	public int roomsAmount;
	public float mainsPercentage;
	public GridPalette gridPalette;

	private bool[,] gridPlaced;
	private int MAX_ATTEMPTS = 5;
	private List<TriangleEdge> minimumTree = new List<TriangleEdge>();
	private List<Vector2> vertices = new List<Vector2>();
	private List<int> indices = new List<int>();
	private List<TriangleEdge> triangleEdges = new List<TriangleEdge>();

	public void LoadFromFile(string fileName, string path)
	{
		//import into a worlddata

		//GetComponent<TileMapGrid>().LoadGrid(gridData);
	}

	public void Generate()
	{
		Vector2Int trueStartPosition = Helper.GetStartPosition(startGridPositionType, startposition, mapHeight, mapWidth);
		GridData gridData = GenerateGrid(startposition, trueStartPosition, topDepthAtStart);

		GetComponent<TileMapGrid>().LoadGrid(gridData);
	}

	public GridData GenerateGrid(Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		vertices.Clear();
		indices.Clear();
		triangleEdges.Clear();
		minimumTree.Clear();
		gridPlaced = new bool[mapWidth, mapHeight];

		GridData gridData = new GridData(startposition, startGridPositionType, mapWidth, mapHeight, mapDepth);

		gridData.rooms = GenerateRooms(startposition, relativeStartPosition, topDepthAtStart);
		triangleEdges = GenerateTriangleEdge(gridData.rooms);
		minimumTree = GenerateMinimumTree(triangleEdges, indices, vertices, gridData.rooms);
		gridData.corridors = GenerateCorrridors(gridData);
		gridData.paletteId = gridPalette.name;

		DefineStartRoom(gridData);
		DefineEndRoom(gridData);
		return gridData;
	}

	private List<CorridorData> GenerateCorrridors(GridData gridData)
	{
		List<CorridorData> corridorDatas = new List<CorridorData>();
		foreach (RoomData room in gridData.rooms)
		{
			foreach (int connectedRoom in room.connectedRooms)
			{
				RoomData destination = gridData.rooms.FirstOrDefault(rooom => rooom.roomID == connectedRoom);
				CorridorData corridorData = GenerateCorridor(room, destination);
				corridorDatas.Add(corridorData);
			}
		}
		return corridorDatas;
	}

	private CorridorData GenerateCorridor(RoomData source, RoomData destination)
	{
		Vector2Int direction = Helper.GetCorridorDirectionFromRooms(source, destination, out Vector2Int difference);
		Vector2Int sourcePosition = Vector2Int.RoundToInt(source.centerPosition);
		CorridorData corridorData = new CorridorData();
		corridorData.startRoomID = source.roomID;
		corridorData.endRoomID = destination.roomID;

		for (int i = 0; i < (int)Mathf.Abs(difference.x); i++)
		{
			int x = (int)(sourcePosition.x + (i * direction.x));
			int y = (int)(sourcePosition.y);
			Vector2Int position = new Vector2Int(x, y);
			corridorData.corridorPositions.Add(position);

		}

		for (int j = 0; j < (int)Mathf.Abs(difference.y); j++)
		{
			int x = (int)(sourcePosition.x + difference.x);
			int y = (int)(sourcePosition.y + (j * direction.y));
			Vector2Int position = new Vector2Int(x, y);
			corridorData.corridorPositions.Add(position);
		}
		corridorData.corridorPositions = corridorData.corridorPositions.Distinct().ToList();
		return corridorData;
	}

	private List<TriangleEdge> GenerateTriangleEdge(List<RoomData> rooms)
	{
		List<Vector2> roomPositions = rooms.Select(x => x.centerPosition).ToList();
		TriangulationWrapper.Triangulate(roomPositions, out indices, out vertices);
		List<TriangleEdge> triangleEdges = TriangulationWrapper.ConvertIndicesToEdges(indices, vertices);
		foreach (TriangleEdge edge in triangleEdges)
		{
			edge.source = rooms.FirstOrDefault(x => x.centerPosition == vertices[edge.v1]); ;
			edge.destination = rooms.FirstOrDefault(x => x.centerPosition == vertices[edge.v2]); ;
		}
		return triangleEdges;
	}

	private List<TriangleEdge> GenerateMinimumTree(List<TriangleEdge> triangleEdges, List<int> indices, List<Vector2> vertices , List<RoomData> rooms)
	{
		Graph graph = new Graph(triangleEdges, indices, vertices);
		List<TriangleEdge> minimumTree = MinimumGraph.Kruskal(graph).ToList();
		for (int i = 0; i < triangleEdges.Count; i++)
		{
			if (UnityEngine.Random.Range(0.0f, 1.0f) < includeExtraEdgesPercent && !minimumTree.Contains(triangleEdges[i]))
			{
				minimumTree.Add(triangleEdges[i]);
			}
		}

		foreach (TriangleEdge edge in minimumTree)
		{
			RoomData room = rooms.Find(x => x.roomID == edge.source.roomID);
			room.connectedRooms.Add(edge.destination.roomID);
		}

		return minimumTree;
	}

	private void DefineEndRoom(GridData gridData)
	{
		RoomData end = gridData.rooms.OrderBy(x => x.depth).ThenByDescending(x => Vector2.Distance(gridData.startRoom.centerPosition, x.centerPosition)).FirstOrDefault();
		end.isEnd = true;
	}

	private void DefineStartRoom(GridData gridData)
	{
		RoomData start = gridData.rooms.OrderByDescending(x => x.depth).FirstOrDefault();
		start.isStart = true;
	}

	private RoomData GenerateRoom()
	{
		RoomData tileMapRoom = new RoomData();
		tileMapRoom.width = (int)Helper.NextGaussian(widths[1], standard_divation, widths[0], widths[2]);
		tileMapRoom.height = (int)Helper.NextGaussian(heights[1], standard_divation, heights[0], heights[2]);
		tileMapRoom.isMain = tileMapRoom.width * 1.25 > widths[1] && tileMapRoom.height * 1.25 > heights[1] || (Random.Range(0.0f, 1f) < mainsPercentage);
		return tileMapRoom;
	}

	private bool TryPlaceRoom(RoomData tileMapRoom, float maxDistance, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		bool canPlace = false;
		int attmepts = 0;
		Vector2Int position = Vector2Int.zero;
		while (attmepts < MAX_ATTEMPTS && !canPlace)
		{
			position = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
			canPlace = CanPlaceRoom(tileMapRoom, position);
			attmepts++;
		}
		if (canPlace)
		{
			PlaceRoom(tileMapRoom, position);
			SetDepth(tileMapRoom, maxDistance, startposition, relativeStartPosition, topDepthAtStart);
			return true;
		}
		else
		{
			return false;
		}
	}

	public List<RoomData> GenerateRooms(Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		float minDist = Vector2Int.Distance(startposition, relativeStartPosition);
		float maxDist = Vector2Int.Distance(startposition, new Vector2Int(relativeStartPosition.x + mapWidth, relativeStartPosition.y + mapHeight));
		float maxDistance = Mathf.Max(minDist, maxDist);

		List<RoomData> tileMapRooms = new List<RoomData>();
		for (int room = 0; room < roomsAmount; room++)
		{
			tileMapRooms.Add(GenerateRoom());
		}
		for (int i = tileMapRooms.Count - 1; i >= 0; i--)
		{
			if (!TryPlaceRoom(tileMapRooms[i], maxDistance, startposition, relativeStartPosition, topDepthAtStart))
			{
				tileMapRooms.RemoveAt(i);
			}
		}

		for (int room = 0; room < tileMapRooms.Count; room++)
		{
			tileMapRooms[room].roomID = room;
		}
		return tileMapRooms;
	}

	private bool CanPlaceRoom(RoomData tileMapRoom, Vector2Int position)
	{
		for (int i = -1; i < tileMapRoom.width + 1; i++)
		{
			for (int j = -1; j < tileMapRoom.height + 1; j++)
			{
				if (position.x + i >= mapWidth || position.x + i < 0 || position.y + j >= mapHeight || position.y + j < 0)
				{
					return false;
				}

				if (gridPlaced[position.x + i, position.y + j] != false)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void PlaceRoom(RoomData tileMapRoom, Vector2Int position)
	{
		tileMapRoom.position = position;
		for (int i = 0; i < tileMapRoom.width; i++)
		{
			for (int j = 0; j < tileMapRoom.height; j++)
			{
				gridPlaced[position.x + i, position.y + j] = true;
			}
		}
	}

	private void SetDepth(RoomData tileMapRoom, float maxDistance, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		float dist = Vector2Int.Distance(startposition, Vector2Int.RoundToInt(tileMapRoom.centerPosition) + relativeStartPosition);
		int inverse = topDepthAtStart ? 1 : 0;
		tileMapRoom.depth = Mathf.RoundToInt(Mathf.Abs(inverse - (dist / maxDistance)) * mapDepth);
	}
}
