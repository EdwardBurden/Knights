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
	//public List<CorridorData> corridors;
	public int seed;

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
		//scorridors = new List<CorridorData>();
	}

	public List<CorridorData> GetRoomsCorridorData(int roomId)
	{
		List<CorridorData> corridors = rooms.SelectMany(x => x.corridors).Where(x => x.startRoomID == roomId || x.endRoomID == roomId).ToList();
		return corridors;
	}

	public List<Vector2Int> GetRoomDoorPositions(int roomId)
	{
		List<CorridorData> corridors = GetRoomsCorridorData(roomId);
		List<Vector2Int> positons = corridors.Where(x => x.startRoomID == roomId).Select(x => x.corridorPositions.FirstOrDefault()).ToList();
		positons.AddRange(corridors.Where(x => x.endRoomID == roomId).Select(x => x.corridorPositions.LastOrDefault()).ToList());
		return positons;
	}

	public RoomData GetRoom(int id)
	{
		return rooms.FirstOrDefault(x => x.roomID == id);

	}

	public RoomData startRoom => rooms.FirstOrDefault(x => x.isStart);

	public RoomData endRoom => rooms.FirstOrDefault(x => x.isEnd);
}
