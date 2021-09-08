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
		gridPlaced = new bool[mapWidth, mapHeight];
		minimumTree.Clear();
		vertices.Clear();
		indices.Clear();
		triangleEdges.Clear();

		float minDist = Vector2Int.Distance(startposition, relativeStartPosition);
		float maxDist = Vector2Int.Distance(startposition, new Vector2Int(relativeStartPosition.x + mapWidth, relativeStartPosition.y + mapHeight));
		float maxDistance = Mathf.Max(minDist, maxDist);

		GridData gridData = new GridData(startposition, startGridPositionType, mapWidth, mapHeight, mapDepth);
		gridData.rooms = GenerateRooms(maxDistance, startposition, relativeStartPosition, topDepthAtStart);


		bool success = TriangulationWrapper.Triangulate(gridData.rooms.Select(x => x.centerPosition).ToList(), out indices, out vertices);
		triangleEdges = TriangulationWrapper.ConvertIndicesToEdges(indices, vertices);
		foreach (TriangleEdge edge in triangleEdges)
		{
			TileMapRoom source = gridData.rooms.FirstOrDefault(x => x.centerPosition == vertices[edge.v1]);
			TileMapRoom destination = gridData.rooms.FirstOrDefault(x => x.centerPosition == vertices[edge.v2]);
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

			TileMapRoom room = gridData.rooms.Find(x => x.roomID == edge.source.roomID);
			room.connectedRooms.Add(edge.destination.roomID);
		}




		return gridData;
	}

	private TileMapRoom GenerateRoom()
	{
		TileMapRoom tileMapRoom = new TileMapRoom();
		tileMapRoom.width = (int)Helper.NextGaussian(widths[1], standard_divation, widths[0], widths[2]);
		tileMapRoom.height = (int)Helper.NextGaussian(heights[1], standard_divation, heights[0], heights[2]);
		tileMapRoom.isMain = tileMapRoom.width * 1.25 > widths[1] && tileMapRoom.height * 1.25 > heights[1] || (Random.Range(0.0f, 1f) < mainsPercentage);
		return tileMapRoom;
	}

	private bool TryPlaceRoom(TileMapRoom tileMapRoom, float maxDistance, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
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

	public List<TileMapRoom> GenerateRooms(float maxDistance, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		List<TileMapRoom> tileMapRooms = new List<TileMapRoom>();
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

	/*private TileMapCorridor GenerateCorridor(TriangleEdge edge, Vector2Int relativeStartPosition, int width)
	{
		TileMapCorridor tileMapCorridor = new TileMapCorridor();
		//tileMapCorridor.TileBase = corridorTile;
		List<Vector3Int> positions = new List<Vector3Int>();


		Vector3 end = (Vector3)vertices[edge.v2];
		Vector3 start = (Vector3)vertices[edge.v1];
		Vector3Int startInt = new Vector3Int((int)start.x, (int)start.y, 0);
		Vector3Int endInt = new Vector3Int((int)end.x, (int)end.y, 0);
		int xDiff = endInt.x - startInt.x;
		int yDiff = endInt.y - startInt.y;
		int xSign = xDiff > 0 ? 1 : -1;
		int ySign = yDiff > 0 ? 1 : -1;


		//REDO AT SOMEPOINT

		for (int i = 0; i < Mathf.Abs(xDiff) + (width / 2); i++)
		{
			Vector3Int gridPosition = startInt + new Vector3Int(xSign * i, 0, 0) - new Vector3Int(0, width / 2, 0);
			for (int w = 0; w < width; w++)
			{
				Vector3Int pos = gridPosition + new Vector3Int(0, w, 0);
				if (gridPlaced[pos.x, pos.y] == null) //check all tilemap
				{
					gridPlaced[pos.x, pos.y] = corridorTile;
					positions.Add(pos);
				}
			}
		}

		for (int i = 0; i < Mathf.Abs(yDiff) + (width / 2); i++)
		{
			Vector3Int gridPosition = startInt + new Vector3Int(xDiff, ySign * i, 0) - new Vector3Int(width / 2, 0, 0);
			for (int w = 0; w < width; w++)
			{
				Vector3Int pos = gridPosition + new Vector3Int(w, 0, 0);
				if (gridPlaced[pos.x, pos.y] == null) //check all tilemap
				{
					gridPlaced[pos.x, pos.y] = corridorTile;
					positions.Add(pos);
				}
			}
		}

		tileMapCorridor.positions = positions;

		return tileMapCorridor;
	}*/

	private bool CanPlaceRoom(TileMapRoom tileMapRoom, Vector2Int position)
	{
		for (int i = 0; i < tileMapRoom.width; i++)
		{
			for (int j = 0; j < tileMapRoom.height; j++)
			{
				if (position.x + i >= mapWidth || position.y + j >= mapHeight)
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

	private void PlaceRoom(TileMapRoom tileMapRoom, Vector2Int position)
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

	private void SetDepth(TileMapRoom tileMapRoom, float maxDistance, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		float dist = Vector2Int.Distance(startposition, (Vector2Int)tileMapRoom.position + relativeStartPosition);
		int inverse = topDepthAtStart ? 1 : 0;
		tileMapRoom.depth = Mathf.RoundToInt(Mathf.Abs(inverse - (dist / maxDistance)) * mapDepth);
	}
}
