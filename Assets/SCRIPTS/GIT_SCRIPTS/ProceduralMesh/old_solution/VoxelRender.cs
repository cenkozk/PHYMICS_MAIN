using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter),(typeof(MeshRenderer)))]
public class VoxelRender : MonoBehaviour
{
    private Mesh _mesh;
    List<Vector3> _vertices;
    List<int> _tris;

    public float scale = 1f;
    private float _adjustedScale;
    
    //COL

    public GameObject colCube;
    private int ColCount = 0;
    public List<string> stringOfBlocks = new List<string>();


    void Awake()
    {
        _mesh = gameObject.GetComponent<MeshFilter>().mesh;
        gameObject.AddComponent<VoxelData>();
    }

    // Update is called once per frame

    public void generate()
    {
        GenerateVoxelMesh(gameObject.GetComponent<VoxelData>());
        UpdateMesh();
    }

    void Start()
    {
        _adjustedScale = scale * 0.5f;
        generate();
    }
    
    public void GenerateVoxelMesh(VoxelData data)
    {
        ColCount = 0;
        _vertices = new List<Vector3>();
        _tris = new List<int>();
        
        for (int y = 0; y < data.Height; y++)
        {
            for (int z = 0; z < data.Depth; z++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    {
                        if (data.GetCell(x,y,z) == 0)
                        {
                            continue;
                        }

                        MakeCube(_adjustedScale, new Vector3(x * scale, y  * scale, z * scale), x, y, z, data);
                    }
                }
            }   
        }
    }
    
    private void MakeCube(float cubeScale,Vector3 cubePos,int x,int y,int z, VoxelData data)
    {
        //
        data.GreedyChecker(x,y,z, (Direction)4);
        //
        var EvenOneFaceCollision = 0;
        for (int i = 0; i < 6; i++)
        {
            //print("Cube: " +$"{x}" + ", " + "Direction: " + (Direction)i + ", " + "Create: " + data.GetNeighbor(x,y,z, (Direction)i));
            if (data.GetNeighbor(x,y,z, (Direction)i) == 0)
            {
                EvenOneFaceCollision++;
                MakeFace((Direction)i,cubeScale,cubePos);
            }
        }

        if (EvenOneFaceCollision >= 1)
        {
            if (!stringOfBlocks.Contains($"{x},{y},{z}"))
            {
                ColCount++;
                var colCubeObject = Instantiate(colCube, gameObject.transform, false);
                colCubeObject.name = $"{x},{y},{z}";
                stringOfBlocks.Add($"{x},{y},{z}");
                colCubeObject.transform.localPosition = cubePos;  
            }
        }
    }
    
    void MakeFace(Direction dir,float faceScale,Vector3 facePos)
    {
        _vertices.AddRange(CubeMeshData.faceVertices(dir,faceScale,facePos));
        int vCount = _vertices.Count;
        
        _tris.Add(vCount - 4);
        _tris.Add(vCount - 4 + 1);
        _tris.Add(vCount - 4 + 2);
        _tris.Add(vCount - 4);
        _tris.Add(vCount - 4 + 2);
        _tris.Add(vCount - 4 + 3);
    }
    
     public void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        print(_mesh.vertices.Length);
        _mesh.triangles = _tris.ToArray();
        _mesh.RecalculateNormals();
        Debug.Log("Created Col "+ ColCount);
    }
    
}


