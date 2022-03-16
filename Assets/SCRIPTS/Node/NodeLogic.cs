using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using UnityEngine;

public class NodeLogic : MonoBehaviour,IINodeOnHover,IINodeOnLeave
{

    [Header("Node Settings")] 
    private float startSize = 0.25f;
    private float inflatedSize = 0.35f;
    
    private float inflateSpeed  = 0.1f;

    #region NodeHoverScaling

    private void Inflate()
    {
        transform.DOScale(inflatedSize, inflateSpeed);
    }
    
    private void Deflate()
    {
        transform.DOScale(startSize, inflateSpeed);
    }
    
    public void NodeHover()
    {
        Inflate();
    }

    public void NodeLeave()
    {
        Deflate();
    }

    #endregion
}
