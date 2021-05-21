using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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


	public int minRoomWidth;
	public int meanRoomWidth;
	public int maxRoomWidth;

	public int minRoomHeight;
	public int meanRoomHeight;
	public int maxRoomHeight;

	public int roomsAmount;

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
		worldData.rooms = GenerateRooms(roomsAmount);

		Dictionary<int, Tilemap> depthTilemaps = new Dictionary<int, Tilemap>();

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
	}

	private void AddRoomToTileMap(Tilemap tilemap, TileMapRoom room)
	{
		Color color = new Color((float)room.depth / (float)depth, 0, 0);
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

	private List<TileMapRoom> GenerateRooms(int amount)
	{
		float minDist = Vector2Int.Distance(startposition, GetStartPosition());
		float maxDist = Vector2Int.Distance(startposition, new Vector2Int(GetStartPosition().x + width, GetStartPosition().y + height));
		float maxDistance = Mathf.Max(minDist, maxDist);
		List<TileMapRoom> tileMapRooms = new List<TileMapRoom>();
		for (int room = 0; room < roomsAmount; room++)
		{
			tileMapRooms.Add(GenerateRoom(maxDistance));
		}
		return tileMapRooms;
	}

	private TileMapRoom GenerateRoom(float maxDistance)
	{
		TileMapRoom tileMapRoom = new TileMapRoom();

		tileMapRoom.width = (int)NextGaussian(meanRoomWidth, room_Deviation, minRoomWidth, maxRoomWidth);
		tileMapRoom.height = (int)NextGaussian(meanRoomHeight, room_Deviation, minRoomHeight, maxRoomHeight);
		tileMapRoom.position = new Vector2Int(Random.Range(0, width), Random.Range(0, height));

		float dist = Vector2Int.Distance(startposition, (Vector2Int)tileMapRoom.position + GetStartPosition());
		int inverse = topDepthAtStart ? 1 : 0;
		tileMapRoom.depth = Mathf.RoundToInt(Mathf.Abs(inverse - (dist / maxDistance)) * depth);
		return tileMapRoom;
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
