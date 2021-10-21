using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Helper //rename later maybe
{
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

	public static Vector2Int GetGridStartPosition(StartGridPosition startGridPosition, Vector2Int startposition, int height, int width)
	{
		switch (startGridPosition)
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

	public static bool TileExistsInTileMap(Vector2Int position, ITilemap tilemap, TileBase tileBase, int height)
	{
		for (int i = 0; i < height; i++)
		{
			Vector3Int realPosition = new Vector3Int(position.x, position.y, height);
			if (tilemap.GetTile(realPosition) == tileBase)
			{
				return true;
			};
		}
		return false;
	}

	public static bool TileExistsOnAnyDepth(Dictionary<int, Tilemap> depths, Vector3Int position)
	{
		foreach (KeyValuePair<int, Tilemap> tilemap in depths.ToList())
		{
			if (tilemap.Value.GetTile(position) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static Vector2Int GetCorridorDirectionFromRooms(RoomData source, RoomData destination, out Vector2Int difference)
	{
		Vector2Int sourcePosition = Vector2Int.RoundToInt(source.centerPosition);
		Vector2Int destinationPosition = Vector2Int.RoundToInt(destination.centerPosition);
		difference = destinationPosition - sourcePosition;
		int xDirection = 0;
		if (difference.x != 0)
		{
			xDirection = difference.x > 0 ? 1 : -1;
		}
		int yDirection = 0;
		if (difference.y != 0)
		{
			yDirection = difference.y > 0 ? 1 : -1;
		}

		return new Vector2Int(xDirection, yDirection);

	}
}
