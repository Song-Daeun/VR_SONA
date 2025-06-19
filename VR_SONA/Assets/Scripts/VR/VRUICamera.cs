using UnityEngine;

public class VRUICamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform vrCamera; 
    public bool followPosition = true;
    public bool followRotation = false; 
    
    [Header("Positioning")]
    public Vector3 offset = new Vector3(0f, 0.2f, 1.5f); 
    public float followSpeed = 5f; 
    
    [Header("Rotation Settings")]
    public bool alwaysFacePlayer = true; 
    public bool lockYRotationOnly = true; 
    
    [Header("Distance Settings")]
    public float minDistance = 1f; 
    public float maxDistance = 5f; 
    public bool keepFixedDistance = true;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
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
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = vrCamera.GetComponent<Camera>();
        }
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
        offset = new Vector3(0f, 0f, 1.5f); 
    }
    
    [ContextMenu("Set Top Position")]
    public void SetTopPosition()
    {
        offset = new Vector3(0f, 1f, 0.5f); 
    }
    
    [ContextMenu("Set Right Position")]
    public void SetRightPosition()
    {
        offset = new Vector3(1f, 0f, 0.5f);
    }
}