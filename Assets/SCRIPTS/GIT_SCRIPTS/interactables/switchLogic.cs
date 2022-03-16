using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class switchLogic : MonoBehaviour,IIFirstNode
{
    public int OUTPUT = 0;
    public List<GameObject> OutputObjects = new List<GameObject>();
    private void receiver(GameObject firstNode, GameObject secondNode)
    {
        if (!OutputObjects.Contains(secondNode))
        {
            print(secondNode.name + " added.");
            OutputObjects.Add(secondNode);
        }
        else
        {
            print(secondNode.name + " removed.");
            OutputObjects.Remove(secondNode);
        }
    }
    
    public void FirstNodeSender(GameObject firstNode, GameObject secondNode)
    {
        if(firstNode.transform.parent.gameObject != gameObject) return; //if first node is not this object, return!
        if(secondNode.CompareTag("input")) return;  //if second node is type of input, return! (can't connect input -> input.)
        if (secondNode.CompareTag("output"))
        {
            var parent = secondNode.transform.parent;
            var interactBool = parent.GetComponent<IIisOccupied>();
            var interactObject = parent.GetComponent<IIReturnOccupiedObject>();
            if (interactBool != null && !interactBool.isOccupied() || interactObject.returnOccupiedObject() == gameObject)
            {
                secondNode.transform.parent.GetComponent<IIOccupyOutput>().Occupy(firstNode.transform.parent.gameObject);
            }
            else
            {
                print("occupied!");
                return;
            }
        }
        receiver(firstNode.transform.parent.gameObject,secondNode.transform.parent.gameObject);
    }
}
