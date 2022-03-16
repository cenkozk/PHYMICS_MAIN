using UnityEngine;

public class VoxelData : MonoBehaviour
{
    private static int x = 16;
    private static int y = 16;
    private static int z = 16;
    
    public int[,,] Data = new int[x,y,z];

    private void Awake()
    {
        Data[(x - 1) / 2 + 1, (y - 1) / 2 + 1, (z - 1) / 2 + 1] = 1;

    }

    public void GreedyChecker(int x,int y,int z, Direction dir)
    {
        DataCoordinate offsetToCheck = offsets[(int)dir];
        DataCoordinate neighborCoord = new DataCoordinate(x+offsetToCheck.x,y+offsetToCheck.y,z + offsetToCheck.z);
    }


    public int Height => Data.GetLength(0); //y

    public int Width => Data.GetLength(1); //x
    
    public int Depth => Data.GetLength(2); //z
    
    
    public int GetCell(int x,int y,int z)
    {
        return Data[y,x,z];
    }

    public bool isBlockAt(int x,int y,int z)
    {
        if(Data[y,x,z] == 1)
        {
            return false;
        }
        return true;
    }

    public int GetNeighbor(int x,int y,int z, Direction dir)
    {
        DataCoordinate offsetToCheck = offsets[(int)dir];
        DataCoordinate neighborCoord = new DataCoordinate(x+offsetToCheck.x,y+offsetToCheck.y,z + offsetToCheck.z);
        if (neighborCoord.x < 0 || neighborCoord.x >= Width || neighborCoord.y < 0 || neighborCoord.y >= Height || neighborCoord.z < 0 || neighborCoord.z >= Depth)
        {
            return 0;
        }

        return GetCell(neighborCoord.x,neighborCoord.y,neighborCoord.z);
    }

    struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x,int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    private DataCoordinate[] offsets =
    {
        new DataCoordinate(0, 0, 1),
        new DataCoordinate(1, 0, 0),
        new DataCoordinate(0, 0, -1),
        new DataCoordinate(-1, 0, 0),
        new DataCoordinate(0, 1, 0),
        new DataCoordinate(0, -1, 0),
    };
}


public enum Direction
{
    North,
    East,
    South,
    West,
    Up,
    Down
}


/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class VoxelData : MonoBehaviour
{
    public static readonly int[,] Data = {
        {1,1,1,1,1,0},
        {1,1,1,1,1,1},
        {1,1,1,1,1,1},
        {1,1,1,1,0,0},
        {1,1,1,1,0,0}
    };
    
    public int Width => Data.GetLength(0); //x

    public int Depth => Data.GetLength(1); //z

    public int[,] dataTmp = Data;
    
    public int GetCell(int x,int z)
    {
        return Data[x,z];
    }

    public int GetNeighbor(int x,int y,int z, Direction dir)
    {
        DataCoordinate offsetToCheck = offsets[(int)dir];
        DataCoordinate neighborCoord = new DataCoordinate(x+offsetToCheck.x,y+offsetToCheck.y,z + offsetToCheck.z);
        if (neighborCoord.x < 0 || neighborCoord.x >= Width || neighborCoord.y != 0 || neighborCoord.z < 0 || neighborCoord.z >= Depth)
        {
            return 0;
        }
        else
        {
            return GetCell(neighborCoord.x,neighborCoord.z);
        }
    }

    struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x,int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    private DataCoordinate[] offsets =
    {
        new DataCoordinate(0, 0, 1),
        new DataCoordinate(1, 0, 0),
        new DataCoordinate(0, 0, -1),
        new DataCoordinate(-1, 0, 0),
        new DataCoordinate(0, 1, 0),
        new DataCoordinate(0, -1, 0),
    };
}


public enum Direction
{
    North,
    East,
    South,
    West,
    Up,
    Down
}

 */
