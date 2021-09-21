using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GridData //to be json
{
	public string id;
	public string paletteId;

	public Vector2Int startposition;
	public StartGridPosition startGridPosition;
	public int mapWidth;
	public int mapHeight;
	public int mapDepth;
	public List<RoomData> rooms;

	public GridData(Vector2Int startposition, StartGridPosition startGridPosition, int mapWidth, int mapHeight, int mapDepth)
	{
		Guid guid = Guid.NewGuid();
		id = guid.ToString();
		this.startposition = startposition;
		this.startGridPosition = startGridPosition;
		this.mapWidth = mapWidth;
		this.mapHeight = mapHeight;
		this.mapDepth = mapDepth;
		rooms = new List<RoomData>();
	}

	public RoomData startRoom => rooms.FirstOrDefault(x => x.isStart);

	public RoomData endRoom => rooms.FirstOrDefault(x => x.isEnd);
}
