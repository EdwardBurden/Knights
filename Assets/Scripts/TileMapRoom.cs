using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapRoom
{
	public int width;
	public int height;
	public Vector2Int position;

	public List<Vector2> floorGridPositions;
	public List<Vector2> colliderGridPositions;
	public int depth;
}
