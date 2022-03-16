using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleProceduralMesh : MonoBehaviour
{

    public enum Direction
    {
        Forward, //+z
        Right,   //+x
        Back,    //-z
        Left,    //-x
        Up,      //+y
        Down     //-y
    }
    
    public static readonly Vector3[] Vertices = {
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, 0)
    };

    public static readonly int[][] Triangles = {
        new[] { 0, 1, 2, 3 },
        new[] { 5, 0, 3, 6 },
        new[] { 4, 5, 6, 7 },
        new[] { 1, 4, 7, 2 },
        new[] { 5, 4, 1, 0 },
        new[] { 3, 2, 7, 6 }
    };
    
    void Start()
    {
        var mesh = new Mesh();
        mesh.name = "Procedural Mesh";

        mesh.vertices = Vertices;
        mesh.triangles = new[] { 0, 1, 2, 3};

        GetComponent<MeshFilter>().mesh = mesh;
        


    }

    
}
