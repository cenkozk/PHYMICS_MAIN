using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CubeMeshData
{
    public static Vector3[] vertices =
    {
        new Vector3( 1,  1,  1),
        new Vector3(-1,  1,  1),
        new Vector3(-1, -1,  1),
        new Vector3( 1, -1,  1),
        new Vector3(-1,  1, -1),
        new Vector3( 1,  1, -1),
        new Vector3( 1, -1, -1),
        new Vector3(-1, -1, -1)
    };

    public static int[][] faceTris =
    {
       new []{0,1,2,3},
       new []{5,0,3,6},
       new []{4,5,6,7},
       new []{1,4,7,2},
       new []{5,4,1,0},
       new []{3,2,7,6}
    };

    public static Vector3[] faceVertices(int dir,float scale,Vector3 pos)
    {
        Vector3[] fv = new Vector3[4];
        for (int i = 0; i < fv.Length; i++)
        {
            fv[i] = vertices[faceTris[dir][i]] * scale + pos;
        }

        return fv;
    }

    public static Vector3[] faceVertices(Direction dir, float scale, Vector3 pos)
    {
        return faceVertices((int)dir, scale, pos);
    }
}


/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CubeMeshData
{
    public static Vector3[] vertices =
    {
        new Vector3( 1,  1,  1),
        new Vector3(-1,  1,  1),
        new Vector3(-1, -1,  1),
        new Vector3( 1, -1,  1),
        new Vector3(-1,  1, -1),
        new Vector3( 1,  1, -1),
        new Vector3( 1, -1, -1),
        new Vector3(-1, -1, -1)
    };

    public static int[][] faceTris =
    {
       new []{0,1,2,3},
       new []{5,0,3,6},
       new []{4,5,6,7},
       new []{1,4,7,2},
       new []{5,4,1,0},
       new []{3,2,7,6}
    };

    public static Vector3[] faceVertices(int dir,float scale,Vector3 pos)
    {
        Vector3[] fv = new Vector3[4];
        for (int i = 0; i < fv.Length; i++)
        {
            fv[i] = vertices[faceTris[dir][i]] * scale + pos;
        }

        return fv;
    }

    public static Vector3[] faceVertices(Direction dir, float scale, Vector3 pos)
    {
        return faceVertices((int)dir, scale, pos);
    }
}
*/
