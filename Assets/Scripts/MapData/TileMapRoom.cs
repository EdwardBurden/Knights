using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapRoom
{
	public int roomID { get; private set; }

	public int width;
	public int height;
	public Vector2Int position = new Vector2Int(-1, -1);
	public int depth;
	public bool isMain;
	public TileBase floorTile;


	public Vector2 centerPosition => new Vector2(position.x + (width * 0.5f), position.y + (height * 0.5f));
	public void SetID(int id)
	{
		roomID = id;
	}
}
