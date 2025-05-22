using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("이동 설정")]
    public Transform playerTransform;
    public List<Transform> tileList; // StartTile → Tile1~8 등록 (순서대로!)
    public float moveDuration = 0.5f;

    private int currentTileIndex = 0; // 처음은 StartTile

    public void MovePlayer(int steps)
    {
        int targetIndex = currentTileIndex + steps;

        if (targetIndex >= tileList.Count)
        {
            Debug.LogWarning("보드 끝을 넘어갔습니다. 최대 타일까지만 이동합니다.");
            targetIndex = tileList.Count - 1;
        }

        StartCoroutine(MoveToTile(tileList[targetIndex]));
        currentTileIndex = targetIndex;
    }

    private IEnumerator MoveToTile(Transform targetTile)
    {
        Vector3 start = playerTransform.position;
        Vector3 end = targetTile.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            playerTransform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = end;
        Debug.Log($"플레이어가 타일 {targetTile.name}으로 이동했습니다.");
    }
}
