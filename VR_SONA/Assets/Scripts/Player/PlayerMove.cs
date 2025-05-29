using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public XROrigin xrOrigin;
    public Transform playerVisual; // 외부 시각 모델 (예: BoyPlayer)

    [Header("Movement Control")]
    public bool enableMovement = false; // 이동 기능을 완전히 끌 수 있는 스위치

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

        // 이동 기능이 활성화된 경우에만 입력을 활성화합니다
        if (enableMovement)
        {
            moveInput.action.Enable();
            runInput.action.Enable();
            jumpInput.action.Enable();
        }
    }

    void Update()
    {
        // 항상 시각적 동기화는 수행합니다
        UpdatePlayerVisual();

        // 이동 기능이 비활성화되어 있으면 이동 처리를 건너뜁니다
        if (!enableMovement)
        {
            // 중력만 적용하여 플레이어가 공중에 떠있지 않도록 합니다
            HandleGravityOnly();
            return;
        }

        // 기존 이동 로직
        HandleMovement();
    }

    private void HandleMovement()
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
    }

    private void HandleGravityOnly()
    {
        // 이동은 하지 않지만 중력은 적용합니다
        if (controller.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 gravityMove = Vector3.up * verticalVelocity;
        controller.Move(gravityMove * Time.deltaTime);
    }

    private void UpdatePlayerVisual()
    {
        // 외부 시각 모델을 XR 헤드셋의 움직임에 맞춰 동기화합니다
        if (playerVisual != null && xrOrigin != null)
        {
            Transform head = xrOrigin.Camera.transform;
            playerVisual.position = transform.position;
            playerVisual.rotation = Quaternion.Euler(0, head.eulerAngles.y, 0);
        }
    }

    // 외부에서 이동 기능을 제어할 수 있는 메서드들
    public void EnableMovement(bool enable)
    {
        enableMovement = enable;
        
        if (enable)
        {
            moveInput.action.Enable();
            runInput.action.Enable();
            jumpInput.action.Enable();
        }
        else
        {
            moveInput.action.Disable();
            runInput.action.Disable();
            jumpInput.action.Disable();
        }
    }
}