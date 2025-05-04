using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform visualModel;

    public float moveSpeed = 5f;
    public float runMultiplier = 2f;
    public float rotationSpeed = 120f;
    public float jumpPower = 10f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;

    private PlayerActions inputActions;
    private bool isUsingVR = false;

    void Awake()
    {
        inputActions = new PlayerActions();
        isUsingVR = XRSettings.isDeviceActive;

        if (isUsingVR)
            inputActions.XRIControls.Enable();
        else
            inputActions.PCControls.Enable();
    }

    void OnEnable()
    {
        if (isUsingVR)
            inputActions.XRIControls.Enable();
        else
            inputActions.PCControls.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    // void Update()
    // {
    //     MOVE();
    //     JUMP();
    //     ApplyGravity();
    //     controller.Move(moveDirection * Time.deltaTime);

    //     if (visualModel != null)
    //     {
    //         visualModel.position = transform.position;
    //         visualModel.rotation = transform.rotation;
    //     }
    //     ResetXZ();

    //     Vector2 moveInput = isUsingVR
    //         ? inputActions.XRIControls.Move.ReadValue<Vector2>()
    //         : inputActions.PCControls.Move.ReadValue<Vector2>();

    //     Debug.Log($"[TEST] moveInput = {moveInput}");

        
    // }

    void Update()
    {
        Vector3 moveXZ = MOVE();
        JUMP();
        ApplyGravity();

        Vector3 finalMove = moveXZ + Vector3.up * moveDirection.y;
        controller.Move(finalMove * Time.deltaTime);

        if (visualModel != null)
        {
            visualModel.position = transform.position;
            visualModel.rotation = transform.rotation;
        }

        Debug.Log($"[MOVE CHECK] moveXZ = {moveXZ}, moveDirection.y = {moveDirection.y}");

    }


    private Vector3 MOVE()
    {
        Vector2 moveInput;
        float rotateInput;
        bool isRunning;

        if (isUsingVR)
        {
            moveInput = inputActions.XRIControls.Move.ReadValue<Vector2>();
            rotateInput = inputActions.XRIControls.Rotate.ReadValue<float>();
            isRunning = inputActions.XRIControls.Run.ReadValue<float>() > 0;
        }
        else
        {
            moveInput = inputActions.PCControls.Move.ReadValue<Vector2>();
            rotateInput = inputActions.PCControls.Rotate.ReadValue<float>();
            isRunning = inputActions.PCControls.Run.ReadValue<float>() > 0;
        }

        float finalSpeed = moveSpeed * (isRunning ? runMultiplier : 1f);

        Vector3 forward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 move = forward * moveInput.y;

        // Rotate
        if (Mathf.Abs(rotateInput) > 0.1f)
        {
            transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime);
        }

        return move * finalSpeed;  
    }

    private void JUMP()
    {
        bool jumpPressed = isUsingVR
            ? inputActions.XRIControls.Jump.triggered
            : inputActions.PCControls.Jump.triggered;

        if (IsGrounded() && jumpPressed)
        {
            moveDirection.y = jumpPower;
        }
    }

    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        else if (moveDirection.y < 0)
        {
            moveDirection.y = -1f;
        }
    }

    private void ResetXZ()
    {
        moveDirection.x = 0;
        moveDirection.z = 0;
    }

    private bool IsGrounded()
    {
        return controller.isGrounded;
    }
}
