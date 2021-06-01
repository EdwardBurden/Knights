using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Triangulation;
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

	public int width;
	public int height;
	public int depth;



	public int room_Deviation = 1;
	const int MAX_ATTEMPTS = 5;

	public int minRoomWidth;
	public int meanRoomWidth;
	public int maxRoomWidth;

	public int minRoomHeight;
	public int meanRoomHeight;
	public int maxRoomHeight;

	public int roomsAmount;

	private List<Triangle> triangulation = new List<Triangle>();
	private List<HalfEdge> minimumTree = new List<HalfEdge>();
	List<HalfEdge> allEdges = new List<HalfEdge>();

	Dictionary<int, Tilemap> depthTilemaps = new Dictionary<int, Tilemap>();

	public bool show;


	private void Awake()
	{
		Generate();
	}


	public Vector2Int GetStartPosition()
	{
		switch (startGridPositionType)
		{
			case StartGridPosition.Top:
				return startposition + new Vector2Int(-width, -height);
			case StartGridPosition.Center:
				return startposition + new Vector2Int(-width / 2, -height / 2);
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



		bool success = TriangulationWrapper.Triangulate(worldData.rooms.Select(x => x.centerPosition).ToList(), out List<int> indices, out List<Vector3> vertices);


		//convex
		//Debug.Log("Rooms:" + worldData.rooms.Count);
		//triangulation = Triangulation.Triangulation.TriangulateByFlippingEdges(worldData.rooms);
		//Debug.Log("Vertex:" + triangulation.SelectMany(x => x.GetVertex()).Distinct().Count());
		//Debug.Log("Triangles:" + triangulation.Count);
		Graph Graph = new Graph(triangulation, worldData.rooms.Count);
		//Debug.Log("Graph:Edges:" + Graph.edges.Count());
		//Debug.Log("Graph:EdgesCount:" + Graph.edgesCount);
		//Debug.Log("Graph:verticesCount:" + Graph.verticesCount);
		minimumTree = MinimumGraph.Kruskal(Graph).Select(x => x.edge).ToList();
		

		 allEdges = new List<HalfEdge>();
		for (int i = 0; i < triangulation.Count; i++)
		{
			if (triangulation[i].halfEdge != null)
			{
				AddToList(allEdges, triangulation[i].halfEdge);
				AddToList(allEdges, triangulation[i].halfEdge.prevEdge);
				AddToList(allEdges, triangulation[i].halfEdge.nextEdge);

			}
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


	}
	private static void AddToList(List<HalfEdge> allEdges, HalfEdge halfEdge)
	{
		if (allEdges.Contains(halfEdge))
		{
			return;
		}
		if (halfEdge.oppositeEdge != null && allEdges.Contains(halfEdge.oppositeEdge))
		{
			allEdges.Add(halfEdge);
		}
		allEdges.Add(halfEdge);


	}

	private void OnDrawGizmos()
	{

		foreach (HalfEdge halfedge in allEdges)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(VertexToWorld(halfedge.prevEdge.v), VertexToWorld(halfedge.v));
			//Gizmos.DrawLine(VertexToWorld(triangle.v1), VertexToWorld(triangle.v3));
			//Gizmos.DrawLine(VertexToWorld(triangle.v2), VertexToWorld(triangle.v3));
			//Vector3 vector3 = depthTilemaps[0].GetCellCenterWorld(new Vector3Int((int)vertex.position.x + GetStartPosition().x, (int)vertex.position.y + GetStartPosition().y, (int)vertex.position.z));
			//vector3 += new Vector3(GetStartPosition().x, GetStartPosition().y, 0);
			//Gizmos.DrawSphere(vector3, 1);
		}

		/*for (int i = 0; i < minimumTree.Count; i++)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(VertexToWorld(minimumTree[i].prevEdge.v), VertexToWorld(minimumTree[i].v));
		}*/
		
	}

	private Vector3 VertexToWorld(Vertex vertex)
	{
		return depthTilemaps[0].GetCellCenterWorld(new Vector3Int((int)vertex.position.x + GetStartPosition().x, (int)vertex.position.y + GetStartPosition().y, 0));
	}

	private void AddRoomToTileMap(Tilemap tilemap, TileMapRoom room)
	{
		Color color = new Color((float)room.depth / (float)depth, 0, 0);
		if (room.main)
		{
			color = Color.green;
		}
		//Color color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1, 1);
		for (int x = 0; x < room.width; x++)
		{
			for (int y = 0; y < room.height; y++)
			{
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(GetStartPosition()[0] + x, GetStartPosition()[1] + y));

				tilemap.SetTile(gridPosition, groundTile);
				tilemap.SetTileFlags(gridPosition, TileFlags.None);
				tilemap.SetColor(gridPosition, color);
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
		float maxDist = Vector2Int.Distance(startposition, new Vector2Int(GetStartPosition().x + width, GetStartPosition().y + height));
		float maxDistance = Mathf.Max(minDist, maxDist);
		List<TileMapRoom> tileMapRooms = new List<TileMapRoom>();
		for (int room = 0; room < roomsAmount; room++)
		{
			tileMapRooms.Add(GenerateRoom(room));
		}

		int[,] gridPlaced = new int[width, height];
		for (int i = tileMapRooms.Count - 1; i >= 0; i--)
		{
			if (!AttempToPlace(tileMapRooms[i], ref gridPlaced, maxDistance))
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

	private TileMapRoom GenerateRoom(int id)
	{
		TileMapRoom tileMapRoom = new TileMapRoom();
		tileMapRoom.width = (int)NextGaussian(meanRoomWidth, room_Deviation, minRoomWidth, maxRoomWidth);
		tileMapRoom.height = (int)NextGaussian(meanRoomHeight, room_Deviation, minRoomHeight, maxRoomHeight);
		tileMapRoom.main = tileMapRoom.width * 1.25 > meanRoomWidth && tileMapRoom.height * 1.25 > meanRoomHeight;
		return tileMapRoom;
	}

	private bool AttempToPlace(TileMapRoom tileMapRoom, ref int[,] gridPlaced, float maxDistance)
	{
		bool canPlace = false;
		int attmepts = 0;
		Vector2Int position = Vector2Int.zero;
		while (attmepts < MAX_ATTEMPTS && !canPlace)
		{
			position = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
			canPlace = CanPlaceRoom(tileMapRoom, gridPlaced, position);
			attmepts++;
		}

		if (canPlace)
		{
			PlaceRoom(tileMapRoom, ref gridPlaced, position);
			SetDepth(tileMapRoom, maxDistance);
			//	Debug.Log("Placed");
			return true;
		}
		else
		{
			//Debug.Log("Could Not place");
			return false;
		}
	}

	private void PlaceRoom(TileMapRoom tileMapRoom, ref int[,] gridPlaced, Vector2Int position)
	{
		tileMapRoom.position = position;
		for (int i = 0; i < tileMapRoom.width; i++)
		{
			for (int j = 0; j < tileMapRoom.height; j++)
			{
				gridPlaced[position.x + i, position.y + j] = 1;
			}
		}
	}

	private bool CanPlaceRoom(TileMapRoom tileMapRoom, int[,] gridPlaced, Vector2Int position)
	{
		for (int i = 0; i < tileMapRoom.width; i++)
		{
			for (int j = 0; j < tileMapRoom.height; j++)
			{
				if (position.x + i >= width || position.y + j >= height)
				{
					return false;
				}

				if (gridPlaced[position.x + i, position.y + j] == 1)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void SetDepth(TileMapRoom tileMapRoom, float maxDistance)
	{
		float dist = Vector2Int.Distance(startposition, (Vector2Int)tileMapRoom.position + GetStartPosition());
		int inverse = topDepthAtStart ? 1 : 0;
		tileMapRoom.depth = Mathf.RoundToInt(Mathf.Abs(inverse - (dist / maxDistance)) * depth);
	}


	public static float NextGaussian()
	{
		float v1, v2, s;
		do
		{
			v1 = (2.0f * Random.Range(0f, 1f)) - 1.0f;
			v2 = (2.0f * Random.Range(0f, 1f)) - 1.0f;
			s = v1 * v1 + v2 * v2;
		} while (s >= 1.0f || s == 0f);
		s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

		return v1 * s;
	}

	public static float NextGaussian(float mean, float standard_deviation, float min, float max)
	{
		float x;
		do
		{
			x = NextGaussian(mean, standard_deviation);
		} while (x < min || x > max);
		return x;
	}

	public static float NextGaussian(float mean, float standard_deviation)
	{
		return mean + NextGaussian() * standard_deviation;
	}

	public void ChangeActiveLayer(Tilemap tilemap)
	{
		foreach (Transform item in grid.transform)
		{
			item.gameObject.SetActive(!(item.position.z < tilemap.transform.position.z));
		}
	}
	private void Update()
	{

		/*if (Input.GetMouseButtonDown(0))
		{
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int gridPos = tilemap.WorldToCell(pos);

			TileBase tile = tilemap.GetTile(gridPos);
			if (tile != null)
			{
				Debug.Log($"Pos {pos} Grid {gridPos} Tile {tile.name}");
			}
		}*/
	}
}
