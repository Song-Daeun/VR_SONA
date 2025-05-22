using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform;
    public List<Transform> tileList;
    public float moveDuration = 0.5f;
    
    [Header("Landing Settings")]
    public float heightOffset = 15.0f; // 타일 위에서 플레이어가 떠있을 높이
    public LayerMask groundLayerMask = -1; // 바닥으로 인식할 레이어
    public float raycastDistance = 10.0f; // 바닥 감지를 위한 레이캐스트 거리

    private int currentTileIndex = 0;

    public void MovePlayer(int diceResult)
    {
        int targetIndex = diceResult - 1;
        
        // 간단하고 직접적인 접근법
        Vector3 targetPosition = tileList[targetIndex].position + Vector3.up * heightOffset;
        
        StartCoroutine(MoveToPosition(targetPosition));
        currentTileIndex = targetIndex;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / moveDuration);
            playerTransform.position = currentPosition;
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = targetPosition;
        Debug.Log($"플레이어 이동 완료: {targetPosition}");
    }
    // public void MovePlayer(int diceResult)
    // {
    //     int targetIndex = diceResult - 1;
        
    //     // if (targetIndex < 0 || targetIndex >= tileList.Count)
    //     // {
    //     //     Debug.LogWarning($"주사위 결과 {diceResult}에 해당하는 타일이 없습니다. 유효한 범위: 1~{tileList.Count}");
    //     //     return;
    //     // }
        
    //     // Debug.Log($"주사위 결과: {diceResult} → 목적지: {tileList[targetIndex].name}");
        
    //     StartCoroutine(MoveToTileSafely(tileList[targetIndex]));
        
    //     // 현재 위치 업데이트
    //     currentTileIndex = targetIndex;
    // }

    // private IEnumerator MoveToTileSafely(Transform targetTile)
    // {
    //     Vector3 startPosition = playerTransform.position;
        
    //     // 핵심: 안전한 목적지 위치를 계산합니다
    //     Vector3 safeTargetPosition = CalculateSafeLandingPosition(targetTile);
        
    //     // Debug.Log($"이동 시작: {startPosition} → {safeTargetPosition}");
    //     // Debug.Log($"타일 원본 위치: {targetTile.position}, 안전한 위치: {safeTargetPosition}");
        
    //     float elapsed = 0f;

    //     while (elapsed < moveDuration)
    //     {
    //         // 시작점에서 안전한 목적지까지 부드럽게 이동
    //         Vector3 currentPosition = Vector3.Lerp(startPosition, safeTargetPosition, elapsed / moveDuration);
    //         playerTransform.position = currentPosition;
            
    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     // 최종 위치를 정확히 설정
    //     playerTransform.position = safeTargetPosition;
        
    //     // Debug.Log($"이동 완료. 최종 위치: {playerTransform.position}");
    //     // Debug.Log($"플레이어가 타일 {targetTile.name}으로 안전하게 이동했습니다.");
    // }

    private Vector3 CalculateSafeLandingPosition(Transform targetTile)
    {
        // 1단계: 타일의 Collider 정보 수집 및 분석
        // 이 과정에서 Unity의 컴포넌트 시스템에 대해 배울 수 있습니다
        Collider tileCollider = targetTile.GetComponent<Collider>();
        
        if (tileCollider == null)
        {
            Debug.LogError($"타일 {targetTile.name}에 Collider가 없습니다!");
            // 백업 계산: Transform.position에서 위쪽으로 오프셋 추가
            return targetTile.position + Vector3.up * heightOffset;
        }
        
        // 2단계: Bounds를 통한 3차원 공간 분석
        // Bounds는 오브젝트가 차지하는 실제 공간의 크기와 위치를 알려줍니다
        Bounds tileBounds = tileCollider.bounds;
        
        // 디버깅을 위한 상세한 정보 출력
        // Debug.Log($"=== 타일 {targetTile.name} 공간 분석 ===");
        // Debug.Log($"Transform.position (피벗): {targetTile.position}");
        // Debug.Log($"Bounds.center (실제 중심): {tileBounds.center}");
        // Debug.Log($"Bounds.min (최하단 모서리): {tileBounds.min}");
        // Debug.Log($"Bounds.max (최상단 모서리): {tileBounds.max}");
        // Debug.Log($"Bounds.size (전체 크기): {tileBounds.size}");
        
        // 3단계: 안전한 착지점 계산
        // tileBounds.max.y는 타일의 가장 높은 지점을 의미합니다
        // 여기에 heightOffset을 더해서 플레이어가 타일 "위에" 서도록 합니다
        Vector3 safePosition = new Vector3(
            tileBounds.center.x,  // X축: 타일의 중앙
            tileBounds.max.y + heightOffset,  // Y축: 타일 최상단 + 안전 거리
            tileBounds.center.z   // Z축: 타일의 중앙
        );
        
        // 4단계: 계산 결과 검증 및 로깅
        float heightDifference = safePosition.y - targetTile.position.y;
        Debug.Log($"계산된 안전 위치: {safePosition}");
        Debug.Log($"Transform.position과의 높이 차이: {heightDifference:F2}m");
        
        // 5단계: 합리성 검사
        // 만약 계산된 위치가 너무 이상하다면 경고를 출력합니다
        // if (heightDifference > 10f || heightDifference < -10f)
        // {
        //     Debug.LogWarning($"타일 {targetTile.name}의 계산된 높이 차이가 비정상적입니다: {heightDifference:F2}m");
        //     Debug.LogWarning("타일의 피벗 포인트나 모델 스케일을 확인해보세요.");
        // }
        
        return safePosition;
    }

    // Scene 뷰에서 디버깅을 위한 시각적 도구
    private void OnDrawGizmos()
    {
        if (tileList == null || tileList.Count == 0) return;
        
        // 모든 타일의 원본 위치와 계산된 안전 위치를 시각화
        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i] != null)
            {
                Transform tile = tileList[i];
                Vector3 originalPos = tile.position;
                Vector3 safePos = CalculateSafeLandingPosition(tile);
                
                // 원본 위치는 빨간색으로 표시
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(originalPos, Vector3.one * 0.1f);
                
                // 안전한 위치는 초록색으로 표시
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(safePos, 0.2f);
                
                // 두 위치를 연결하는 선 그리기
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(originalPos, safePos);
                
                // 레이캐스트 경로 시각화
                Vector3 rayStart = originalPos + Vector3.up * (raycastDistance * 0.5f);
                Vector3 rayEnd = originalPos - Vector3.up * (raycastDistance * 0.5f);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(rayStart, rayEnd);
            }
        }
        
        // 현재 플레이어 위치 표시
        if (playerTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerTransform.position, 0.3f);
        }
    }
}