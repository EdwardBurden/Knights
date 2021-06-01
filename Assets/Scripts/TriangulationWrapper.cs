using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using System.Linq;

public class TriangulationWrapper
{
	public static bool Triangulate(List<Vector2> points, out List<int> outIndices, out List<Vector2> outVertices)
	{
		outVertices = new List<Vector2>();
		outIndices = new List<int>();

		Polygon polygon = new Polygon();

		//List<Vertex> vertices = new List<Vertex>();
		for (int i = 0; i < points.Count; i++)
		{
			Vertex vertex = new Vertex(points[i].x, points[i].y);
			//vertices.Add(vertex);
			polygon.Add(vertex);
		}

		/*for (int i = 0; i < polygon.Points.Count; i++)
		{
			if (i == polygon.Points.Count - 1)
			{
				polygon.Add(new Segment(polygon.Points[i], polygon.Points[0]));
			}
			else
			{
				polygon.Add(new Segment(polygon.Points[i], polygon.Points[i + 1]));
			}
		}
		*/
		ConstraintOptions constraintOptions = new ConstraintOptions();
		constraintOptions.ConformingDelaunay = true;
		//constraintOptions.Convex = false;
		//constraintOptions.SegmentSplitting = 0;
		//constraintOptions.Convex = true;
		//constraintOptions.UseRegions = false;
		QualityOptions qualityOptions = new QualityOptions();
		qualityOptions.SteinerPoints = 100;
		//qualityOptions.VariableArea = false;
		//	qualityOptions.MinimumAngle = 5;
		//qualityOptions.MaximumAngle = 90;
		IMesh mesh = polygon.Triangulate();

		foreach (ITriangle triangle in mesh.Triangles)
		{
			for (int i = 2; i >= 0; i--)
			{
				bool found = false;
				for (int j = 0; j < outVertices.Count; j++)
				{
					if ((outVertices[j].x == triangle.GetVertex(i).X) && (outVertices[j].y == triangle.GetVertex(i).Y))
					{
						outIndices.Add(j);
						found = true;
						break;
					}
				}

				if (!found)
				{
					outVertices.Add(new Vector2((float)triangle.GetVertex(i).X, (float)triangle.GetVertex(i).Y));
					outIndices.Add(outVertices.Count - 1);
				}
			}
		}
		return true;

	}

	public static List<TriangleEdge> ConvertIndicesToEdges(List<int> indices, List<Vector2> vertices)
	{
		List<TriangleEdge> edges = new List<TriangleEdge>();
		for (int i = 0; i < indices.Count; i += 3)
		{
			int v1 = indices[i + 0];
			int v2 = indices[i + 1];
			int v3 = indices[i + 2];

			if (!ContainsOppositeEdge(edges, vertices, v1, v2))
			{
				edges.Add(new TriangleEdge(v1, v2));
			}
			if (!ContainsOppositeEdge(edges, vertices, v2, v3))
			{
				edges.Add(new TriangleEdge(v2, v3));
			}
			if (!ContainsOppositeEdge(edges, vertices, v3, v1))
			{
				edges.Add(new TriangleEdge(v3, v1));
			}

			//edges.Add(new TriangleEdge(v1, v2));
			//edges.Add(new TriangleEdge(v2, v3));
			//edges.Add(new TriangleEdge(v3, v1));

		}
		return edges;
	}

	public static bool ContainsOppositeEdge(List<TriangleEdge> edges, List<Vector2> vertices, int v1, int v2)
	{
		return edges.Any(x => IsOppositeEdge(vertices[x.v1], vertices[x.v2], vertices[v1], vertices[v2]));
	}

	public static bool IsOppositeEdge(Vector2 v1, Vector2 v2, Vector2 opV1, Vector2 opV2)
	{
		return (opV1.x == v2.x) && (opV1.y == v2.y) && (opV2.x == v1.x) && (opV2.y == v1.y);
	}

}
