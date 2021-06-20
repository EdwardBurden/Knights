using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldData
{
	public List<TileMapRoom> rooms;
	public List<TileMapCorridor> corridors;

	public WorldData()
	{

		rooms = new List<TileMapRoom>();
		corridors = new List<TileMapCorridor>();
	}
}
