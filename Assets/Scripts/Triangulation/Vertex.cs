using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public Vector3 position;

    //The outgoing halfedge (a halfedge that starts at this vertex)
    //Doesnt matter which edge we connect to it
    public HalfEdge halfEdge;

    //Which triangle is this vertex a part of?
    public Triangle triangle;

    //The previous and next vertex this vertex is attached to
    public Vertex prevVertex;
    public Vertex nextVertex;

    //Properties this vertex may have
    //Reflex is concave
    public bool isReflex;
    public bool isConvex;
    public bool isEar;

    public Vertex(Vector3 position)
    {
        this.position = position;
    }
}
