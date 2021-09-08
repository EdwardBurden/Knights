using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData //to be json
{
	public Vector2Int startposition;
	public StartGridPosition startGridPosition;
	public int mapWidth;
	public int mapHeight;
	public int mapDepth;
	public List<TileMapRoom> rooms;

	public GridData(Vector2Int startposition, StartGridPosition startGridPosition, int mapWidth, int mapHeight, int mapDepth)
	{
		this.startposition = startposition;
		this.startGridPosition = startGridPosition;
		this.mapWidth = mapWidth;
		this.mapHeight = mapHeight;
		this.mapDepth = mapDepth;
		rooms = new List<TileMapRoom>();
	}
}
