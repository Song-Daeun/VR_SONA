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

    private bool isMoving = false;
    private int currentTileIndex = 0;
    
    public bool IsMoving()
    {
        // 현재 이동 상태를 반환합니다
        // 이 메서드는 외부 클래스(DiceSceneManager)에서 
        // 플레이어가 현재 이동 중인지 확인할 때 사용됩니다
        return isMoving;
    }

    public void MovePlayer(int diceResult)
    {
        Debug.Log($"[🎯] MovePlayer 호출됨 - 주사위 결과: {diceResult}");
        Debug.Log($"[📊] 현재 상태 - isMoving: {isMoving}, tileList 개수: {(tileList?.Count ?? 0)}");

        if (isMoving)
        {
            Debug.LogWarning("⚠️ 이미 이동 중입니다. 이동 요청을 무시합니다.");
            return;
        }

        int targetIndex = diceResult - 1;

        if (tileList == null || tileList.Count == 0)
        {
            Debug.LogError("❌ tileList가 비어있습니다!");
            return;
        }

        if (targetIndex < 0 || targetIndex >= tileList.Count)
        {
            Debug.LogWarning($"❌ 주사위 결과 {diceResult}에 해당하는 타일이 없습니다! (인덱스: {targetIndex}, 최대 인덱스: {tileList.Count - 1})");
            return;
        }

        Transform targetTile = tileList[targetIndex];
        if (targetTile == null)
        {
            Debug.LogError($"❌ 타일 {targetIndex}가 null입니다!");
            return;
        }

        Vector3 targetPosition = CalculateSafeLandingPosition(targetTile);
        Debug.Log($"[🏃] 이동 시작 - 목표 위치: {targetPosition} (타일: {diceResult})");

        StartCoroutine(MoveToPosition(targetPosition));
    }

    // private IEnumerator MoveToPosition(Vector3 targetPosition)
    // {
    //     Vector3 startPosition = playerTransform.position;
    //     float elapsed = 0f;

    //     while (elapsed < moveDuration)
    //     {
    //         Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / moveDuration);
    //         playerTransform.position = currentPosition;

    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     playerTransform.position = targetPosition;
    // }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        CharacterController cc = playerTransform.GetComponent<CharacterController>();

        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;

        if (cc != null) cc.enabled = false;

        while (elapsed < moveDuration)
        {
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / moveDuration);
            playerTransform.position = currentPosition;

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = targetPosition;

        if (cc != null) cc.enabled = true;
    }

    private Vector3 CalculateSafeLandingPosition(Transform targetTile)
    {
        Collider tileCollider = targetTile.GetComponent<Collider>();

        if (tileCollider == null)
        {
            return targetTile.position + Vector3.up * heightOffset;
        }

        Bounds tileBounds = tileCollider.bounds;

        Vector3 safePosition = new Vector3(
            tileBounds.center.x,  // 타일의 중앙
            tileBounds.max.y + heightOffset,
            tileBounds.center.z
        );

        return safePosition;
    }

    // Scene 뷰에서 디버깅을 위한 시각적 도구
    private void OnDrawGizmos()
    {
        if (tileList == null || tileList.Count == 0) return;
        
        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i] != null)
            {
                Transform tile = tileList[i];
                Vector3 originalPos = tile.position;
                Vector3 safePos = CalculateSafeLandingPosition(tile);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(originalPos, Vector3.one * 0.1f);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(safePos, 0.2f);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(originalPos, safePos);
                
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