using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public XROrigin xrOrigin;
    public Transform playerVisual; // BoyPlayer

    public float moveSpeed = 2.0f;
    public float runMultiplier = 1.5f;
    public float jumpPower = 5.0f;
    public float gravity = -9.81f;

    public InputActionProperty moveInput;
    public InputActionProperty runInput;
    public InputActionProperty jumpInput;

    private CharacterController controller;
    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (xrOrigin == null) xrOrigin = GetComponent<XROrigin>();

        moveInput.action.Enable();
        runInput.action.Enable();
        jumpInput.action.Enable();
        
        // 값 확인
        Debug.Log($"Move Input 활성화 상태: {moveInput.action.enabled}");

    }

    void Update()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();
        bool isRunning = runInput.action.ReadValue<float>() > 0.1f;
        bool jumpPressed = jumpInput.action.triggered;

        float speed = moveSpeed * (isRunning ? runMultiplier : 1f);

        // 카메라 방향 기준 이동
        Transform head = xrOrigin.Camera.transform;
        Vector3 forward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 right = new Vector3(head.right.x, 0, head.right.z).normalized;

        Vector3 move = (forward * input.y + right * input.x) * speed;

        // 중력, 점프
        if (controller.isGrounded)
            verticalVelocity = jumpPressed ? jumpPower : -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        // 강제로 위치 동기화
        if (playerVisual != null)
        {
            playerVisual.transform.position = transform.position;
            playerVisual.transform.rotation = Quaternion.Euler(0, head.eulerAngles.y, 0);
        
            Debug.Log($"XR Origin 위치: {transform.position}, BoyPlayer 위치: {playerVisual.transform.position}");

        }
    }
}
