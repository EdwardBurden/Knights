using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class RoomData //tobe json
{
	public int roomID;

	public int width;
	public int height;
	public int depth;
	public Vector2Int position;
	public bool isMain;
	public bool isStart;
	public bool isEnd;

	public List<CorridorData> corridors;

	public RoomData()
	{
		this.corridors = new List<CorridorData>();
		isMain = false;
		isStart = false;
		isEnd = false;
	}

	public Vector2 centerPosition => new Vector2(position.x + (width * 0.5f), position.y + (height * 0.5f));

}
