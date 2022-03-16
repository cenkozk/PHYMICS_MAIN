using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header(("Movement"))] 
    [Tooltip("Default move speed is '0,3' for walking")]
    [Range(0,1)]public float moveSpeed = 0.3f;
    public Transform oriantation;

    [Header("Jump")] 
    [Tooltip("Default is '100'")]
    public float jumpForce = 100f;
    public float jumpFloatRadius = 0.245f;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Player Settings")]
    [Tooltip("Default is '2'")]
    public float playerHeight = 2;

    public LayerMask playerGroundTouchLayer;
    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 0f;
    [Header("Physic Material")]
    public PhysicMaterial characterPhysicMaterial;
    [Tooltip("Default is '0.25f")]
    public float slopeFriction = 0.25f;
    public float groundFriction = 0;
    [Header("Headbob Settings")] 
    public bool enableHeadBob = true;
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public float headBobShakeFrequency = 0.04f;
    

    //
    private float moveMultiplier = 100f;
    private bool isGrounded;
    private float horizontalMovement;
    private float verticalMovement;
    private CinemachineBasicMultiChannelPerlin cinemachineHeadBobNoise;
    private bool HeadBobSetter;

    private Vector3 moveDirection;
    private Vector3 slopeMoveDirection;

    private Rigidbody rb;

    RaycastHit slopeHit;
    
    private bool OnSlope()//Check for slope.
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 1f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cinemachineHeadBobNoise = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position-new Vector3(0,1,0),jumpFloatRadius,playerGroundTouchLayer);
        HandleInput();
        ControlDrag();

        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            Jump();
        }

        #region Headbob

        if (Mathf.Abs(rb.velocity.magnitude) > 1 && enableHeadBob && HeadBobSetter == false && isGrounded)
        {
            if (Mathf.Abs(horizontalMovement) > 0 || Mathf.Abs(verticalMovement) > 0)
            {
                StartHeadBobShake();
                HeadBobSetter = true;  
            }
        }
        else if (HeadBobSetter && enableHeadBob && Mathf.Abs(horizontalMovement) == 0 && Mathf.Abs(verticalMovement) == 0)
        {
            ResetHeadBobShake();
            HeadBobSetter = false;
        }

        #endregion
        
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void StartHeadBobShake()
    {
        cinemachineHeadBobNoise.m_FrequencyGain = headBobShakeFrequency;
    }
    
    private void ResetHeadBobShake()
    {
        cinemachineHeadBobNoise.m_FrequencyGain = 0;
    }

    private void ControlDrag()
    {
        if (isGrounded == false)
        {
            rb.drag = airDrag;
        }
        else
        {
            rb.drag = groundDrag;
        }
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce,ForceMode.VelocityChange);
    }

    private void HandleInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = oriantation.forward * verticalMovement + oriantation.right * horizontalMovement;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        float localMoveSpeed;
        if (isGrounded && !OnSlope())//Set the speed if we are in the air.
        {
            localMoveSpeed = moveSpeed * moveMultiplier;
            Vector3 velocity = moveDirection.normalized * localMoveSpeed;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;
            //rb.AddForce(moveDirection.normalized * localMoveSpeed,ForceMode.Acceleration);
            characterPhysicMaterial.dynamicFriction = groundFriction;//Prevent sliding
        }else if (isGrounded && OnSlope())
        {
            localMoveSpeed = moveSpeed * moveMultiplier;
            //rb.AddForce(slopeMoveDirection.normalized * localMoveSpeed,ForceMode.Acceleration);
            Vector3 velocity = moveDirection.normalized * localMoveSpeed;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;
            characterPhysicMaterial.dynamicFriction = slopeFriction;//Prevent sliding
        }
        else if(!isGrounded)
        {
            //localMoveSpeed = moveSpeed / 3 * moveMultiplier;
            localMoveSpeed = moveSpeed * moveMultiplier;
            //rb.AddForce(moveDirection.normalized * localMoveSpeed,ForceMode.Acceleration);
            Vector3 velocity = moveDirection.normalized * localMoveSpeed;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;
            characterPhysicMaterial.dynamicFriction = groundFriction;//Prevent sliding
        }
        
    }
}
