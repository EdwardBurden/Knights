using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapRoom
{
	public int roomID { get; private set; }

	public int width;
	public int height;
	public Vector2Int position = new Vector2Int(-1, -1);

	public List<Vector2> floorGridPositions;
	public List<Vector2> colliderGridPositions;
	public int depth;
	public bool main;

	public TileMapRoom(int id)
	{
		roomID = id;
	}
}
