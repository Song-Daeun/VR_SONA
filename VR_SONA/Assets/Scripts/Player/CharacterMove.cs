using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMove : MonoBehaviour
{
    public Transform cameraTransform;
    public float moveSpeed = 20f;
    public float jumpSpeed = 6f;
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    Vector3 currentVelocity = Vector3.zero;
    float smoothTime = 0.1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        StartCoroutine(SnapToGroundAfterPhysics());
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -4f;  // 바닥에 붙게 살짝 눌러줌
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(h, 0, v);

        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            inputDirection = camForward * v + camRight * h;
        }

        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        Vector3 targetDirection = inputDirection * moveSpeed;
        // Vector3 smoothedDirection = Vector3.SmoothDamp(Vector3.zero, targetDirection, ref currentVelocity, smoothTime);
        Vector3 smoothedDirection = Vector3.SmoothDamp(currentVelocity * 0.5f, targetDirection, ref currentVelocity, smoothTime);

        controller.Move(smoothedDirection * Time.deltaTime);

        // 점프
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpSpeed;
        }

        // 이동 디버깅용
        // 움직임 디버깅
        if (smoothedDirection.magnitude > 0.1f)
        {
            Debug.Log($"이동 방향: {smoothedDirection}, 속도: {smoothedDirection.magnitude}");
        }

        controller.Move(smoothedDirection * Time.deltaTime);

        // 점프
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpSpeed;
            Debug.Log("점프!");
        }
        
        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // 추가된 부분
    private IEnumerator SnapToGroundAfterPhysics()
    {
        yield return new WaitForFixedUpdate(); // 물리 업데이트 후 실행

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1f;

        // 디버그용 Ray 그리기 (씬 뷰에서 빨간 선으로 확인 가능)
        Debug.DrawRay(origin, Vector3.down * 10f, Color.red, 3f);

        if (Physics.Raycast(origin, Vector3.down, out hit, 10f))
        {
            Debug.Log("Ray hit: " + hit.collider.name);  // 무엇을 맞췄는지 확인

            Vector3 newPos = hit.point + Vector3.up * controller.height / 2f;
            transform.position = newPos;
        }
        else
        {
            Debug.LogWarning("Ray가 아무것도 맞추지 못했어요!");
        }
    }
}
