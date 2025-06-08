using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty moveInput; // Vector2 (예: Left Controller Joystick)

    [Header("Movement")]
    public float moveSpeed = 1.5f;

    void Update()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();

        if (input == Vector2.zero)
            return;

        // 카메라 기준 전후좌우 방향 얻기
        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
