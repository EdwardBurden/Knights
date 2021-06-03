using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class RoomGenerator
{
	private const int MAX_ATTEMPTS = 5;

	public static TileMapRoom GenerateRoom(int id, Vector3 widths, Vector3 heights, float mainsPercentage, float standard_divation)
	{
		TileMapRoom tileMapRoom = new TileMapRoom();
		tileMapRoom.width = (int)Helper.NextGaussian(widths[1], standard_divation, widths[0], widths[2]);
		tileMapRoom.height = (int)Helper.NextGaussian(heights[1], standard_divation, heights[0], heights[2]);
		tileMapRoom.main = tileMapRoom.width * 1.25 > widths[1] && tileMapRoom.height * 1.25 > heights[1] || (Random.Range(0.0f, 1f) < mainsPercentage);
		return tileMapRoom;
	}

	public static bool AttempToPlace(TileMapRoom tileMapRoom, ref int[,] gridPlaced, float maxDistance, int mapWidth, int mapHeight, int mapDepth, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart)
	{
		bool canPlace = false;
		int attmepts = 0;
		Vector2Int position = Vector2Int.zero;
		while (attmepts < MAX_ATTEMPTS && !canPlace)
		{
			position = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
			canPlace = CanPlaceRoom(tileMapRoom, gridPlaced, position, mapWidth, mapHeight);
			attmepts++;
		}

		if (canPlace)
		{
			PlaceRoom(tileMapRoom, ref gridPlaced, position);
			SetDepth(tileMapRoom, maxDistance, startposition, relativeStartPosition, topDepthAtStart, mapDepth);
			//	Debug.Log("Placed");
			return true;
		}
		else
		{
			//Debug.Log("Could Not place");
			return false;
		}
	}

	private static void PlaceRoom(TileMapRoom tileMapRoom, ref int[,] gridPlaced, Vector2Int position)
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

	private static bool CanPlaceRoom(TileMapRoom tileMapRoom, int[,] gridPlaced, Vector2Int position, int mapWidth, int mapHeight)
	{
		for (int i = 0; i < tileMapRoom.width; i++)
		{
			for (int j = 0; j < tileMapRoom.height; j++)
			{
				if (position.x + i >= mapWidth || position.y + j >= mapHeight)
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

	private static void SetDepth(TileMapRoom tileMapRoom, float maxDistance, Vector2Int startposition, Vector2Int relativeStartPosition, bool topDepthAtStart, int mapDepth)
	{
		float dist = Vector2Int.Distance(startposition, (Vector2Int)tileMapRoom.position + relativeStartPosition);
		int inverse = topDepthAtStart ? 1 : 0;
		tileMapRoom.depth = Mathf.RoundToInt(Mathf.Abs(inverse - (dist / maxDistance)) * mapDepth);
	}

	public static void ChangeActiveLayer(Tilemap tilemap, Transform grid)
	{
		foreach (Transform item in grid)
		{
			item.gameObject.SetActive(!(item.position.z < tilemap.transform.position.z));
		}
	}
}
