using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

public class TriangulationWrapper
{
	public static bool Triangulate(List<Vector2> points, out List<int> outIndices, out List<Vector3> outVertices)
	{
		outVertices = new List<Vector3>();
		outIndices = new List<int>();

		Polygon polygon = new Polygon();

		for (int i = 0; i < points.Count; i++)
		{
			polygon.Add(new Vertex(points[i].x, points[i].y));

			if (i == points.Count - 1)
			{
				polygon.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[0].x, points[0].y)));
			}
			else
			{
				polygon.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[i + 1].x, points[i + 1].y)));

			}
		}
		ConstraintOptions constraintOptions = new ConstraintOptions();
		constraintOptions.ConformingDelaunay = true;
		QualityOptions qualityOptions = new QualityOptions();
		//qualityOptions.MinimumAngle = 0.1;
		IMesh mesh = polygon.Triangulate(constraintOptions, qualityOptions);

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
					outVertices.Add(new Vector3((float)triangle.GetVertex(i).X, (float)triangle.GetVertex(i).Y, 0));
					outIndices.Add(outVertices.Count - 1);
				}
			}
		}
		return true;

	}

}
