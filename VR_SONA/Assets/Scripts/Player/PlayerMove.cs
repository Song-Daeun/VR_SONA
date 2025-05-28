using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public XROrigin xrOrigin;
    public Transform playerVisual; // 외부 시각 모델 (예: BoyPlayer)

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
    }

    void Update()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();
        bool isRunning = runInput.action.ReadValue<float>() > 0.1f;
        bool jumpPressed = jumpInput.action.triggered;

        float speed = moveSpeed * (isRunning ? runMultiplier : 1f);

        // XR 카메라 방향 기준 이동
        Transform head = xrOrigin.Camera.transform;
        Vector3 forward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 right = new Vector3(head.right.x, 0, head.right.z).normalized;
        Vector3 move = (forward * input.y + right * input.x) * speed;

        // 중력 및 점프 처리
        if (controller.isGrounded)
            verticalVelocity = jumpPressed ? jumpPower : -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;

        // CharacterController 기반 이동
        controller.Move(move * Time.deltaTime);

        // 외부 시각 모델 위치 및 회전 동기화
        if (playerVisual != null)
        {
            playerVisual.position = transform.position;
            playerVisual.rotation = Quaternion.Euler(0, head.eulerAngles.y, 0);
        }
    }
}
