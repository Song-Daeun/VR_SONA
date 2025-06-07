// PlayerManager.cs
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
        StartCoroutine(MoveToPosition(targetPosition));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;
        isMoving = true;

        while (elapsed < moveDuration)
        {
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = targetPosition;
        if (cc != null) cc.enabled = true;

        isMoving = false;

        ShowMissionMessage();
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
        if (missionPanel == null)
            return;

        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        Vector3 position = cameraTransform.position + forward * messageDistance;
        position.y = cameraTransform.position.y - 0.2f;

        missionPanel.transform.position = position;
        missionPanel.SetActive(true);

        DiceManager.Instance?.SetDiceButtonVisible(false);
    }
} 
