using System;
using System.Linq;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerLogic : MonoBehaviour
{
    [Header("Camera")] 
    public Camera mainCamera;
    [Tooltip("Default is '4'")]
    public float maxCameraReachDistance;

    [Header("Node Layer")] 
    public LayerMask nodeLayer;

    [Header("Connecting Mode")] 
    public KeyCode connectingModeKey;
    
    
    
    private Vector3 rayPos;
    private GameObject focusedNode;
    private GameObject firstNode;
    private GameObject secondNode;
    private bool FocusedObjectSetter;
    private bool connectingSet;

    private bool onConnectingMode;
    
    //VOXELS
    private bool isClickUpLeft;
    private bool isClickUpRight;
    private void Start()
    {
        rayPos = new Vector3(mainCamera.pixelWidth / 2 - 1, mainCamera.pixelHeight / 2 - 1, 0);
    }

    private void Update()
    {
        Ray screenRay = mainCamera.ScreenPointToRay(rayPos);
        RaycastHit hit;
        
        #region Node Hover Checker
        if (Physics.Raycast(screenRay, out hit, maxCameraReachDistance, nodeLayer))
        {
            if (focusedNode != hit.transform.gameObject && FocusedObjectSetter == false && connectingSet == false)
            {
                FocusedObjectSetter = true;
                focusedNode = hit.transform.gameObject;
                var nodeOnHover = focusedNode.transform.gameObject.GetComponent<IINodeOnHover>();
                nodeOnHover?.NodeHover();
            }

            if (hit.transform.gameObject != focusedNode && connectingSet == false)
            {
                FocusedObjectSetter = false;
                hoverFocusedResetter();
            }
            
            if (connectingSet && focusedNode != hit.transform.gameObject)
            {
                FocusedObjectSetter = true;
                focusedNode = hit.transform.gameObject;
                var nodeOnHover = focusedNode.transform.gameObject.GetComponent<IINodeOnHover>();
                nodeOnHover?.NodeHover();
            }
        }
        else if(FocusedObjectSetter && connectingSet == false)
        {
            hoverFocusedResetter();
            
        }else if (FocusedObjectSetter && connectingSet && focusedNode != firstNode)
        {
            connectionFocusedResetter();
            
        }
        #endregion

        #region Node Connecting

        if (Input.GetKeyDown(KeyCode.Mouse0) && focusedNode || connectingSet)
        {
            if (connectingSet == false)
            { 
                firstNode = focusedNode;
            }
            connectingSet = true;
            
        }
        else
        {
            connectingSet = false;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) && firstNode)
        {
            connectingSet = false;
            if (focusedNode != firstNode && focusedNode != null)
            {
                secondNode = focusedNode;
            }

            if (onConnectingMode && secondNode != null)
            {
                SendNodes(firstNode,secondNode);
            }
            resetFirstandSecondNode();
        }

        #endregion

        #region NodeVisibility

        if (Input.GetKeyDown(connectingModeKey) && !onConnectingMode)
        {
            onConnectingMode = true;
            NodeVisibility();
        }else if (Input.GetKeyDown(connectingModeKey)&& onConnectingMode)
        {
            onConnectingMode = false;
            NodeVisibility();
        }

        #endregion

         #region VoxelEdit
        
        if (Input.GetMouseButtonUp(0))
        {
            isClickUpLeft = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isClickUpRight = true;
        }

        RaycastHit chunkHit;
        Ray chunkRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(chunkRay, out chunkHit,maxCameraReachDistance)) {
            if (isClickUpLeft)
            {
                if (!chunkHit.collider.transform.parent.CompareTag("chunk"))
                {
                    isClickUpLeft = false;
                    return;
                }
                /*
                 * Find the corresponding voxel.
                 */
                var CB_TMP = chunkHit.collider.transform.parent.InverseTransformPoint(chunkHit.point);
                var currentBlock = new Vector3(Mathf.Clamp(Mathf.Floor(CB_TMP.x),0f,15f), Mathf.Clamp(Mathf.Floor(CB_TMP.y),0f,15f), Mathf.Clamp(Mathf.Floor(CB_TMP.z),0f,15f));
                
                /*
                 * Destroy the voxel.
                 */
                // ReSharper disable once Unity.InefficientPropertyAccess
                var greedyRob = chunkHit.collider.transform.parent.GetComponent<GreedyRob>();
                /*
                 * Set the voxel type.
                 */
                greedyRob.RemoveBlock((int) currentBlock.x, (int) currentBlock.y, (int) currentBlock.z);
                greedyRob.CreateGreedyMesh();

                isClickUpLeft = false;
            }

            if (isClickUpRight)
            {
                var chunkCreateBool = false;
                if (chunkHit.collider.transform.parent == null ||!chunkHit.collider.transform.parent.CompareTag("chunk"))
                {
                    isClickUpRight = false;
                    return;
                }

                /*
                 * Find the corresponding voxel.
                 */
                var parent = chunkHit.collider.transform.parent;
                var CB_TMP = parent.InverseTransformPoint(chunkHit.point);
                var CB_NORMAL = parent.InverseTransformDirection(chunkHit.normal);
                var flooredVec3 = new Vector3(Mathf.Floor(CB_TMP.x), Mathf.Floor(CB_TMP.y), Mathf.Floor(CB_TMP.z));
                var currentBlock = new Vector3(Mathf.Clamp(flooredVec3.x,0f,15f), Mathf.Clamp(flooredVec3.y,0f,15f), Mathf.Clamp(flooredVec3.z,0f,15f));
                currentBlock += CB_NORMAL - new Vector3(-0.01f,-0.01f,-0.01f);
                currentBlock = new Vector3(Mathf.Clamp(Mathf.Floor(currentBlock.x),0f,15f), Mathf.Clamp(Mathf.Floor(currentBlock.y),0f,15f), Mathf.Clamp(Mathf.Floor(currentBlock.z),0f,15f));
                
                var greedyRob = parent.GetComponent<GreedyRob>();

                /*
                  * Create new Chunk if array is out of array bounds.
                  */
                float[] pos = {CB_TMP.x, CB_TMP.y, CB_TMP.z};
                Array.ForEach(pos, element => {if (element < 0.01f) { chunkCreateBool = true; print(element);} });
                Array.ForEach(pos, element => {if (element >= 15.99f) { chunkCreateBool = true; print(element);} });
                if (chunkCreateBool)
                {
                    print(CB_TMP + " " + CB_NORMAL);
                    if (CB_NORMAL == new Vector3(0, 1, 0))
                    {
                        if (!greedyRob.CC_TOP)
                        {
                            /*
                             * Create the chunk and set the connected Voxels, bottom part adds the voxel.
                             */
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, 4, 0);
                            greedyRob.CC_TOP = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_BOTTOM = greedyRob.gameObject;
                            //Create a block
                            var number = currentBlock.y >= 15 ? 0 : 15;
                            chunkRob.AddBlock((int) currentBlock.x, number,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                            
                        }
                        else
                        {
                            var number = currentBlock.y >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_TOP.GetComponent<GreedyRob>();
                            chunkRob.AddBlock((int) currentBlock.x, number,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                        }
                        
                    }
                    if (CB_NORMAL == new Vector3(0, -1, 0))
                    {
                        if (!greedyRob.CC_BOTTOM)
                        {
                            /*
                             * Create the chunk and set the connected Voxels, bottom part adds the voxel.
                             */
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, -4, 0);
                            greedyRob.CC_BOTTOM = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_TOP = greedyRob.gameObject;
                            //Create a block
                            var number = currentBlock.y >= 15 ? 0 : 15;
                            chunkRob.AddBlock((int) currentBlock.x, number,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                            
                        }
                        else
                        {
                            var number = currentBlock.y >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_BOTTOM.GetComponent<GreedyRob>();
                            chunkRob.AddBlock((int) currentBlock.x, number,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                        }
                    }
                    if (CB_NORMAL == new Vector3(1, 0, 0))
                    {
                        if (!greedyRob.CC_WEST)
                        {
                            /*
                             * Create the chunk and set the connected Voxels, bottom part adds the voxel.
                             */
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(4, 0, 0);
                            greedyRob.CC_WEST = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_EAST = greedyRob.gameObject;
                            //Create a block
                            var number = currentBlock.x >= 15 ? 0 : 15;
                            chunkRob.AddBlock(number, (int)currentBlock.y,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                            
                        }
                        else
                        {
                            var number = currentBlock.x >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_WEST.GetComponent<GreedyRob>();
                            chunkRob.AddBlock(number, (int)currentBlock.y,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                        } 
                    }
                    if (CB_NORMAL == new Vector3(-1, 0, 0))
                    {
                        if (!greedyRob.CC_EAST)
                        {
                            /*
                             * Create the chunk and set the connected Voxels, bottom part adds the voxel.
                             */
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(-4, 0, 0);
                            greedyRob.CC_EAST = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_WEST = greedyRob.gameObject;
                            //Create a block
                            var number = currentBlock.x >= 15 ? 0 : 15;
                            chunkRob.AddBlock(number, (int)currentBlock.y,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                            
                        }
                        else
                        {
                            var number = currentBlock.x >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_EAST.GetComponent<GreedyRob>();
                            chunkRob.AddBlock(number, (int)currentBlock.y,(int) currentBlock.z, 1);
                            chunkRob.CreateGreedyMesh();
                        } 
                    }
                    if (CB_NORMAL == new Vector3(0, 0, 1))
                    {
                        if (!greedyRob.CC_SOUTH)
                        {
                            /*
                             * Create the chunk and set the connected Voxels, bottom part adds the voxel.
                             */
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, 0, 4);
                            greedyRob.CC_SOUTH = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_NORTH = greedyRob.gameObject;
                            //Create a block
                            var number = currentBlock.z >= 15 ? 0 : 15;
                            chunkRob.AddBlock((int)currentBlock.x, (int)currentBlock.y,number, 1);
                            chunkRob.CreateGreedyMesh();
                            
                        }
                        else
                        {
                            var number = currentBlock.z >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_SOUTH.GetComponent<GreedyRob>();
                            chunkRob.AddBlock((int)currentBlock.x, (int)currentBlock.y,number, 1);
                            chunkRob.CreateGreedyMesh();
                        } 
                    }
                    if (CB_NORMAL == new Vector3(0, 0, -1))
                    {
                        if (!greedyRob.CC_NORTH)
                        {
                            /*
                             * Create the chunk and set the connected Voxels, bottom part adds the voxel.
                             */
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, 0, -4);
                            greedyRob.CC_NORTH = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_SOUTH = greedyRob.gameObject;
                            //Create a block
                            var number = currentBlock.z >= 15 ? 0 : 15;
                            chunkRob.AddBlock((int)currentBlock.x, (int)currentBlock.y,number, 1);
                            chunkRob.CreateGreedyMesh();
                            
                        }
                        else
                        {
                            var number = currentBlock.z >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_NORTH.GetComponent<GreedyRob>();
                            chunkRob.AddBlock((int)currentBlock.x, (int)currentBlock.y,number, 1);
                            chunkRob.CreateGreedyMesh();
                        } 
                    }
                }
                /*
                 * Set the voxel type.
                 */
                
                var curVox = greedyRob.GetVoxelByIndex((int) currentBlock.x, (int) currentBlock.y, (int) currentBlock.z);
                if (!curVox.transparent)
                {
                    isClickUpRight = false;
                    return;
                };
                curVox.type = 1;
                curVox.transparent = false;
                
                /*
                 * Start building the mesh.
                 */
                greedyRob.CreateGreedyMesh();
                
                isClickUpRight = false;
            }
        }
        #endregion
    }

    private void NodeVisibility()
    {
        Object[] nodes = FindObjectsOfType(typeof(NodeLogic),true);
        if (onConnectingMode)
        {
            foreach (var VARIABLE in nodes)
            {
                VARIABLE.GameObject().SetActive(true);
            }
        }else if (!onConnectingMode)
        {
            foreach (var VARIABLE in nodes)
            {
                VARIABLE.GameObject().SetActive(false);
            }
        }
    }

    void SendNodes(GameObject firstNodeGameObject, GameObject secondNodeGameObject)
    {
        var send = firstNodeGameObject.transform.parent.GetComponent<IIFirstNode>();
        if (send != null)
        {
            send.FirstNodeSender(firstNodeGameObject, secondNodeGameObject);
        }
        else
        {
            print("nodes not sent!");
        }
    }

    #region Resetters
    void resetFirstandSecondNode()
    {
        var nodeOnLeave = firstNode.transform.gameObject.GetComponent<IINodeOnLeave>();
        nodeOnLeave?.NodeLeave();
        firstNode = null;
        secondNode = null;
    }

    void connectionFocusedResetter()
    {
        FocusedObjectSetter = false;
        var nodeOnLeave = focusedNode.transform.gameObject.GetComponent<IINodeOnLeave>();
        nodeOnLeave?.NodeLeave();
        focusedNode = null;
    }

    void hoverFocusedResetter()
    {
        FocusedObjectSetter = false;
        var nodeOnLeave = focusedNode.transform.gameObject.GetComponent<IINodeOnLeave>();
        nodeOnLeave?.NodeLeave();
        focusedNode = null;
    }
    
    #endregion
}
