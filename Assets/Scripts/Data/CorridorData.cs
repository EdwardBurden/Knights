using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CorridorData
{
	public int corridorID;
	public int startRoomID;
	public int endRoomID;
	public List<Vector2> corridorPositions;

	public CorridorData()
	{
		corridorPositions = new List<Vector2>();
	}
}
