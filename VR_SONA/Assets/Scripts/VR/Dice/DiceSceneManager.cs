using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceSceneManager : MonoBehaviour
{
    [Header("Reference")]
    public Rigidbody diceRigidbody;
    public DiceResultDetector diceDetector;
    public DiceResultUI resultUI;
    public Transform planeBottomTransform;
    public Transform rootGroupToMove;

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

    public PlayerManager playerManager;
    private bool isDetectionActivated = false;

    public void InitializeScene(PlayerManager player)
    {
        playerManager = player;
        // AlignSceneToPlayer();
    }

    // public void AlignSceneToPlayer()
    // {
    //     if (planeBottomTransform == null || rootGroupToMove == null || playerManager == null)
    //     {
    //         Debug.LogWarning("AlignSceneToPlayerFeet(): 필수 참조가 누락됨");
    //         return;
    //     }

    //     Vector3 playerFeet = playerManager.transform.position;
    //     Vector3 planeBottom = planeBottomTransform.position;

    //     Vector3 offset = playerFeet - planeBottom;
    //     rootGroupToMove.position += offset;

    //     Debug.Log($"[📌] Plane을 플레이어 위치에 정렬 완료 (offset: {offset})");
    // }

    public void AlignSceneToPlayer()
    {
        if (planeBottomTransform == null || rootGroupToMove == null || playerManager == null)
        {
            Debug.LogWarning("❌ AlignPlaneToPlayerAndStandOnIt(): 필요한 참조가 없음");
            return;
        }

        // Step 1: Plane을 플레이어 위치로 옮기기
        Vector3 playerFeet = playerManager.transform.position;
        Vector3 planeBottomPos = planeBottomTransform.position;
        Vector3 offset = playerFeet - planeBottomPos;

        Rigidbody[] rigidbodies = rootGroupToMove.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
            rb.isKinematic = true;

        rootGroupToMove.position += offset;

        StartCoroutine(ReenableRigidbodies(rigidbodies));

        // Step 2: 플레이어를 Plane 위로 정확히 올려주기 (Y만 조정)
        Vector3 planeTop = planeBottomTransform.position + Vector3.up * 0.05f;
        Vector3 current = playerManager.transform.position;
        Vector3 adjusted = new Vector3(current.x, planeTop.y, current.z);

        playerManager.transform.position = adjusted;

        Debug.Log($"✅ Plane 정렬 + 플레이어 위치 완료: {adjusted}");
    }

    private IEnumerator ReenableRigidbodies(Rigidbody[] rigidbodies)
    {
        yield return null;
        foreach (var rb in rigidbodies)
            rb.isKinematic = false;
    }

    void Start()
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
                diceDetector.playerCamera = mainCamera;
        }

        resultUI?.ShowCustomMessage("주사위를 굴려주세요");
    }

    void Update()
    {
        if (!isDetectionActivated && grabInteractable != null && grabInteractable.isSelected)
            ActivateDiceDetection();

        if (!isDetectionActivated) return;

        CheckDiceState();
    }

    private void CheckDiceState()
    {
        float velocity = diceRigidbody.velocity.magnitude;
        float angularVelocity = diceRigidbody.angularVelocity.magnitude;

        if (velocity > minVelocityThreshold && isResultDisplayed)
        {
            resultUI?.resultPanel?.SetActive(false);
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
                ShowDiceResult();
        }
        else if (!isStill)
        {
            if (!isRolling)
            {
                isRolling = true;
                resultUI?.resultPanel?.SetActive(false);
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
    }

    public void OnDiceResultDetected(int result)
    {
        isProcessingResult = true;
        StartCoroutine(HandleDiceResultFlow(result));
    }

    private IEnumerator HandleDiceResultFlow(int result)
    {
        resultUI?.ShowResult(result, null);

        float totalUITime = resultUI.fadeInDuration + 0.5f;
        yield return new WaitForSeconds(totalUITime + uiDisplayDelay);

        playerManager?.MovePlayer(result);
        float estimatedMoveTime = playerManager != null ? playerManager.moveDuration : 0.5f;
        yield return new WaitForSeconds(estimatedMoveTime + moveCompleteDelay);

        FindObjectOfType<DiceManager>()?.OnBackButtonClicked();
        isProcessingResult = false;
    }

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

        resultUI?.resultPanel?.SetActive(false);
    }

    public void ActivateDiceDetection()
    {
        if (showDebugLogs)
            Debug.Log("Dice detection activated by user grab!");
        isDetectionActivated = true;
    }

    public bool IsProcessingResult() => isProcessingResult;
    public bool IsRolling() => isRolling;
    public bool IsResultShown() => resultShown;

    public void ForceStopResultProcessing()
    {
        StopAllCoroutines();
        isProcessingResult = false;
        resultUI?.resultPanel?.SetActive(false);
    }

    public void SetUIDisplayDelay(float delay) => uiDisplayDelay = Mathf.Max(0f, delay);
    public void SetMoveCompleteDelay(float delay) => moveCompleteDelay = Mathf.Max(0f, delay);
}