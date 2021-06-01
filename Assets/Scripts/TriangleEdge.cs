using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TriangleEdge
{
	public int v1;
	public int v2;

	public TriangleEdge(int v1, int v2)
	{
		this.v1 = v1;
		this.v2 = v2;
	}
}
