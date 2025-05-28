using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceSceneManager : MonoBehaviour
{
    [Header("Reference")]
    public Rigidbody diceRigidbody;
    public DiceResultDetector diceDetector;
    public DiceResultUI resultUI;

    [Header("Result Detection Settings")]
    public float stoppedVelocityThreshold = 0.1f;
    public float stoppedAngularThreshold = 0.1f;
    public float settleTime = 1.0f;

    [Header("Game Flow Settings")]
    public float uiDisplayDelay = 1.0f;
    public float moveCompleteDelay = 2.0f;

    [Header("Debugging")]
    public bool showDebugLogs = false;
    public bool drawDebugVisuals = false;

    private bool isRolling = false;
    private bool resultShown = false;
    private float stoppedTimer = 0f;
    private Vector3 diceInitialPosition;
    private Quaternion diceInitialRotation;

    public XRGrabInteractable grabInteractable;
    private bool isResultDisplayed = false;
    private float minVelocityThreshold = 0.1f;
    private bool isProcessingResult = false;

    // 플레이어 연결
    public PlayerManager playerManager;

    private void Start()
    {
        if (diceRigidbody != null)
        {
            diceInitialPosition = diceRigidbody.transform.position;
            diceInitialRotation = diceRigidbody.transform.rotation;
        }

        if (diceDetector != null && diceDetector.playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                diceDetector.playerCamera = mainCamera;
            }
        }

        resultUI?.ShowCustomMessage("🎲 주사위를 굴려주세요!");
    }

    private void Update()
    {
        // CheckDiceState();
        if (!isDetectionActivated && grabInteractable != null && grabInteractable.isSelected)
        {
            ActivateDiceDetection();
        }

        if (!isDetectionActivated) return;

    CheckDiceState();
    }

    private void CheckDiceState()
    {
        float velocity = diceRigidbody.velocity.magnitude;
        float angularVelocity = diceRigidbody.angularVelocity.magnitude;

        if (velocity > minVelocityThreshold && isResultDisplayed)
        {
            if (resultUI != null && resultUI.resultPanel != null)
            {
                resultUI.resultPanel.SetActive(false);
            }

            isResultDisplayed = false;
            resultShown = false;
            stoppedTimer = 0f;
            isProcessingResult = false;
        }

        bool isStill = velocity < stoppedVelocityThreshold && angularVelocity < stoppedAngularThreshold;

        if (isStill && isRolling)
        {
            stoppedTimer += Time.deltaTime;
            if (stoppedTimer >= settleTime && !resultShown)
            {
                ShowDiceResult();
            }
        }
        else if (!isStill)
        {
            if (!isRolling)
            {
                isRolling = true;

                if (resultUI != null && resultUI.resultPanel != null && resultUI.resultPanel.activeSelf)
                {
                    resultUI.resultPanel.SetActive(false);
                }
            }

            stoppedTimer = 0f;
        }
    }

    private void ShowDiceResult()
    {
        if (diceDetector == null || resultUI == null) return;

        int result = diceDetector.GetVisibleNumber();
        resultUI.ShowResult(result);

        resultShown = true;
        isRolling = false;
        isResultDisplayed = true;

        // OnDiceResultConfirmed(result);
    }

    public void OnDiceResultDetected(int result)
    {
        isProcessingResult = true;
        StartCoroutine(HandleDiceResultFlow(result));
    }

    private IEnumerator HandleDiceResultFlow(int result)
    {
        if (resultUI != null)
        {
            resultUI.ShowResult(result, null);
        }
        else
        {
            yield break;
        }

        float totalUITime = resultUI.fadeInDuration + 0.5f;
        yield return new WaitForSeconds(totalUITime + uiDisplayDelay);

        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            Debug.Log($"PlayerManager.MovePlayer({result}) 호출됨");
            playerManager.MovePlayer(result);
        }
        else
        {
            Debug.LogWarning("[❌] playerManager가 null이어서 이동 실패");
        }

        float estimatedMoveTime = playerManager != null ? playerManager.moveDuration : 0.5f;
        yield return new WaitForSeconds(estimatedMoveTime + moveCompleteDelay);

        DiceManager diceManager = FindObjectOfType<DiceManager>();
        if (diceManager != null)
        {
            diceManager.OnBackButtonClicked();
        }

        isProcessingResult = false;
    }

    // private void OnDiceResultConfirmed(int result)
    // {
    //     // 결과 확정 이후 로직 훅
    // }

    public void ResetDice()
    {
        if (diceRigidbody == null) return;

        diceRigidbody.velocity = Vector3.zero;
        diceRigidbody.angularVelocity = Vector3.zero;

        diceRigidbody.transform.position = diceInitialPosition;
        diceRigidbody.transform.rotation = diceInitialRotation;

        isRolling = false;
        resultShown = false;
        stoppedTimer = 0f;
        isResultDisplayed = false;
        isProcessingResult = false;

        if (resultUI != null && resultUI.resultPanel != null)
        {
            resultUI.resultPanel.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugVisuals || diceRigidbody == null) return;

        Vector3 dicePos = diceRigidbody.transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(dicePos, diceRigidbody.velocity);

        if (isProcessingResult)
            Gizmos.color = Color.magenta;
        else if (isRolling)
            Gizmos.color = Color.yellow;
        else if (resultShown)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.gray;

        Gizmos.DrawWireSphere(dicePos, 0.3f);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(diceInitialPosition, Vector3.one * 0.1f);

#if UNITY_EDITOR
        string statusText = "";
        if (isProcessingResult) statusText = "Processing Result";
        else if (isRolling) statusText = "Rolling";
        else if (resultShown) statusText = "Result Shown";
        else statusText = "Waiting";

        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(dicePos + Vector3.up * 0.5f, $"Dice Status: {statusText}");

        float velocity = diceRigidbody.velocity.magnitude;
        float angularVel = diceRigidbody.angularVelocity.magnitude;
        UnityEditor.Handles.Label(dicePos + Vector3.up * 0.7f, $"V: {velocity:F2} | AV: {angularVel:F2}");
#endif
    }

    public bool IsProcessingResult() => isProcessingResult;
    public bool IsRolling() => isRolling;
    public bool IsResultShown() => resultShown;

    public void ForceStopResultProcessing()
    {
        StopAllCoroutines();
        isProcessingResult = false;

        if (resultUI != null && resultUI.resultPanel != null)
        {
            resultUI.resultPanel.SetActive(false);
        }
    }

    public void SetUIDisplayDelay(float delay)
    {
        uiDisplayDelay = Mathf.Max(0f, delay);
    }

    public void SetMoveCompleteDelay(float delay)
    {
        moveCompleteDelay = Mathf.Max(0f, delay);
    }

    private bool isDetectionActivated = false;

    public void ActivateDiceDetection()
    {
        if (showDebugLogs)
            Debug.Log("🎲 Dice detection activated by user grab!");
        isDetectionActivated = true;
    }

}
