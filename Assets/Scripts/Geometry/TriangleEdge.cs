using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleEdge
{
	public RoomData source;
	public RoomData destination;
	public int distance;
	public int v1;
	public int v2;

	public TriangleEdge(int v1, int v2, List<Vector2> vertices)
	{
		this.v1 = v1;
		this.v2 = v2;
		source= null;
		destination = null;
		distance = (int)Vector2.Distance(vertices[v1], vertices[v2]);
	}

}
