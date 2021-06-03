using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;

public class TileMapGenerator : MonoBehaviour
{
	public Vector2Int startposition;
	public StartGridPosition startGridPositionType = StartGridPosition.Top;
	public bool topDepthAtStart;

	public Transform grid;
	public Tilemap tileMapPrefab;
	public TileBase groundTile;

	public int mapWidth;
	public int mapHeight;
	public int depth;

	public int corridorWidth;

	const int MAX_ATTEMPTS = 5;

	public Vector3Int widths;
	//public int minRoomWidth;
	//public int meanRoomWidth;
	//	public int maxRoomWidth;
	public Vector3Int heights;
	//public int minRoomHeight;
	//public int meanRoomHeight;
	//public int maxRoomHeight;

	public int roomsAmount;
	[Range(0, 1)]
	public float mainsPercentage;
	public float mean_d;

	[Range(0.0f, 1.0f)]
	public float includeExtraEdgesPercent;
	//private List<Triangle> triangulation = new List<Triangle>();
	private List<TriangleEdge> minimumTree = new List<TriangleEdge>();
	private List<Vector2> vertices = new List<Vector2>();
	private List<int> indices = new List<int>();
	private List<TriangleEdge> triangleEdges = new List<TriangleEdge>();
	//List<HalfEdge> allEdges = new List<HalfEdge>();

	Dictionary<int, Tilemap> depthTilemaps = new Dictionary<int, Tilemap>();

	public bool showFullTree;


	private void Awake()
	{
		Generate();
	}


	public Vector2Int GetStartPosition()
	{
		switch (startGridPositionType)
		{
			case StartGridPosition.Top:
				return startposition + new Vector2Int(-mapWidth, -mapHeight);
			case StartGridPosition.Center:
				return startposition + new Vector2Int(-mapWidth / 2, -mapHeight / 2);
			case StartGridPosition.Bottom:
				return startposition + new Vector2Int(0, 0);
			default:
				return Vector2Int.zero;
		}

	}
	public void GenerateFromData(WorldData worldData)
	{

	}




	public void Generate()
	{
		//data
		WorldData worldData = new WorldData();
		worldData.rooms = GenerateRooms();

		depthTilemaps.Clear();
		foreach (Transform map in grid.transform)
		{
			Destroy(map.gameObject);
		}

		//display
		foreach (TileMapRoom room in worldData.rooms)
		{
			if (!depthTilemaps.TryGetValue(room.depth, out Tilemap tilemap))
			{
				tilemap = CreateNewTileMap(room.depth);
				depthTilemaps.Add(room.depth, tilemap);
			}
			AddRoomToTileMap(tilemap, room);
		}

		bool success = TriangulationWrapper.Triangulate(worldData.rooms.Select(x => x.centerPosition).ToList(), out indices, out vertices);
		triangleEdges = TriangulationWrapper.ConvertIndicesToEdges(indices, vertices);


		foreach (TriangleEdge edge in triangleEdges)
		{
			TileMapRoom source = worldData.rooms.FirstOrDefault(x => x.centerPosition == vertices[edge.v1]);
			TileMapRoom destination = worldData.rooms.FirstOrDefault(x => x.centerPosition == vertices[edge.v2]);
			edge.source = source;
			edge.destination = destination;
		}

		Graph graph = new Graph(triangleEdges, indices, vertices);
		minimumTree = MinimumGraph.Kruskal(graph).ToList();
		for (int i = 0; i < triangleEdges.Count; i++)
		{
			if (UnityEngine.Random.Range(0.0f, 1.0f) < includeExtraEdgesPercent && !minimumTree.Contains(triangleEdges[i]))
			{
				minimumTree.Add(triangleEdges[i]);
			}
		}
		

		foreach (TriangleEdge edge in minimumTree)
		{
			GenerateCorridor(edge);
		}

	}

	private void GenerateCorridor(TriangleEdge triangleEdge)
	{

		if (triangleEdge.source.depth == triangleEdge.destination.depth)
		{
			int xDiff = Mathf.RoundToInt(vertices[triangleEdge.v2].x - vertices[triangleEdge.v1].x);
			int yDiff = Mathf.RoundToInt(vertices[triangleEdge.v2].y - vertices[triangleEdge.v1].y);
			int xSign = xDiff > 0 ? 1 : -1;
			int ySign = yDiff > 0 ? 1 : -1;

			var gridPosition = new Vector2Int(GetStartPosition()[0] + (int)triangleEdge.source.centerPosition.x, GetStartPosition()[1] + (int)triangleEdge.source.centerPosition.y);
			triangleEdge.source.tilemap.SetTile((Vector3Int)gridPosition, groundTile);
			triangleEdge.source.tilemap.SetTileFlags((Vector3Int)gridPosition, TileFlags.None);
			triangleEdge.source.tilemap.SetColor((Vector3Int)gridPosition, Color.blue);

			gridPosition = new Vector2Int(GetStartPosition()[0] + (int)triangleEdge.destination.centerPosition.x, GetStartPosition()[1] + (int)triangleEdge.destination.centerPosition.y);
			triangleEdge.source.tilemap.SetTile((Vector3Int)gridPosition, groundTile);
			triangleEdge.source.tilemap.SetTileFlags((Vector3Int)gridPosition, TileFlags.None);
			triangleEdge.source.tilemap.SetColor((Vector3Int)gridPosition, Color.blue);


			for (int i = 0; i < Mathf.Abs(xDiff); i++)
			{
				gridPosition = new Vector2Int(GetStartPosition()[0] + (int)triangleEdge.source.centerPosition.x + i * xSign, GetStartPosition()[1] + (int)triangleEdge.source.centerPosition.y);
				triangleEdge.source.tilemap.SetTile((Vector3Int)gridPosition, groundTile);
				triangleEdge.source.tilemap.SetTileFlags((Vector3Int)gridPosition, TileFlags.None);
				triangleEdge.source.tilemap.SetColor((Vector3Int)gridPosition, Color.blue);
			}
			for (int i = 0; i < Mathf.Abs(yDiff); i++)
			{
				gridPosition = new Vector2Int(GetStartPosition()[0] + (int)triangleEdge.source.centerPosition.x + xDiff, GetStartPosition()[1] + (int)triangleEdge.source.centerPosition.y + i * ySign);
				triangleEdge.source.tilemap.SetTile((Vector3Int)gridPosition, groundTile);
				triangleEdge.source.tilemap.SetTileFlags((Vector3Int)gridPosition, TileFlags.None);
				triangleEdge.source.tilemap.SetColor((Vector3Int)gridPosition, Color.blue);
			}

		}
		else
		{
			Debug.LogWarning("Not same");
		}

	}

	private Vector3 VertexToWorld(Vector2 position)
	{
		return depthTilemaps[0].GetCellCenterWorld(new Vector3Int((int)position.x + GetStartPosition().x, (int)position.y + GetStartPosition().y, 0));
	}

	private void AddRoomToTileMap(Tilemap tilemap, TileMapRoom room)
	{
		Color color = new Color((float)room.depth / (float)depth, 0, 0);

		//color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1, 1);
		if (room.main)
		{
			color = Color.green;
		}
		for (int x = 0; x < room.width; x++)
		{
			for (int y = 0; y < room.height; y++)
			{
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(GetStartPosition()[0] + x, GetStartPosition()[1] + y));

				tilemap.SetTile(gridPosition, groundTile);
				tilemap.SetTileFlags(gridPosition, TileFlags.None);
				tilemap.SetColor(gridPosition, color);
				room.SetTilemap(tilemap);
			}
		}
	}

	private Tilemap CreateNewTileMap(int order)
	{
		Tilemap tilemap = Instantiate(tileMapPrefab, grid);
		tilemap.name = order.ToString();
		tilemap.GetComponent<TilemapRenderer>().sortingOrder = order;
		tilemap.transform.localPosition += new Vector3(0, 0, -order * 10);
		return tilemap;
	}

	private List<TileMapRoom> GenerateRooms()
	{
		float minDist = Vector2Int.Distance(startposition, GetStartPosition());
		float maxDist = Vector2Int.Distance(startposition, new Vector2Int(GetStartPosition().x + mapWidth, GetStartPosition().y + mapHeight));
		float maxDistance = Mathf.Max(minDist, maxDist);
		List<TileMapRoom> tileMapRooms = new List<TileMapRoom>();
		for (int room = 0; room < roomsAmount; room++)
		{
			tileMapRooms.Add(RoomGenerator.GenerateRoom(room, widths, heights, mainsPercentage, mean_d));
		}

		int[,] gridPlaced = new int[mapWidth, mapHeight];
		for (int i = tileMapRooms.Count - 1; i >= 0; i--)
		{
			if (!RoomGenerator.AttempToPlace(tileMapRooms[i], ref gridPlaced, maxDistance, mapWidth, mapHeight, depth, startposition, GetStartPosition(), topDepthAtStart))
			{
				tileMapRooms.RemoveAt(i);
			}
		}

		for (int room = 0; room < tileMapRooms.Count; room++)
		{
			tileMapRooms[room].SetID(room);
		}
		return tileMapRooms;
	}

	private void OnDrawGizmos()
	{
		if (showFullTree)
		{
			for (int i = 0; i < triangleEdges.Count; i++)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(VertexToWorld(vertices[triangleEdges[i].v1]), VertexToWorld(vertices[triangleEdges[i].v2]));
			}
		}

		for (int i = 0; i < minimumTree.Count; i++)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(VertexToWorld(vertices[minimumTree[i].v1]), VertexToWorld(vertices[minimumTree[i].v2]));
		}
	}
}
