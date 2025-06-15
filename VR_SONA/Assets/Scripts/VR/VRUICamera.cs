using UnityEngine;

public class VRUICamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform vrCamera; // XR Main Camera
    public bool followPosition = true;
    public bool followRotation = false; // 회전까지 따라할지 (보통 false)
    
    [Header("Positioning")]
    public Vector3 offset = new Vector3(0f, 0.2f, 1.5f); // 카메라 기준 오프셋
    public float followSpeed = 5f; // 따라가는 속도 (1=즉시, 낮을수록 부드러움)
    
    [Header("Rotation Settings")]
    public bool alwaysFacePlayer = true; // 항상 플레이어를 바라보기
    public bool lockYRotationOnly = true; // Y축 회전만 적용
    
    [Header("Distance Settings")]
    public float minDistance = 1f; // 최소 거리
    public float maxDistance = 5f; // 최대 거리
    public bool keepFixedDistance = true; // 고정 거리 유지
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        // VR 카메라 자동 찾기
        if (vrCamera == null)
        {
            vrCamera = Camera.main?.transform;
            if (vrCamera == null)
            {
                vrCamera = FindObjectOfType<Camera>()?.transform;
            }
        }
        
        if (vrCamera == null)
        {
            Debug.LogError("VR Camera를 찾을 수 없습니다!");
            return;
        }
        
        // Canvas를 World Space로 설정 (아직 안되어 있다면)
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = vrCamera.GetComponent<Camera>();
        }
        
        Debug.Log("VR Follow UI 초기화 완료");
    }
    
    void Update()
    {
        if (vrCamera == null) return;
        
        UpdatePosition();
        UpdateRotation();
    }
    
    void UpdatePosition()
    {
        if (!followPosition) return;
        
        // 카메라 기준 오프셋 위치 계산
        Vector3 cameraPosition = vrCamera.position;
        Vector3 cameraForward = vrCamera.forward;
        Vector3 cameraRight = vrCamera.right;
        Vector3 cameraUp = vrCamera.up;
        
        // 오프셋을 카메라 좌표계로 변환
        targetPosition = cameraPosition + 
                        (cameraRight * offset.x) + 
                        (cameraUp * offset.y) + 
                        (cameraForward * offset.z);
        
        // 고정 거리 유지
        if (keepFixedDistance)
        {
            Vector3 direction = (targetPosition - cameraPosition).normalized;
            float desiredDistance = offset.magnitude;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            targetPosition = cameraPosition + direction * desiredDistance;
        }
        
        // 부드럽게 이동
        if (followSpeed >= 10f)
        {
            // 즉시 이동
            transform.position = targetPosition;
        }
        else
        {
            // 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
    
    void UpdateRotation()
    {
        if (!alwaysFacePlayer) 
        {
            // 카메라 회전 따라하기
            if (followRotation)
            {
                targetRotation = vrCamera.rotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
            }
            return;
        }
        
        // 항상 플레이어를 바라보도록 회전
        Vector3 lookDirection = vrCamera.position - transform.position;
        
        if (lockYRotationOnly)
        {
            // Y축 회전만 적용 (UI가 기울어지지 않음)
            lookDirection.y = 0;
        }
        
        if (lookDirection != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
        }
    }
    
    // 오프셋 실시간 조정
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    // 따라가는 속도 조정
    public void SetFollowSpeed(float speed)
    {
        followSpeed = Mathf.Max(0.1f, speed);
    }
    
    // UI 일시정지/재개
    public void SetFollowEnabled(bool enabled)
    {
        followPosition = enabled;
    }
    
    // 거리 조정
    public void SetDistance(float distance)
    {
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        offset = offset.normalized * distance;
    }
    
    // 미리 정의된 위치들
    [ContextMenu("Set Front Position")]
    public void SetFrontPosition()
    {
        offset = new Vector3(0f, 0f, 1.5f); // 앞쪽
    }
    
    [ContextMenu("Set Top Position")]
    public void SetTopPosition()
    {
        offset = new Vector3(0f, 1f, 0.5f); // 위쪽
    }
    
    [ContextMenu("Set Right Position")]
    public void SetRightPosition()
    {
        offset = new Vector3(1f, 0f, 0.5f); // 오른쪽
    }
    
    // 디버그용 기즈모
    void OnDrawGizmosSelected()
    {
        if (vrCamera == null) return;
        
        // 현재 타겟 위치 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.1f);
        
        // 카메라와의 연결선
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(vrCamera.position, transform.position);
        
        // 오프셋 벡터 표시
        Gizmos.color = Color.blue;
        Vector3 offsetWorldPos = vrCamera.position + vrCamera.TransformDirection(offset);
        Gizmos.DrawLine(vrCamera.position, offsetWorldPos);
    }
}