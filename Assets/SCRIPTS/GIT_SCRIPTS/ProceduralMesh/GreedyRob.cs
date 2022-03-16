using System;
using System.Collections.Generic;
using UnityEngine;

public class GreedyRob : MonoBehaviour
{
    private Mesh mesh;
    private ChunkColBuilder chunkColBuilder;
     /*
     * In this test each voxel has a size of one world unit - in reality a voxel engine 
     * might have larger voxels - and there's a multiplication of the vertex coordinates 
     * below to account for this.
     */
    private static int VOXEL_SIZE = 1;

    /*
     * These are the chunk dimensions - it may not be the case in every voxel engine that 
     * the data is rendered in chunks - but this demo assumes so.  Anyway the chunk size is 
     * just used to populate the sample data array.  Also, in reality the chunk size will likely 
     * be larger - for example, in my voxel engine chunks are 16x16x16 - but the small size 
     * here allows for a simple demostration.
     */
    [NonSerialized]
    public int CHUNK_SIZE = 16;
    private static int CHUNK_WIDTH = 16;
    private static int CHUNK_HEIGHT = 16;
    
    /*
     * This is a 3D array of sample data - I'm using voxel faces here because I'm returning 
     * the same data for each face in this example - but calls to the getVoxelFace function below 
     * will return variations on voxel data per face in a real engine.  For example, in my system 
     * each voxel has a type, temperature, humidity, etc - which are constant across all faces, and 
     * then attributes like sunlight, artificial light which face per face or even per vertex.
     */
    public VoxelFace[,,] voxels = new VoxelFace[CHUNK_WIDTH,CHUNK_HEIGHT,CHUNK_WIDTH];
    public VoxelFace[,,] pushArray = new VoxelFace[CHUNK_WIDTH,CHUNK_HEIGHT,CHUNK_WIDTH];

    
    /*
     *Vertices and tris array.
     */
    private List<Vector3> _vertices = new();
    private List<int> _tris = new();

    /*
     * These are just constants to keep track of which face we're dealing with - their actual 
     * values are unimportantly - only that they're constant.
     */
    private static int SOUTH      = 0;
    private static int NORTH      = 1;
    private static int EAST       = 2;
    private static int WEST       = 3;
    private static int TOP        = 4;
    private static int BOTTOM     = 5;
    
    /*
     * Connected Chunks
     */
    [NonSerialized]
    public  GameObject CC_SOUTH = null;
    [NonSerialized]
    public GameObject CC_NORTH = null;
    [NonSerialized]
    public GameObject CC_EAST;
    [NonSerialized]
    public GameObject CC_WEST;
    [NonSerialized]
    public GameObject CC_TOP;
    [NonSerialized]
    public GameObject CC_BOTTOM;
    
    /**
     * Support for different voxel types, sides and ID's
     * 
     * Each face can contain vertex data - for example, int[] sunlight, in order to compare vertex attributes.
     *
     * Greedy Mesher Algorithm
     */
    public class VoxelFace {
    
        public bool transparent;
        public int type;
        public int side;
        public int ID;
        
        public bool equals(VoxelFace face) { return face.transparent == transparent && face.type == type; }
    }

    /**
     * This is a Starter function used here to set up the Chunk Collision Class.
     */
    public void Starter()
    {
        chunkColBuilder = gameObject.GetComponent<ChunkColBuilder>();
    }

    public void FirstChunk()
    {
        VoxelFace face;
        for (int x = 0; x < CHUNK_WIDTH; x++) {
            
            for (int y = 0; y < CHUNK_HEIGHT; y++) {
            
                for (int z = 0; z < CHUNK_HEIGHT; z++)
                { 
                    face = new VoxelFace();
                    face.type = 1;
                    face.transparent = false;
                    face.ID = 1; 
                    /*
                       * Voxel ID detection.
                       */
                    
                    /*int leftCol;
                    int botCol;
                    int forCol;
                    if (!face.transparent)
                    {
                        leftCol = x-1 >= 0 && x <= 15 ? voxels[x - 1, y, z].ID : 0;
                        botCol = y-1 >= 0 && y <= 15 ? voxels[x, y - 1, z].ID : 0;
                        forCol = z-1 >= 0 && z <= 15 ? voxels[x, y, z - 1].ID : 0;
  
                        if (leftCol == 0 && botCol == 0 && forCol == 0)
                        {
                            id++;
                            equivalent.Add(id,id);
                            face.ID = id;
                        }else if (leftCol != 0 && botCol != 0 && forCol != 0 && leftCol == botCol && botCol == forCol)
                        {
                            face.ID = leftCol;
                        }
                        else if (leftCol != 0 && botCol == 0 && forCol != 0 || botCol != 0 && leftCol == 0 && forCol != 0 || botCol != 0 && leftCol != 0 && forCol == 0 || botCol == 0 && leftCol == 0 && forCol != 0 || botCol == 0 && leftCol != 0 && forCol == 0 || botCol != 0 && leftCol == 0 && forCol == 0)
                        {
                            var idT = Mathf.Max(leftCol, botCol);
                            idT = Mathf.Max(idT, forCol);
                            face.ID = idT;
                        }
                        else if (leftCol != botCol || forCol != botCol || forCol != leftCol )
                        {
                            var min = Mathf.Min(leftCol, botCol);
                            min = Mathf.Max(min, forCol);
                            var max = Mathf.Max(leftCol, botCol);
                            max = Mathf.Max(max, forCol);
                            if (equivalent.ContainsKey(max))
                            {
                                equivalent[max] = min;
                            }
                            else
                            {
                                equivalent.Add(max, min);
                            }
                            face.ID = min; 
                        }
                    }*/

                    voxels[x,y,z] = face;
                }
            }            
        }
    }

    public void PopulateVoxelArray()
    {
        VoxelFace face;
        for (int x = 0; x < CHUNK_WIDTH; x++) {
            
            for (int y = 0; y < CHUNK_HEIGHT; y++) {
            
                for (int z = 0; z < CHUNK_HEIGHT; z++)
                {
                    face = new VoxelFace();
                    face.type = 0;
                    face.transparent = true;
                    face.ID = 0;
                    voxels[x,y,z] = face;
                }
            }            
        }
    }

    
    void findFlood(int x, int y ,int z)
    {
        if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_SIZE || z < 0 || z >= CHUNK_SIZE || voxels[x,y,z].ID == 0 || voxels[x,y,z].ID == 12)
        {
            return;
        }
        
        voxels[x, y, z].ID = 12;
        
        findFlood(x+1,y,z);
        findFlood(x-1,y,z);
        findFlood(x,y+1,z);
        findFlood(x,y-1,z);
        findFlood(x,y,z+1);
        findFlood(x,y,z-1);
    }

    /**
     * 
     */
    public void CreateGreedyMesh()
    {
        bool voxStarted = false;

        for (int a = 0; a < CHUNK_WIDTH; a++)
        {
            for (int b = 0; b < CHUNK_HEIGHT; b++)
            {
                for (int c = 0; c < CHUNK_HEIGHT; c++)
                {
                    if (!voxels[a,b,c].transparent && voxStarted == false)
                    {
                        //voxels[x, y, z].ID = equivalent[voxels[x, y, z].ID];
                        voxStarted = true;
                        findFlood(a,b,c);
                    }
                }
            }
        }
        
        /*
         * Reset the triangle and the vertices array.
         */
        _tris.Clear();
        _vertices.Clear();

        /*
         * These are just working variables for the algorithm
         */
        int i, j, k, l, w, h, u, v, n, side = 0;
        
        int[] x = {0,0,0};
        int[] q = {0,0,0};
        int[] du = {0,0,0}; 
        int[] dv = {0,0,0};         
        
        /*
         * We create a mask - this will contain the groups of matching voxel faces 
         * as we proceed through the chunk in 6 directions - once for each face.
         */
        VoxelFace[] mask = new VoxelFace [CHUNK_WIDTH * CHUNK_HEIGHT];
        
        /*
         * These are just working variables to hold two faces during comparison.
         */
        VoxelFace voxelFace, voxelFace1;

        /*
         * We start with the boolean for loop. 
         * 
         * The variable backFace will be TRUE on the first iteration and FALSE on the second - this allows 
         * us to track which direction the indices should run during creation of the quad.
         * 
         * This loop runs twice, and the inner loop 3 times - totally 6 iterations - one for each 
         * voxel face.
         */
        for (bool backFace = true, b = false; b != backFace; backFace = backFace && b, b = !b) { 

            /*
             * We sweep over the 3 dimensions - most of what follows is well described by Mikola Lysenko 
             * in his post - and is ported from his Javascript implementation.  Where this implementation 
             * diverges, I've added commentary.
             */
            for(int d = 0; d < 3; d++) {

                u = (d + 1) % 3; 
                v = (d + 2) % 3;

                x[0] = 0;
                x[1] = 0;
                x[2] = 0;

                q[0] = 0;
                q[1] = 0;
                q[2] = 0;
                q[d] = 1;

                /*
                 * Here we're keeping track of the side that we're meshing.
                 */
                if (d == 0)      { side = backFace ? WEST   : EAST;  }
                else if (d == 1) { side = backFace ? BOTTOM : TOP;   }
                else if (d == 2) { side = backFace ? SOUTH  : NORTH; }                

                /*
                 * We move through the dimension from front to back
                 */            
                for(x[d] = -1; x[d] < CHUNK_WIDTH;) {

                    /*
                     * -------------------------------------------------------------------
                     *   We compute the mask
                     * -------------------------------------------------------------------
                     */
                    n = 0;

                    for(x[v] = 0; x[v] < CHUNK_HEIGHT; x[v]++) {

                        for(x[u] = 0; x[u] < CHUNK_WIDTH; x[u]++) {

                            /*
                             * Here we retrieve two voxel faces for comparison.
                             */
                            voxelFace  = (x[d] >= 0 )             ? getVoxelFace(x[0], x[1], x[2], side)                      : null;
                            voxelFace1 = (x[d] < CHUNK_WIDTH - 1) ? getVoxelFace(x[0] + q[0], x[1] + q[1], x[2] + q[2], side) : null;

                            /*
                             * Note that we're using the equals function in the voxel face class here, which lets the faces 
                             * be compared based on any number of attributes.
                             * 
                             * Also, we choose the face to add to the mask depending on whether we're moving through on a backface or not.
                             */
                            mask[n++] = ((voxelFace != null && voxelFace1 != null && voxelFace.equals(voxelFace1))) 
                                        ? null 
                                        : backFace ? voxelFace1 : voxelFace;
                        }
                    }

                    x[d]++;

                    /*
                     * Now we generate the mesh for the mask
                     */
                    n = 0;

                    for(j = 0; j < CHUNK_HEIGHT; j++) {

                        for(i = 0; i < CHUNK_WIDTH;) {

                            if(mask[n] != null) {

                                /*
                                 * We compute the width
                                 */
                                for(w = 1; i + w < CHUNK_WIDTH && mask[n + w] != null && mask[n + w].equals(mask[n]); w++) {}

                                /*
                                 * Then we compute height
                                 */
                                bool done = false;

                                for(h = 1; j + h < CHUNK_HEIGHT; h++) {

                                    for(k = 0; k < w; k++) {

                                        if(mask[n + k + h * CHUNK_WIDTH] == null || !mask[n + k + h * CHUNK_WIDTH].equals(mask[n])) { done = true; break; }
                                    }

                                    if(done) { break; }
                                }

                                /*
                                 * Here we check the "transparent" attribute in the VoxelFace class to ensure that we don't mesh 
                                 * any culled faces.
                                 */
                                if (!mask[n].transparent) {
                                    /*
                                     * Add quad
                                     */
                                    x[u] = i;  
                                    x[v] = j;

                                    du[0] = 0;
                                    du[1] = 0;
                                    du[2] = 0;
                                    du[u] = w;

                                    dv[0] = 0;
                                    dv[1] = 0;
                                    dv[2] = 0;
                                    dv[v] = h;

                                    /*
                                     * And here we call the quad function in order to render a merged quad in the scene.
                                     * 
                                     * We pass mask[n] to the function, which is an instance of the VoxelFace class containing 
                                     * all the attributes of the face - which allows for variables to be passed to shaders - for 
                                     * example lighting values used to create ambient occlusion.
                                     */
                                    quad(new Vector3(x[0],                 x[1],                   x[2]), 
                                         new Vector3(x[0] + du[0],         x[1] + du[1],           x[2] + du[2]), 
                                         new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1],   x[2] + du[2] + dv[2]), 
                                         new Vector3(x[0] + dv[0],         x[1] + dv[1],           x[2] + dv[2]), 
                                         w,
                                         h,
                                         mask[n],
                                         backFace);
                                }

                                /*
                                 * We zero out the mask
                                 */
                                for(l = 0; l < h; ++l) {

                                    for(k = 0; k < w; ++k) { mask[n + k + l * CHUNK_WIDTH] = null; }
                                }

                                /*
                                 * And then finally increment the counters and continue
                                 */
                                i += w; 
                                n += w;

                            } else {

                              i++;
                              n++;
                            }
                        }
                    } 
                }
            }        
        }
        
        /*
         * We start generating the mesh.
         */
        meshgen();
    }

    /*
     * Deprecated Chunk Connection Algorithm.
     */
    /*public void SetConnectedChunk(int Direction)
        {
            if (Direction == 4 && !CC_TOP)
            {
                var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, gameObject.transform.parent);
                chunk.transform.localPosition = gameObject.transform.localPosition + new Vector3(0, 4, 0);
                CC_TOP = chunk;
                chunk.GetComponent<GreedyRob>().CC_BOTTOM = gameObject;
            }
            if (Direction == 5 && !CC_BOTTOM)
            {
                var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, gameObject.transform.parent);
                chunk.transform.localPosition = gameObject.transform.localPosition + new Vector3(0, -4, 0);
                CC_BOTTOM = chunk;
                chunk.GetComponent<GreedyRob>().CC_TOP = gameObject;
            }
            if (Direction == 2 && !CC_EAST)
            {
                var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, gameObject.transform.parent);
                chunk.transform.localPosition = gameObject.transform.localPosition + new Vector3(-4, 0, 0);
                CC_EAST = chunk;
                chunk.GetComponent<GreedyRob>().CC_WEST = gameObject;
            }
            if (Direction == 3 && !CC_WEST)
            {
                var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, gameObject.transform.parent);
                chunk.transform.localPosition = gameObject.transform.localPosition + new Vector3(4, 0, 0);
                CC_WEST = chunk;
                chunk.GetComponent<GreedyRob>().CC_EAST = gameObject;
            }
            if (Direction == 0 && !CC_NORTH)
            {
                var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, gameObject.transform.parent);
                chunk.transform.localPosition = gameObject.transform.localPosition + new Vector3(0, 0, -4);
                CC_NORTH = chunk;
                chunk.GetComponent<GreedyRob>().CC_SOUTH = gameObject;
            }
            if (Direction == 1 && !CC_SOUTH)
            {
                var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, gameObject.transform.parent);
                chunk.transform.localPosition = gameObject.transform.localPosition + new Vector3(0, 0, 4);
                CC_SOUTH = chunk;
                chunk.GetComponent<GreedyRob>().CC_NORTH = gameObject;
            }
        }*/

    /**
     * This function returns an instance of VoxelFace containing the attributes for 
     * one side of a voxel.  In this simple demo we just return a value from the 
     * sample data array.  However, in an actual voxel engine, this function would 
     * check if the voxel face should be culled, and set per-face and per-vertex 
     * values as well as voxel values in the returned instance.
     * 
     * @param x
     * @param y
     * @param z
     * @param face
     * @return 
     */
    VoxelFace getVoxelFace(int x, int y, int z, int side) {

        VoxelFace voxelFace = voxels[x,y,z];
        
        voxelFace.side = side;

        return voxelFace;
    }
    
    /**
     * This function renders a single quad in the scene. This quad may represent many adjacent voxel 
     * faces - so in order to create the illusion of many faces, you might consider using a tiling 
     * function in your voxel shader. For this reason I've included the quad width and height as parameters.
     * 
     * For example, if your texture coordinates for a single voxel face were 0 - 1 on a given axis, they should now 
     * be 0 - width or 0 - height. Then you can calculate the correct texture coordinate in your fragement 
     * shader using coord.xy = fract(coord.xy). 
     * 
     * 
     * @param bottomLeft
     * @param topLeft
     * @param topRight
     * @param bottomRight
     * @param width
     * @param height
     * @param voxel
     * @param backFace 
     */
    void quad( Vector3 bottomLeft, 
               Vector3 topLeft, 
               Vector3 topRight, 
               Vector3 bottomRight,
               int width,
               int height,
               VoxelFace voxel, 
               bool backFace) {
 
        /*Vector3 [] vertices = new Vector3[4];

        vertices[2] = topLeft.multLocal(VOXEL_SIZE);
        vertices[3] = topRight.multLocal(VOXEL_SIZE);
        vertices[0] = bottomLeft.multLocal(VOXEL_SIZE);
        vertices[1] = bottomRight.multLocal(VOXEL_SIZE);*/
        
        _vertices.Add(topLeft);
        _vertices.Add(topRight);
        _vertices.Add(bottomLeft);
        _vertices.Add(bottomRight);

        var vCount = _vertices.Count;

        if (backFace)
        {
            _tris.Add(vCount - 4 + 2);
            _tris.Add(vCount - 4 + 3);
            _tris.Add(vCount - 4 + 1);
            _tris.Add(vCount - 4+ 1);
            _tris.Add(vCount - 4 + 0);
            _tris.Add(vCount - 4 + 2); 
        }
        else
        {
            _tris.Add(vCount - 4 + 2);
            _tris.Add(vCount - 4 + 0);
            _tris.Add(vCount - 4 + 1);
            _tris.Add(vCount - 4+ 1);
            _tris.Add(vCount - 4 + 3);
            _tris.Add(vCount - 4 + 2); 
        }


        //int [] indexes = backFace ? new[] { 2,0,1, 1,3,2 } : new[]{ 2,3,1, 1,0,2 };

        /*float[] colorArray = new float[4*4];
        
        for (int i = 0; i < colorArray.length; i+=4) {
        
            /*
             * Here I set different colors for quads depending on the "type" attribute, just 
             * so that the different groups of voxels can be clearly seen.
             * 
             
            if (voxel.type == 1) {
                
                colorArray[i]   = 1.0f;
                colorArray[i+1] = 0.0f;
                colorArray[i+2] = 0.0f;
                colorArray[i+3] = 1.0f;                
                
            } else if (voxel.type == 2) {
                
                colorArray[i]   = 0.0f;
                colorArray[i+1] = 1.0f;
                colorArray[i+2] = 0.0f;
                colorArray[i+3] = 1.0f;
                
            } else {
            
                colorArray[i]   = 0.0f;
                colorArray[i+1] = 0.0f;
                colorArray[i+2] = 1.0f;
                colorArray[i+3] = 1.0f;                
            }
        }
        
        Mesh mesh = new Mesh();
        
        mesh.setBuffer(Type.Position, 3, BufferUtils.createFloatBuffer(vertices));
        mesh.setBuffer(Type.Color,    4, colorArray);
        mesh.setBuffer(Type.Index,    3, BufferUtils.createIntBuffer(indexes));
        mesh.updateBound();
        
        Geometry geo = new Geometry("ColoredMesh", mesh);
        Material mat = new Material(assetManager, "Common/MatDefs/Misc/Unshaded.j3md");
        mat.setBoolean("VertexColor", true);

        /*
         * To see the actual rendered quads rather than the wireframe, just comment outthis line.
         
        mat.getAdditionalRenderState().setWireframe(true);
        
        geo.setMaterial(mat);

        rootNode.attachChild(geo);*/
    }

    void meshgen()
    {
        mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        
        /*
         * Build the corresponding Box Colliders according to the VoxelFace array.
         */
        chunkColBuilder.CreateCol();
    }
    
    
    
}
