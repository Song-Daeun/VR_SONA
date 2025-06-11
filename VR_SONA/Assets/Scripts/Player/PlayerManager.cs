// PlayerManager.cs - 수정된 버전
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform;
    public List<Transform> tileList;
    public Transform startTile; // Start 타일 추가
    public float moveDuration = 0.5f;
    public float teleportDuration = 0.1f; // 텔레포트용 빠른 이동

    [Header("Landing Settings")]
    public float heightOffset = 13.0f;
    public LayerMask groundLayerMask = -1;
    public float raycastDistance = 10.0f;

    // [Header("Mission UI")]
    // [SerializeField] private GameObject missionPanel;
    // [SerializeField] private float messageDistance = 2.0f;
    // [SerializeField] private float messageHeight = 1.5f;

    private bool isMoving = false;
    private int currentTileIndex = 0;
    
    // 현재 이동 중인 주사위 결과를 저장 (GameManager에게 전달하기 위함)
    private int currentDiceResult = -1;

    public bool IsMoving()
    {
        return isMoving;
    }

    // 기존 일반 이동 (주사위용) - 수정됨
    public void MovePlayer(int diceResult)
    {
        Debug.Log($"=== PlayerManager.MovePlayer 호출됨 ===");
        Debug.Log($"주사위 결과: {diceResult}");
        Debug.Log($"현재 isMoving: {isMoving}");
        
        if (isMoving)
        {
            Debug.Log("이미 이동 중이므로 요청 무시");
            return;
        }

        if (tileList == null || tileList.Count == 0)
        {
            Debug.LogError("tileList가 null이거나 비어있음");
            return;
        }

        int targetIndex = diceResult - 1;
        Debug.Log($"계산된 targetIndex: {targetIndex}");
        
        if (targetIndex < 0 || targetIndex >= tileList.Count)
        {
            Debug.LogError($"targetIndex가 범위를 벗어남: {targetIndex}");
            return;
        }

        Transform targetTile = tileList[targetIndex];
        if (targetTile == null)
        {
            Debug.LogError($"targetTile이 null임 (인덱스: {targetIndex})");
            return;
        }

        // 현재 주사위 결과 저장 (이동 완료 후 GameManager에게 전달하기 위함)
        currentDiceResult = diceResult;

        Debug.Log($"목표 타일: {targetTile.name}");
        Vector3 targetPosition = CalculateSafeLandingPosition(targetTile);
        Debug.Log($"목표 위치: {targetPosition}");
        
        // 일반 이동에서는 미션 메시지를 PlayerManager에서 표시하지 않음
        // GameManager가 이동 완료 신호를 받은 후 직접 처리함
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false, true));
        Debug.Log("MoveToPosition 코루틴 시작됨 (GameManager 알림 포함)");
    }

    // 텔레포트 (즉시 이동) - 수정됨
    public void TeleportToTile(int tileIndex)
    {
        Debug.Log($"=== PlayerManager.TeleportToTile 호출됨 ===");
        Debug.Log($"타일 인덱스: {tileIndex}");
        
        if (isMoving)
        {
            Debug.Log("이미 이동 중이므로 텔레포트 요청 무시");
            return;
        }

        if (tileList == null || tileList.Count == 0)
        {
            Debug.LogError("tileList가 null이거나 비어있음");
            return;
        }

        if (tileIndex < 0 || tileIndex >= tileList.Count)
        {
            Debug.LogError($"tileIndex가 범위를 벗어남: {tileIndex}");
            return;
        }

        Transform targetTile = tileList[tileIndex];
        if (targetTile == null)
        {
            Debug.LogError($"targetTile이 null임 (인덱스: {tileIndex})");
            return;
        }

        // 텔레포트의 경우 주사위 결과는 타일 인덱스 + 1로 설정
        currentDiceResult = tileIndex + 1;

        Vector3 targetPosition = CalculateSafeLandingPosition(targetTile);
        Debug.Log($"텔레포트 목표 위치: {targetPosition}");
        
        StartCoroutine(MoveToPosition(targetPosition, teleportDuration, false, true));
        Debug.Log("텔레포트 코루틴 시작됨 (GameManager 알림 포함)");
    }

    // Start 타일로 이동 - 수정됨
    public void MoveToStart()
    {
        Debug.Log("=== PlayerManager.MoveToStart 호출됨 ===");
        
        if (isMoving)
        {
            Debug.Log("이미 이동 중이므로 Start 이동 요청 무시");
            return;
        }

        if (startTile == null)
        {
            Debug.LogError("Start 타일이 설정되지 않았습니다!");
            return;
        }

        // Start 타일로 이동할 때는 주사위 결과를 -1로 설정 (Start 타일 의미)
        currentDiceResult = -1;

        Vector3 targetPosition = CalculateSafeLandingPosition(startTile);
        Debug.Log($"Start 타일 목표 위치: {targetPosition}");
        
        // Start 타일로 이동할 때는 GameManager에게 알리지 않음 (특별한 경우)
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false, false));
        Debug.Log("Start 타일 이동 코루틴 시작됨 (GameManager 알림 없음)");
    }

    // 수정된 이동 코루틴 - 핵심 변경사항
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration = -1, bool showMission = false, bool notifyGameManager = false)
    {
        Debug.Log($"=== MoveToPosition 코루틴 시작 ===");
        Debug.Log($"목표 위치: {targetPosition}");
        Debug.Log($"이동 시간: {duration}");
        Debug.Log($"미션 메시지 표시: {showMission}");
        Debug.Log($"GameManager 알림: {notifyGameManager}");
        
        if (duration < 0) duration = moveDuration;

        // CharacterController 비활성화 (물리 충돌 방지)
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) 
        {
            cc.enabled = false;
            Debug.Log("CharacterController 비활성화됨");
        }

        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;
        isMoving = true;
        
        Debug.Log($"이동 시작: {startPosition} → {targetPosition}");

        // 실제 이동 애니메이션
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 정확히 설정
        playerTransform.position = targetPosition;
        Debug.Log($"이동 완료: 최종 위치 = {playerTransform.position}");

        // CharacterController 재활성화
        if (cc != null) 
        {
            cc.enabled = true;
            Debug.Log("CharacterController 재활성화됨");
        }

        // 이동 상태 플래그 해제
        isMoving = false;
        Debug.Log("isMoving = false로 설정됨");

        // 이동 완료 후 후속 처리
        // if (showMission)
        // {
        //     Debug.Log("미션 메시지 표시 요청됨");
        //     ShowMissionMessage();
        // }

        // 🔥 핵심 개선사항: GameManager에게 이동 완료 직접 알림
        if (notifyGameManager && GameManager.Instance != null)
        {
            Debug.Log($"🚀 GameManager에게 이동 완료 알림 - 주사위 결과: {currentDiceResult}");
            
            // 한 프레임 대기 후 알림 (안전성을 위해)
            yield return null;
            
            // GameManager의 이동 완료 처리 메서드 호출
            GameManager.Instance.OnPlayerMovementCompleted(currentDiceResult);
            
            Debug.Log("✅ GameManager 알림 완료");
        }
        else if (notifyGameManager && GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance가 null입니다! 이동 완료 알림 실패");
        }

        Debug.Log("=== MoveToPosition 코루틴 완료 ===");
    }

    private Vector3 CalculateSafeLandingPosition(Transform tile)
    {
        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider == null)
        {
            Debug.Log($"타일 {tile.name}에 Collider가 없음. 기본 높이 오프셋 사용");
            return tile.position + Vector3.up * heightOffset;
        }

        Bounds bounds = tileCollider.bounds;
        Vector3 safePosition = new Vector3(bounds.center.x, bounds.max.y + heightOffset, bounds.center.z);
        
        Debug.Log($"타일 {tile.name}의 안전한 착지 위치 계산: {safePosition}");
        return safePosition;
    }

    // public void ShowMissionMessage()
    // {
    //     Debug.Log("=== PlayerManager.ShowMissionMessage 호출됨 ===");
        
    //     // GameManager에서 현재 위치 확인 (안전성 체크)
    //     if (GameManager.Instance != null)
    //     {
    //         int currentIndex = GameManager.Instance.GetCurrentTileIndex();
    //         string currentTile = GameManager.Instance.GetCurrentTileName();
    //         Debug.Log($"현재 위치: {currentTile} (인덱스: {currentIndex})");
            
    //         // Start 타일이면 미션 메시지를 표시하지 않음
    //         if (currentIndex == -1)
    //         {
    //             Debug.LogWarning("🚨 Start 타일에서 미션 메시지 표시 요청이 들어왔습니다!");
    //             Debug.LogWarning("이는 비정상적인 동작입니다. 미션 메시지를 표시하지 않습니다.");
    //             return;
    //         }
    //     }
        
    //     // DiceManager 버튼 숨기기
    //     if (DiceManager.Instance != null)
    //     {
    //         DiceManager.Instance.SetDiceButtonVisible(false);
    //         Debug.Log("DiceManager 버튼 숨김 처리됨");
    //     }
        
    //     if (missionPanel == null)
    //     {
    //         Debug.LogError("missionPanel이 설정되지 않았습니다!");
    //         return;
    //     }

    //     // VR 환경을 고려한 미션 패널 위치 설정
    //     Transform cameraTransform = Camera.main.transform;
    //     if (cameraTransform == null)
    //     {
    //         Debug.LogError("Main Camera를 찾을 수 없습니다!");
    //         return;
    //     }
        
    //     Vector3 forward = cameraTransform.forward;
    //     Vector3 position = cameraTransform.position + forward * messageDistance;
    //     position.y = cameraTransform.position.y - 0.2f;

    //     missionPanel.transform.position = position;
    //     missionPanel.SetActive(true);
        
    //     Debug.Log($"미션 패널 활성화됨. 위치: {position}");
    // }

    // 현재 타일 인덱스 설정 (GameManager에서 호출용)
    public void SetCurrentTileIndex(int index)
    {
        currentTileIndex = index;
        Debug.Log($"PlayerManager 타일 인덱스 설정: {index}");
    }

    public int GetCurrentTileIndex()
    {
        return currentTileIndex;
    }

    // 디버그용 현재 상태 출력
//     public void DebugCurrentState()
//     {
//         Debug.Log($"=== PlayerManager 현재 상태 ===");
//         Debug.Log($"isMoving: {isMoving}");
//         Debug.Log($"currentTileIndex: {currentTileIndex}");
//         Debug.Log($"currentDiceResult: {currentDiceResult}");
//         Debug.Log($"플레이어 위치: {(playerTransform != null ? playerTransform.position : Vector3.zero)}");
//     }
}