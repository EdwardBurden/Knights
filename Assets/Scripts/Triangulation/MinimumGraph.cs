using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Triangulation;
using UnityEngine;

public static class MinimumGraph
{
	public struct Edge
	{
		public HalfEdge edge;
		public int sourceRoomId;
		public int destinationRoomId;
		public int distance;

		public Edge(HalfEdge halfEdge)
		{
			edge = halfEdge;
			this.sourceRoomId = halfEdge.prevEdge.v.id;
			this.destinationRoomId = halfEdge.v.id;
			this.distance = (int)Vector2.Distance(halfEdge.prevEdge.v.position, halfEdge.v.position);
			if (distance == 0) {
				Debug.LogWarning("Found 0");
			}
		}
	}

	public struct Graph
	{
		public int verticesCount;
		public int edgesCount;
		public Edge[] edges;

		public Graph(List<Triangle> triangles, int vertexCount)
		{
			List<HalfEdge> allEdges = new List<HalfEdge>();
			for (int i = 0; i < triangles.Count; i++)
			{
				if (triangles[i].halfEdge != null)
				{
					AddToList(allEdges, triangles[i].halfEdge);
					AddToList(allEdges, triangles[i].halfEdge.prevEdge);
					AddToList(allEdges, triangles[i].halfEdge.nextEdge);
					//AddToList(allEdges, triangles[i].halfEdge.oppositeEdge);
				}
			}
			allEdges = allEdges.Distinct().ToList();

			this.verticesCount = triangles.SelectMany(x => x.GetVertex()).Distinct().Count();
			//this.verticesCount = allEdges.Select(x => x.v).Distinct().Count();
			this.edgesCount = allEdges.Count;
			this.edges = allEdges.Select(x => new Edge(x)).ToArray();
		}

		private static void AddToList(List<HalfEdge> allEdges, HalfEdge halfEdge)
		{
			if (allEdges.Contains(halfEdge))
			{
				return;
			}
			if (halfEdge.oppositeEdge != null && allEdges.Contains(halfEdge.oppositeEdge))
			{
				allEdges.Add(halfEdge);
			}
			allEdges.Add(halfEdge);

			
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

	public static Edge[] Kruskal(Graph graph)
	{
		int verticesCount = graph.verticesCount;
		Edge[] result = new Edge[10000];
		int i = 0;
		int e = 0;

		Array.Sort(graph.edges, delegate (Edge a, Edge b)
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
			Edge nextEdge = graph.edges[i++];
			int x = Find(subsets, nextEdge.sourceRoomId);
			int y = Find(subsets, nextEdge.destinationRoomId);

			if (x != y)
			{
				result[e++] = nextEdge;
				Union(subsets, x, y);
			}
		}
		return result.Take(e).ToArray();
	}
}
