using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk_Manager : MonoBehaviour
{
    void Start()
    {
        var rob = gameObject.GetComponentInChildren<GreedyRob>();
        rob.Starter();
        rob.FirstChunk();
        rob.CreateGreedyMesh();
    }

}
