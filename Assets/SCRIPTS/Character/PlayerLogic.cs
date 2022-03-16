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
                var curVox = greedyRob.voxels[(int) currentBlock.x, (int) currentBlock.y, (int) currentBlock.z];
                /*
                 * Set the voxel type.
                 */
                print("Block ID: " + curVox.ID);
                curVox.type = 0;
                curVox.transparent = true;
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
                Array.ForEach(pos, element => {if (element < 0.2f) { chunkCreateBool = true; } });
                Array.ForEach(pos, element => {if (element > 15f) { chunkCreateBool = true; } });
                if (chunkCreateBool)
                {
                    if (CB_NORMAL == new Vector3(0, 1, 0))
                    {
                        if (!greedyRob.CC_TOP)
                        {
                            var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                            chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, 4, 0);
                            greedyRob.CC_TOP = chunk;
                            var chunkRob = chunk.GetComponent<GreedyRob>();
                            chunkRob.Starter();
                            chunkRob.PopulateVoxelArray();
                            chunkRob.CreateGreedyMesh();
                            chunkRob.CC_BOTTOM = greedyRob.gameObject;
                        }
                        else
                        {
                            var number = currentBlock.y >= 15 ? 0 : 15;
                            var chunkRob = greedyRob.CC_TOP.GetComponent<GreedyRob>();
                            var chunkVox = chunkRob.voxels[(int) currentBlock.x, number, (int) currentBlock.z];
                            print(new Vector3((int) currentBlock.x, number, (int) currentBlock.z));
                            chunkVox.type = 1;
                            chunkVox.transparent = false;
                            chunkRob.CreateGreedyMesh();
                        }
                        
                    }
                    if (CB_NORMAL == new Vector3(0, -1, 0) && !greedyRob.CC_BOTTOM)
                    {
                        var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                        chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, -4, 0);
                        greedyRob.CC_BOTTOM = chunk;
                        chunk.GetComponent<GreedyRob>().CC_TOP = greedyRob.gameObject;
                    }
                    if (CB_NORMAL == new Vector3(1, 0, 0) && !greedyRob.CC_WEST)
                    {
                        var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                        chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(4, 0, 0);
                        greedyRob.CC_WEST = chunk;
                        chunk.GetComponent<GreedyRob>().CC_EAST = greedyRob.gameObject; 
                    }
                    if (CB_NORMAL == new Vector3(-1, 0, 0) && !greedyRob.CC_EAST)
                    {
                        var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                        chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(-4, 0, 0);
                        greedyRob.CC_EAST = chunk;
                        chunk.GetComponent<GreedyRob>().CC_WEST = greedyRob.gameObject;
                    }
                    if (CB_NORMAL == new Vector3(0, 0, 1) && !greedyRob.CC_SOUTH)
                    {
                        var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                        chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, 0, 4);
                        greedyRob.CC_SOUTH = chunk;
                        chunk.GetComponent<GreedyRob>().CC_NORTH = greedyRob.gameObject;
                    }
                    if (CB_NORMAL == new Vector3(0, 0, -1) && !greedyRob.CC_NORTH)
                    {
                        var chunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, greedyRob.gameObject.transform.parent);
                        chunk.transform.localPosition = greedyRob.gameObject.transform.localPosition + new Vector3(0, 0, -4);
                        greedyRob.CC_NORTH = chunk;
                        chunk.GetComponent<GreedyRob>().CC_SOUTH = greedyRob.gameObject;
                    }
                }
                print(currentBlock);
                /*
                 * Set the voxel type.
                 */
                
                var curVox = greedyRob.voxels[(int) currentBlock.x, (int) currentBlock.y, (int) currentBlock.z];
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
