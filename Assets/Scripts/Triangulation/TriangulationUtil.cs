using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TriangulationUtil
{
	public static float IsAPointLeftOfVectorOrOnTheLine(Vector2 a, Vector2 b, Vector2 p)
	{
		float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

		return determinant;
	}

	public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
	{
		//Make sure the triangles have the same orientation
		TriangulationUtil.OrientTrianglesClockwise(triangles);

		//First create a list with all possible half-edges
		List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

		for (int i = 0; i < triangles.Count; i++)
		{
			Triangle t = triangles[i];

			HalfEdge he1 = new HalfEdge(t.v1);
			HalfEdge he2 = new HalfEdge(t.v2);
			HalfEdge he3 = new HalfEdge(t.v3);

			he1.nextEdge = he2;
			he2.nextEdge = he3;
			he3.nextEdge = he1;

			he1.prevEdge = he3;
			he2.prevEdge = he1;
			he3.prevEdge = he2;

			//The vertex needs to know of an edge going from it
			he1.v.halfEdge = he2;
			he2.v.halfEdge = he3;
			he3.v.halfEdge = he1;

			//The face the half-edge is connected to
			t.halfEdge = he1;

			he1.t = t;
			he2.t = t;
			he3.t = t;

			//Add the half-edges to the list
			halfEdges.Add(he1);
			halfEdges.Add(he2);
			halfEdges.Add(he3);
		}

		//Find the half-edges going in the opposite direction
		for (int i = 0; i < halfEdges.Count; i++)
		{
			HalfEdge he = halfEdges[i];

			Vertex goingToVertex = he.v;
			Vertex goingFromVertex = he.prevEdge.v;

			for (int j = 0; j < halfEdges.Count; j++)
			{
				//Dont compare with itself
				if (i == j)
				{
					continue;
				}

				HalfEdge heOpposite = halfEdges[j];

				//Is this edge going between the vertices in the opposite direction
				if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
				{
					he.oppositeEdge = heOpposite;

					break;
				}
			}
		}


		return halfEdges;
	}

	public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		bool isClockWise = true;

		float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

		if (determinant > 0f)
		{
			isClockWise = false;
		}

		return isClockWise;
	}
	public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		bool isConvex = false;

		bool abc = TriangulationUtil.IsTriangleOrientedClockwise(a, b, c);
		bool abd = TriangulationUtil.IsTriangleOrientedClockwise(a, b, d);
		bool bcd = TriangulationUtil.IsTriangleOrientedClockwise(b, c, d);
		bool cad = TriangulationUtil.IsTriangleOrientedClockwise(c, a, d);

		if (abc && abd && bcd & !cad)
		{
			isConvex = true;
		}
		else if (abc && abd && !bcd & cad)
		{
			isConvex = true;
		}
		else if (abc && !abd && bcd & cad)
		{
			isConvex = true;
		}
		//The opposite sign, which makes everything inverted
		else if (!abc && !abd && !bcd & cad)
		{
			isConvex = true;
		}
		else if (!abc && !abd && bcd & !cad)
		{
			isConvex = true;
		}
		else if (!abc && abd && !bcd & !cad)
		{
			isConvex = true;
		}


		return isConvex;
	}

	public static void OrientTrianglesClockwise(List<Triangle> triangles)
	{
		for (int i = 0; i < triangles.Count; i++)
		{
			Triangle tri = triangles[i];

			Vector2 v1 = new Vector2(tri.v1.position.x, tri.v1.position.y);
			Vector2 v2 = new Vector2(tri.v2.position.x, tri.v2.position.y);
			Vector2 v3 = new Vector2(tri.v3.position.x, tri.v3.position.y);

			if (!IsTriangleOrientedClockwise(v1, v2, v3))
			{
				tri.ChangeOrientation();
			}
		}
	}

	public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
	{
		bool isIntersecting = false;

		float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

		//Make sure the denominator is > 0, if not the lines are parallel
		if (denominator != 0f)
		{
			float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
			float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

			//Are the line segments intersecting if the end points are the same
			if (shouldIncludeEndPoints)
			{
				//Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
				if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
				{
					isIntersecting = true;
				}
			}
			else
			{
				//Is intersecting if u_a and u_b are between 0 and 1
				if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
				{
					isIntersecting = true;
				}
			}

		}

		return isIntersecting;
	}

	public static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
	{
		//This first part will simplify how we calculate the determinant
		float a = aVec.x - dVec.x;
		float d = bVec.x - dVec.x;
		float g = cVec.x - dVec.x;

		float b = aVec.y - dVec.y;
		float e = bVec.y - dVec.y;
		float h = cVec.y - dVec.y;

		float c = a * a + b * b;
		float f = d * d + e * e;
		float i = g * g + h * h;

		float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

		return determinant;
	}

	public static List<Triangle> TriangulatePoints(List<Vertex> points)
	{
		//Generate the convex hull - will also remove the points from points list which are not on the hull
		List<Vertex> pointsOnConvexHull = GetConvexHull(points);

		//Triangulate the convex hull
		List<Triangle> triangles = TriangulateConvexPolygon(pointsOnConvexHull);

		//Add the remaining points and split the triangles
		for (int i = 0; i < points.Count; i++)
		{
			Vertex currentPoint = points[i];

			//2d space
			Vector2 p = new Vector2(currentPoint.position.x, currentPoint.position.y);

			//Which triangle is this point in?
			for (int j = 0; j < triangles.Count; j++)
			{
				Triangle t = triangles[j];

				Vector2 p1 = new Vector2(t.v1.position.x, t.v1.position.y);
				Vector2 p2 = new Vector2(t.v2.position.x, t.v2.position.y);
				Vector2 p3 = new Vector2(t.v3.position.x, t.v3.position.y);

				if (IsPointInTriangle(p1, p2, p3, p))
				{
					//Create 3 new triangles
					Triangle t1 = new Triangle(t.v1, t.v2, currentPoint);
					Triangle t2 = new Triangle(t.v2, t.v3, currentPoint);
					Triangle t3 = new Triangle(t.v3, t.v1, currentPoint);

					//Remove the old triangle
					triangles.Remove(t);

					//Add the new triangles
					triangles.Add(t1);
					triangles.Add(t2);
					triangles.Add(t3);

					break;
				}
			}
		}

		return triangles;
	}

	public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
	{
		bool isWithinTriangle = false;

		//Based on Barycentric coordinates
		float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

		float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
		float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
		float c = 1 - a - b;

		//The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
		//if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
		//{
		//    isWithinTriangle = true;
		//}

		//The point is within the triangle
		if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
		{
			isWithinTriangle = true;
		}

		return isWithinTriangle;
	}

	public static List<Triangle> TriangulateConvexPolygon(List<Vertex> convexHullpoints)
	{
		List<Triangle> triangles = new List<Triangle>();

		for (int i = 2; i < convexHullpoints.Count; i++)
		{
			Vertex a = convexHullpoints[0];
			Vertex b = convexHullpoints[i - 1];
			Vertex c = convexHullpoints[i];

			triangles.Add(new Triangle(a, b, c));
		}

		return triangles;
	}

	public static List<Vertex> GetConvexHull(List<Vertex> points)
	{
		//If we have just 3 points, then they are the convex hull, so return those
		if (points.Count == 3)
		{
			//These might not be ccw, and they may also be colinear
			return points;
		}

		//If fewer points, then we cant create a convex hull
		if (points.Count < 3)
		{
			return null;
		}



		//The list with points on the convex hull
		List<Vertex> convexHull = new List<Vertex>();

		//Step 1. Find the vertex with the smallest x coordinate
		//If several have the same x coordinate, find the one with the smallest z
		Vertex startVertex = points[0];

		Vector3 startPos = startVertex.position;

		for (int i = 1; i < points.Count; i++)
		{
			Vector3 testPos = points[i].position;

			//Because of precision issues, we use Mathf.Approximately to test if the x positions are the same
			if (testPos.x < startPos.x || (Mathf.Approximately(testPos.x, startPos.x) && testPos.y < startPos.y))
			{
				startVertex = points[i];

				startPos = startVertex.position;
			}
		}

		//This vertex is always on the convex hull
		convexHull.Add(startVertex);

		points.Remove(startVertex);



		//Step 2. Loop to generate the convex hull
		Vertex currentPoint = convexHull[0];

		//Store colinear points here - better to create this list once than each loop
		List<Vertex> colinearPoints = new List<Vertex>();

		int counter = 0;

		while (true)
		{
			//After 2 iterations we have to add the start position again so we can terminate the algorithm
			//Cant use convexhull.count because of colinear points, so we need a counter
			if (counter == 2)
			{
				points.Add(convexHull[0]);
			}

			//Pick next point randomly
			Vertex nextPoint = points[Random.Range(0, points.Count)];

			//To 2d space so we can see if a point is to the left is the vector ab
			Vector2 a = (Vector2)currentPoint.position;

			Vector2 b = nextPoint.position;

			//Test if there's a point to the right of ab, if so then it's the new b
			for (int i = 0; i < points.Count; i++)
			{
				//Dont test the point we picked randomly
				if (points[i].Equals(nextPoint))
				{
					continue;
				}

				Vector2 c = points[i].position;

				//Where is c in relation to a-b
				// < 0 -> to the right
				// = 0 -> on the line
				// > 0 -> to the left
				float relation = IsAPointLeftOfVectorOrOnTheLine(a, b, c);

				//Colinear points
				//Cant use exactly 0 because of floating point precision issues
				//This accuracy is smallest possible, if smaller points will be missed if we are testing with a plane
				float accuracy = 0.00001f;

				if (relation < accuracy && relation > -accuracy)
				{
					colinearPoints.Add(points[i]);
				}
				//To the right = better point, so pick it as next point on the convex hull
				else if (relation < 0f)
				{
					nextPoint = points[i];

					b = nextPoint.position;

					//Clear colinear points
					colinearPoints.Clear();
				}
				//To the left = worse point so do nothing
			}



			//If we have colinear points
			if (colinearPoints.Count > 0)
			{
				colinearPoints.Add(nextPoint);

				//Sort this list, so we can add the colinear points in correct order
				colinearPoints = colinearPoints.OrderBy(n => Vector3.SqrMagnitude(n.position - currentPoint.position)).ToList();

				convexHull.AddRange(colinearPoints);

				currentPoint = colinearPoints[colinearPoints.Count - 1];

				//Remove the points that are now on the convex hull
				for (int i = 0; i < colinearPoints.Count; i++)
				{
					points.Remove(colinearPoints[i]);
				}

				colinearPoints.Clear();
			}
			else
			{
				convexHull.Add(nextPoint);

				points.Remove(nextPoint);

				currentPoint = nextPoint;
			}

			//Have we found the first point on the hull? If so we have completed the hull
			if (currentPoint.Equals(convexHull[0]))
			{
				//Then remove it because it is the same as the first point, and we want a convex hull with no duplicates
				convexHull.RemoveAt(convexHull.Count - 1);

				break;
			}

			counter += 1;
		}

		return convexHull;
	}
}
