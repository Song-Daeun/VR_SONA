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