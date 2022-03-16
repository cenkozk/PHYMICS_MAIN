using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float time = 5;
    void Start()
    {
        var rb = gameObject.GetComponent<Rigidbody>();
        DOTweenModulePhysics.DOMoveX(rb, rb.position.x + 10, time).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
