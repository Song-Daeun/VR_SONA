// PlayerManager.cs - 확장 버전
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
    public float heightOffset = 15.0f;
    public LayerMask groundLayerMask = -1;
    public float raycastDistance = 10.0f;

    [Header("Mission UI")]
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private float messageDistance = 2.0f;
    [SerializeField] private float messageHeight = 1.5f;

    private bool isMoving = false;
    private int currentTileIndex = 0;

    public bool IsMoving()
    {
        return isMoving;
    }

    // 기존 일반 이동 (주사위용)
    public void MovePlayer(int diceResult)
    {
        if (isMoving)
            return;

        if (tileList == null || tileList.Count == 0)
            return;

        int targetIndex = diceResult - 1;
        if (targetIndex < 0 || targetIndex >= tileList.Count)
            return;

        Transform targetTile = tileList[targetIndex];
        if (targetTile == null)
            return;

        Vector3 targetPosition = CalculateSafeLandingPosition(targetTile);
        StartCoroutine(MoveToPosition(targetPosition, moveDuration));
    }

    // 텔레포트 (즉시 이동)
    public void TeleportToTile(int tileIndex)
    {
        if (isMoving)
            return;

        if (tileList == null || tileList.Count == 0)
            return;

        if (tileIndex < 0 || tileIndex >= tileList.Count)
            return;

        Transform targetTile = tileList[tileIndex];
        if (targetTile == null)
            return;

        Vector3 targetPosition = CalculateSafeLandingPosition(targetTile);
        StartCoroutine(MoveToPosition(targetPosition, teleportDuration));
    }

    // Start 타일로 이동
    public void MoveToStart()
    {
        if (isMoving)
            return;

        if (startTile == null)
        {
            Debug.LogError("Start 타일이 설정되지 않았습니다!");
            return;
        }

        Vector3 targetPosition = CalculateSafeLandingPosition(startTile);
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false)); // 미션 메시지 표시 안함
    }

    // 수정된 이동 코루틴 (duration 매개변수 추가)
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration = -1, bool showMission = true)
    {
        if (duration < 0) duration = moveDuration;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;
        isMoving = true;

        while (elapsed < duration)
        {
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = targetPosition;
        if (cc != null) cc.enabled = true;

        isMoving = false;

        // Start 타일이 아닐 때만 미션 메시지 표시
        if (showMission)
        {
            ShowMissionMessage();
        }
    }

    private Vector3 CalculateSafeLandingPosition(Transform tile)
    {
        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider == null)
            return tile.position + Vector3.up * heightOffset;

        Bounds bounds = tileCollider.bounds;
        return new Vector3(bounds.center.x, bounds.max.y + heightOffset, bounds.center.z);
    }

    public void ShowMissionMessage()
    {
        // DiceManager 버튼 숨기기
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.SetDiceButtonVisible(false);
        }
        
        if (missionPanel == null)
            return;

        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        Vector3 position = cameraTransform.position + forward * messageDistance;
        position.y = cameraTransform.position.y - 0.2f;

        missionPanel.transform.position = position;
        missionPanel.SetActive(true);
    }

    // 현재 타일 인덱스 설정 (GameManager에서 호출용)
    public void SetCurrentTileIndex(int index)
    {
        currentTileIndex = index;
    }

    public int GetCurrentTileIndex()
    {
        return currentTileIndex;
    }
}