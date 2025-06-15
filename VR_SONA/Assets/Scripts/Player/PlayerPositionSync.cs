using System.Collections;
using UnityEngine;

public class PlayerPositionSync : MonoBehaviour
{
    [Header("동기화 설정")]
    public Transform playerTransform; // Player 오브젝트
    public Transform playerPositionTransform; // PlayerPosition 오브젝트
    
    [Header("동기화 옵션")]
    public bool syncPosition = true;
    public bool syncRotation = false; // 회전은 보통 동기화 안 함
    public bool smoothSync = true;
    public float syncSpeed = 10f;
    
    [Header("디버그")]
    public bool showDebugLogs = false;

    private void Start()
    {
        // 컴포넌트 자동 할당
        if (playerTransform == null)
            playerTransform = this.transform; // Player 오브젝트가 이 스크립트를 가지고 있다고 가정
            
        if (playerPositionTransform == null)
            playerPositionTransform = transform.Find("PlayerPosition");
            
        ValidateComponents();
    }

    private void Update()
    {
        if (!CanSync()) return;
        
        SynchronizeTransforms();
    }

    private void SynchronizeTransforms()
    {
        if (syncPosition)
        {
            if (smoothSync)
            {
                // 부드러운 동기화
                playerTransform.position = Vector3.Lerp(
                    playerTransform.position, 
                    playerPositionTransform.position, 
                    syncSpeed * Time.deltaTime
                );
            }
            else
            {
                // 즉시 동기화
                playerTransform.position = playerPositionTransform.position;
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"위치 동기화: Player -> {playerTransform.position}");
            }
        }
        
        if (syncRotation)
        {
            if (smoothSync)
            {
                playerTransform.rotation = Quaternion.Lerp(
                    playerTransform.rotation, 
                    playerPositionTransform.rotation, 
                    syncSpeed * Time.deltaTime
                );
            }
            else
            {
                playerTransform.rotation = playerPositionTransform.rotation;
            }
        }
    }

    private bool CanSync()
    {
        return playerTransform != null && 
               playerPositionTransform != null && 
               (syncPosition || syncRotation);
    }

    private void ValidateComponents()
    {
        if (playerTransform == null)
        {
            Debug.LogError("PlayerPositionSync: playerTransform이 설정되지 않았습니다!");
        }
        
        if (playerPositionTransform == null)
        {
            Debug.LogError("PlayerPositionSync: playerPositionTransform이 설정되지 않았습니다!");
        }
        
        if (CanSync())
        {
            Debug.Log($"PlayerPositionSync 초기화 완료!");
            Debug.Log($"Player: {playerTransform.name}");
            Debug.Log($"PlayerPosition: {playerPositionTransform.name}");
        }
    }

    // PlayerManager가 이동을 시작할 때 호출
    public void OnPlayerMovementStarted()
    {
        if (showDebugLogs)
        {
            Debug.Log("플레이어 이동 시작 - 동기화 활성화");
        }
    }

    // PlayerManager가 이동을 완료했을 때 호출
    public void OnPlayerMovementCompleted()
    {
        if (showDebugLogs)
        {
            Debug.Log("플레이어 이동 완료 - 최종 동기화");
        }
        
        // 이동 완료 후 정확한 위치로 최종 동기화
        if (CanSync() && syncPosition)
        {
            playerTransform.position = playerPositionTransform.position;
        }
    }

    // 수동으로 즉시 동기화
    public void ForceSynchronize()
    {
        if (!CanSync()) return;
        
        if (syncPosition)
        {
            playerTransform.position = playerPositionTransform.position;
        }
        
        if (syncRotation)
        {
            playerTransform.rotation = playerPositionTransform.rotation;
        }
        
        Debug.Log("강제 동기화 완료");
    }

    // 동기화 활성/비활성 토글
    public void SetSyncEnabled(bool enabled)
    {
        this.enabled = enabled;
        Debug.Log($"PlayerPositionSync {(enabled ? "활성화" : "비활성화")}");
    }

    // 디버그용 GUI (Scene 뷰에서 확인)
    private void OnDrawGizmos()
    {
        if (!CanSync()) return;
        
        // Player 위치 (파란색)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerTransform.position, 1f);
        
        // PlayerPosition 위치 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPositionTransform.position, 1.2f);
        
        // 연결선 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerTransform.position, playerPositionTransform.position);
    }
}