using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapRoom
{
	public int roomID;

	public int width;
	public int height;
	public int depth;
	public Vector2Int position;
	public bool isMain;

	//public TileBase floorTile;
	public List<int> connectedRooms;

	public TileMapRoom()
	{
		this.connectedRooms = new List<int>();
	}

	public Vector2 centerPosition => new Vector2(position.x + (width * 0.5f), position.y + (height * 0.5f));


}
