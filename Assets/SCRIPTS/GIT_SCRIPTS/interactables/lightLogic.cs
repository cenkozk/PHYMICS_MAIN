using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class lightLogic : MonoBehaviour,IIisOccupied,IIOccupyOutput,IIReturnOccupiedObject
{
    public bool isOccupiedBool;
    public GameObject OccupiedByGameObject;
    
    
    
    
    
    
    
    
    //INTERFACES & NODE CONNECTIONS!
    public bool isOccupied()
    {
        if (isOccupiedBool) return true;
        return false;
    }

    public void Occupy(GameObject occupiedBy)
    {
        if (!isOccupiedBool)
        {
            isOccupiedBool = true;
            OccupiedByGameObject = occupiedBy; 
        }
        else
        {
            isOccupiedBool = false;
            OccupiedByGameObject = null; 
        }
        
    }

    public GameObject returnOccupiedObject()
    {
        return OccupiedByGameObject;
    }
}
