using UnityEngine;
using System.Collections.Generic;

//Basic struct for storing 3 unsigned integers.
public struct UIntVec3
{
  public uint x;
  public uint y;
  public uint z;

  public UIntVec3(uint x, uint y, uint z)
  {
    this.x = x;
    this.y = y;
    this.z = z;
  }

  //Allows the vector to be converted to the format that Unity3D allows.
  public static explicit operator Vector3(UIntVec3 vec)
  {
    return new Vector3(vec.x, vec.y, vec.z);
  }
}

public class ChunkColBuilder : MonoBehaviour
{
  private GreedyRob.VoxelFace[,,] voxelsCol; 
  private GreedyRob greedyRob;
  private uint SIZE;
  public const byte VOXEL_Y_SHIFT = 4;
  public const byte VOXEL_Z_SHIFT = 8;
  
  private List<BoxCollider> m_colliders = new();
  

  public void CreateCol()
  {
    greedyRob = GetComponent<GreedyRob>();
    voxelsCol = greedyRob.voxels;
    SIZE = (uint) greedyRob.CHUNK_SIZE;

    GenerateMesh();       //Generates and attaches the mesh.
  }

  //Returns the position of a voxel in the array from its 3D co-ordinates.
  private static uint GetVoxelDataIndex(uint x, uint y, uint z)
  {
    return x | y << VOXEL_Y_SHIFT | z << VOXEL_Z_SHIFT;
  }

  //Returns the position of a voxel in 3D co-ordinates from its index in the array.
  private static UIntVec3 GetVoxelDataPosition(uint index)
  {
    uint blockX = index & 0xF;
    uint blockY = (index >> VOXEL_Y_SHIFT) & 0xF;
    uint blockZ = (index >> VOXEL_Z_SHIFT) & 0xF;
    return new UIntVec3(blockX, blockY, blockZ);
  }
  

  //Generates the collision boxes for the mesh and applies them.
  public void GenerateMesh()
  {
    //Keeps track of whether a voxel has been checked.
    bool[] tested = new bool[SIZE * SIZE * SIZE];
    Dictionary<UIntVec3, UIntVec3> boxes = new Dictionary<UIntVec3, UIntVec3>();
    for (uint index = 0; index < tested.Length; ++index)
    {
      if (!tested[index])
      {
        var a = index % 16;  
        var iT = index / 16;
        var j = iT % 16; 
        var k  = iT / 16;
        tested[index] = true;
        if (!voxelsCol[a,j,k].transparent)  //If the voxel contributes to the collision mesh.
        {
          UIntVec3 boxStart = GetVoxelDataPosition(index);
          UIntVec3 boxSize = new UIntVec3(1, 1, 1);
          bool canSpreadX = true;
          bool canSpreadY = true;
          bool canSpreadZ = true;
          //Attempts to expand in all directions and stops in each direction when it no longer can.
          while (canSpreadX || canSpreadY || canSpreadZ)
          {
            canSpreadX = TrySpreadX(canSpreadX, ref tested, boxStart, ref boxSize);
            canSpreadY = TrySpreadY(canSpreadY, ref tested, boxStart, ref boxSize);
            canSpreadZ = TrySpreadZ(canSpreadZ, ref tested, boxStart, ref boxSize);
          }
          boxes.Add(boxStart, boxSize);
        }
      }
    }
    SetCollisionMesh(boxes);    //Applies the collision boxes.
  }

  //Returns whether the box can continue to spread along the positive X axis.
  private bool TrySpreadX(bool canSpreadX, ref bool[] tested, UIntVec3 boxStart, ref UIntVec3 boxSize)
  {
    //Checks the square made by the Y and Z size on the X index one larger than the size of the
    //box.
    uint yLimit = boxStart.y + boxSize.y;
    uint zLimit = boxStart.z + boxSize.z;
    for (uint y = boxStart.y; y < yLimit && canSpreadX; ++y)
    {
      for (uint z = boxStart.z; z < zLimit; ++z)
      {
        uint newX = boxStart.x + boxSize.x;
        uint newIndex = GetVoxelDataIndex(newX, y, z);
        var a = newIndex % 16;  
        var iT = newIndex / 16;
        var j = iT % 16; 
        var k  = iT / 16;
        if (newX >= SIZE || tested[newIndex] || voxelsCol[a,j,k].transparent)
        {
          canSpreadX = false;
        }
      }
    }
    //If the box can spread, mark it as tested and increase the box size in the X dimension.
    if (canSpreadX)
    {
      for (uint y = boxStart.y; y < yLimit; ++y)
      {
        for (uint z = boxStart.z; z < zLimit; ++z)
        {
          uint newX = boxStart.x + boxSize.x;
          uint newIndex = GetVoxelDataIndex(newX, y, z);
          tested[newIndex] = true;
        }
      }
      ++boxSize.x;
    }
    return canSpreadX;
  }

  //Returns whether the box can continue to spread along the positive Y axis.
  private bool TrySpreadY(bool canSpreadY, ref bool[] tested, UIntVec3 boxStart, ref UIntVec3 boxSize)
  {
    //Checks the square made by the X and Z size on the Y index one larger than the size of the
    //box.
    uint xLimit = boxStart.x + boxSize.x;
    uint zLimit = boxStart.z + boxSize.z;
    for (uint x = boxStart.x; x < xLimit && canSpreadY; ++x)
    {
      for (uint z = boxStart.z; z < zLimit; ++z)
      {
        uint newY = boxStart.y + boxSize.y;
        uint newIndex = GetVoxelDataIndex(x, newY, z);
        var a = newIndex % 16;  
        var iT = newIndex / 16;
        var j = iT % 16; 
        var k  = iT / 16;
        if (newY >= SIZE || tested[newIndex] || voxelsCol[a,j,k].transparent)
        {
          canSpreadY = false;
        }
      }
    }
    //If the box can spread, mark it as tested and increase the box size in the Y dimension.
    if (canSpreadY)
    {
      for (uint x = boxStart.x; x < xLimit; ++x)
      {
        for (uint z = boxStart.z; z < zLimit; ++z)
        {
          uint newY = boxStart.y + boxSize.y;
          uint newIndex = GetVoxelDataIndex(x, newY, z);
          tested[newIndex] = true;
        }
      }
      ++boxSize.y;
    }
    return canSpreadY;
  }

  //Returns whether the box can continue to spread along the positive Z axis.
  private bool TrySpreadZ(bool canSpreadZ, ref bool[] tested, UIntVec3 boxStart, ref UIntVec3 boxSize)
  {
    //Checks the square made by the X and Y size on the Z index one larger than the size of the
    //box.
    uint xLimit = boxStart.x + boxSize.x;
    uint yLimit = boxStart.y + boxSize.y;
    for (uint x = boxStart.x; x < xLimit && canSpreadZ; ++x)
    {
      for (uint y = boxStart.y; y < yLimit; ++y)
      {
        uint newZ = boxStart.z + boxSize.z;
        uint newIndex = GetVoxelDataIndex(x, y, newZ);
        var a = newIndex % 16;  
        var iT = newIndex / 16;
        var j = iT % 16; 
        var k  = iT / 16;
        if (newZ >= SIZE || tested[newIndex] || voxelsCol[a,j,k].transparent)
        {
          canSpreadZ = false;
        }
      }
    }
    //If the box can spread, mark it as tested and increase the box size in the Z dimension.
    if (canSpreadZ)
    {
      for (uint x = boxStart.x; x < xLimit; ++x)
      {
        for (uint y = boxStart.y; y < yLimit; ++y)
        {
          uint newZ = boxStart.z + boxSize.z;
          uint newIndex = GetVoxelDataIndex(x, y, newZ);
          tested[newIndex] = true;
        }
      }
      ++boxSize.z;
    }
    return canSpreadZ;
  }

  //Applies the boxes passed to it to the collision mesh, reusing old boxes where it can.
  private void SetCollisionMesh(Dictionary<UIntVec3, UIntVec3> boxData)
  {
    int colliderIndex = 0;
    int existingColliderCount = m_colliders.Count;
    foreach (KeyValuePair<UIntVec3, UIntVec3> box in boxData)
    {
      //Position is the centre of the box collider for Unity3D.
      Vector3 position = (Vector3)box.Key + ((Vector3)box.Value / 2.0f);
      if (colliderIndex < existingColliderCount)  //If an old collider can be reused.
      {
        m_colliders[colliderIndex].center = position;
        m_colliders[colliderIndex].size = (Vector3)box.Value - new Vector3(0.01f,0.01f,0.01f);
      }
      else  //Else if there were more boxes on this mesh generation than there were on the previous one.
      {
        GameObject boxObject = new GameObject(string.Format("Collider {0}", colliderIndex));
        BoxCollider boxCollider = boxObject.AddComponent<BoxCollider>();
        Transform boxTransform = boxObject.transform;
        boxTransform.parent = transform;
        boxTransform.localScale = new Vector3(1f,1f,1f);
        boxTransform.localPosition = new Vector3();
        boxTransform.localRotation = new Quaternion();
        boxCollider.center = position;
        boxCollider.size = (Vector3)box.Value - new Vector3(0.01f,0.01f,0.01f);
        m_colliders.Add(boxCollider);
      }
      ++colliderIndex;
    }
    //Deletes all the unused boxes if this mesh generation had less boxes than the previous one.
    if (colliderIndex < existingColliderCount)
    {
      for (int i = existingColliderCount - 1; i >= colliderIndex; --i)
      {
        Destroy(m_colliders[i].gameObject);
      }
      m_colliders.RemoveRange(colliderIndex, existingColliderCount - colliderIndex);
    }
  }
}