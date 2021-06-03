using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MinimumGraph
{
	public struct Graph
	{
		public int verticesCount;
		public int edgesCount;
		public TriangleEdge[] edges;

		public Graph(List<TriangleEdge> triangleEdges, List<int> indices, List<Vector2> vertices)
		{
			this.verticesCount = vertices.Count;
			this.edgesCount = triangleEdges.Count;
			this.edges = triangleEdges.ToArray();
		}
	}

	public struct Subset
	{
		public int Parent;
		public int Rank;
	}

	private static int Find(Subset[] subsets, int i)
	{
		if (i > subsets.Length)
		{
			Debug.LogWarning("Error Coming");
		}

		if (subsets[i].Parent != i)
			subsets[i].Parent = Find(subsets, subsets[i].Parent);

		return subsets[i].Parent;
	}

	private static void Union(Subset[] subsets, int x, int y)
	{
		int xroot = Find(subsets, x);
		int yroot = Find(subsets, y);

		if (subsets[xroot].Rank < subsets[yroot].Rank)
			subsets[xroot].Parent = yroot;
		else if (subsets[xroot].Rank > subsets[yroot].Rank)
			subsets[yroot].Parent = xroot;
		else
		{
			subsets[yroot].Parent = xroot;
			++subsets[xroot].Rank;
		}
	}

	public static TriangleEdge[] Kruskal(Graph graph)
	{
		int verticesCount = graph.verticesCount;
		TriangleEdge[] result = new TriangleEdge[10000];
		int i = 0;
		int e = 0;

		Array.Sort(graph.edges, delegate (TriangleEdge a, TriangleEdge b)
		{
			return a.distance.CompareTo(b.distance);
		});

		Subset[] subsets = new Subset[verticesCount];

		for (int v = 0; v < verticesCount; ++v)
		{
			subsets[v].Parent = v;
			subsets[v].Rank = 0;
		}

		while (e < verticesCount - 1)
		{
			TriangleEdge nextEdge = graph.edges[i++];
			int x = Find(subsets, nextEdge.v1);
			int y = Find(subsets, nextEdge.v2);

			if (x != y)
			{
				result[e++] = nextEdge;
				Union(subsets, x, y);
			}
		}
		return result.Take(e).ToArray();
	}

}
