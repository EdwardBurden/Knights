using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MinimumGraph;

//ONLY DO TILEMAP GENERATIO HERE
//MOVE ALL GRID PLACING INTO NEW TileMapGrid
//HOLD GRID DATA IN TileMapGrid


public class TileMapGenerator : MonoBehaviour
{
	TileMapGrid tileMapGrid;

	public Vector2Int startposition;
	public StartGridPosition startGridPositionType = StartGridPosition.Top;
	public bool topDepthAtStart;
	public Transform grid;
	public Tilemap tileMapPrefab;

	private List<TriangleEdge> minimumTree = new List<TriangleEdge>();
	private List<Vector2> vertices = new List<Vector2>();
	private List<int> indices = new List<int>();
	private List<TriangleEdge> triangleEdges = new List<TriangleEdge>();
	Dictionary<int, Tilemap> depthTilemaps = new Dictionary<int, Tilemap>();

	public bool showFullTree;
	public float includeExtraEdgesPercent;


	private void Start()
	{
		tileMapGrid = GetComponent<TileMapGrid>();
		Generate();
	}

	public void Generate()
	{
		//GENERATE
		WorldData worldData = tileMapGrid.GenerateWorld(startposition, GetStartPosition(), topDepthAtStart);

		//CLEAR
		depthTilemaps.Clear();
		foreach (Transform map in grid.transform)
		{
			Destroy(map.gameObject);
		}

		//SHOW
		foreach (TileMapRoom room in worldData.rooms)
		{
			if (!depthTilemaps.TryGetValue(room.depth, out Tilemap tilemap))
			{
				tilemap = CreateNewTileMap(room.depth);
				depthTilemaps.Add(room.depth, tilemap);
			}
			AddRoomToTileMap(tilemap, room);
		}
		Tilemap corridorMap = CreateNewTileMap(tileMapGrid.mapDepth+1);
		foreach (TileMapCorridor corridor in worldData.corridors)
		{
			AddCorridorToTileMap(corridorMap, corridor);
		}


	}

	public Vector2Int GetStartPosition()
	{
		switch (startGridPositionType)
		{
			case StartGridPosition.Top:
				return startposition + new Vector2Int(-tileMapGrid.mapWidth, -tileMapGrid.mapHeight);
			case StartGridPosition.Center:
				return startposition + new Vector2Int(-tileMapGrid.mapWidth / 2, -tileMapGrid.mapHeight / 2);
			case StartGridPosition.Bottom:
				return startposition + new Vector2Int(0, 0);
			default:
				return Vector2Int.zero;
		}

	}

	private void AddCorridorToTileMap(Tilemap tilemap, TileMapCorridor corridor)
	{
		foreach (Vector3Int pos in corridor.positions)
		{
			Vector3Int gridPosition = (Vector3Int)(pos + (Vector3Int)GetStartPosition());

			tilemap.SetTile(gridPosition, corridor.TileBase);
			tilemap.SetTileFlags(gridPosition, TileFlags.None);
			//tilemap.SetColor(gridPosition, color);
		}
	}

	private void AddRoomToTileMap(Tilemap tilemap, TileMapRoom room)
	{
		Color color = new Color((float)room.depth / (float)tileMapGrid.mapDepth, 0, 0);

		//color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1, 1);
		if (room.isMain)
		{
			color = Color.green;
		}
		for (int x = 0; x < room.width; x++)
		{
			for (int y = 0; y < room.height; y++)
			{
				Vector3Int gridPosition = (Vector3Int)(room.position + new Vector2Int(GetStartPosition()[0] + x, GetStartPosition()[1] + y));

				tilemap.SetTile(gridPosition, room.floorTile);
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
}
