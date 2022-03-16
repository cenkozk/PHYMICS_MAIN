using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Sensitivity")]
    [Tooltip("Default is '1'")]
    [Range(0,5)]public float sensX;
    [Tooltip("Default is '1'")]
    [Range(0,5)]public float sensY;
    
    [Header("Camera")]
    public Transform cam;
    public Transform oriantation;

    private float mouseX;
    private float mouseY;
    
    private float xRotation;
    private float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleInput();
        
        cam.transform.localRotation = Quaternion.Euler(xRotation,yRotation,0);
        oriantation.rotation = Quaternion.Euler(0,yRotation,0);
    }

    private void HandleInput()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX;
        xRotation -= mouseY * sensY;

        xRotation = Mathf.Clamp(xRotation, -90f, +90f);
    }
}
