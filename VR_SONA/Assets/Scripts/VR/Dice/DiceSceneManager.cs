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

    // 상태 관리 변수들
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

    // 외부 콜백 시스템
    private System.Action<int> onDiceResultCallback;
    private System.Action onDiceSceneCompleteCallback;

    // DiceManager 호출 및 콜백 설정 
    public void SetCallbacks(System.Action<int> resultCallback, System.Action completeCallback)
    {
        onDiceResultCallback = resultCallback;
        onDiceSceneCompleteCallback = completeCallback;
        
        if (showDebugLogs)
        {
            Debug.Log("DiceSceneManager 콜백 설정 완료");
        }
    }
    
    public void InitializeScene(PlayerManager player)
    {
        playerManager = player;
        AlignSceneToPlayer();

        if (showDebugLogs)
        {
            Debug.Log("DiceScene 초기화 완료");
        }
    }
    
    public void AlignSceneToPlayer()
    {
        // XR Origin 찾기
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin == null || planeBottomTransform == null || rootGroupToMove == null)
        {
            return;
        }

        Vector3 playerFeet = xrOrigin.transform.position;
        Vector3 planeBottomPos = planeBottomTransform.position;
        Vector3 offset = playerFeet - planeBottomPos;

        Rigidbody[] rigidbodies = rootGroupToMove.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
            rb.isKinematic = true;

        rootGroupToMove.position += offset;
        StartCoroutine(ReenableRigidbodies(rigidbodies));
    }

    private IEnumerator ReenableRigidbodies(Rigidbody[] rigidbodies)
    {
        yield return null;
        foreach (var rb in rigidbodies)
            rb.isKinematic = false;
    }

    void Start()
    {
        InitializeDiceScene();
    }

    // DiceScene 초기 설정 
    private void InitializeDiceScene()
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

        if (resultUI != null)
        {
            resultUI.ShowCustomMessage("주사위를 굴려주세요");
        }

        if (showDebugLogs)
        {
            Debug.Log(" DiceScene 컴포넌트 초기화 완료");
        }
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
            HideResultUI();
            ResetResultState();
        }

        // 주사위가 충분히 멈췄는지 확인
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
                HideResultUI();
            }
            stoppedTimer = 0f;
        }
    }

    private void HideResultUI()
    {
        if (resultUI?.resultPanel != null)
        {
            resultUI.resultPanel.SetActive(false);
        }
    }

    private void ResetResultState()
    {
        isResultDisplayed = false;
        resultShown = false;
        stoppedTimer = 0f;
        isProcessingResult = false;
    }

    private void ShowDiceResult()
    {
        if (diceDetector == null || resultUI == null) return;

        int result = diceDetector.GetVisibleNumber();
        resultUI.ShowResult(result);

        resultShown = true;
        isRolling = false;
        isResultDisplayed = true;

        if (showDebugLogs)
        {
            Debug.Log($"주사위 결과 감지: {result}");
        }

        // 결과 처리 시작
        OnDiceResultDetected(result);
    }

    public void OnDiceResultDetected(int result)
    {
        if (isProcessingResult)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("이미 결과 처리 중입니다.");
            }
            return;
        }

        isProcessingResult = true;
        StartCoroutine(HandleDiceResultFlow(result));
    }

    private IEnumerator HandleDiceResultFlow(int result)
    {
        if (showDebugLogs)
        {
            Debug.Log($"게임 플로우 시작 - 주사위 결과: {result}");
        }

        if (resultUI != null)
        {
            resultUI.ShowResult(result, null);
            float totalUITime = resultUI.fadeInDuration + 0.5f;
            yield return new WaitForSeconds(totalUITime + uiDisplayDelay);
        }

        if (onDiceResultCallback != null)
        {
            onDiceResultCallback.Invoke(result);
            
            if (showDebugLogs)
            {
                Debug.Log($"결과 콜백 호출됨: {result}");
            }
        }

        if (playerManager != null)
        {
            if (showDebugLogs)
            {
                Debug.Log("플레이어 이동 시작");
            }
            
            playerManager.MovePlayer(result);

            // 이동 완료까지 대기
            if (playerManager.IsMoving())
            {
                yield return new WaitUntil(() => !playerManager.IsMoving());
                
                if (showDebugLogs)
                {
                    Debug.Log("플레이어 이동 완료");
                }
            }
        }
        else
        {
            Debug.LogError("PlayerManager가 설정되지 않았습니다!");
        }

        if (onDiceSceneCompleteCallback != null)
        {
            onDiceSceneCompleteCallback.Invoke();
            
            if (showDebugLogs)
            {
                Debug.Log("씬 완료 콜백 호출됨");
            }
        }

        isProcessingResult = false;
        
        if (showDebugLogs)
        {
            Debug.Log("게임 플로우 완료");
        }
    }

    public void ResetDice()
    {
        if (diceRigidbody == null) return;

        diceRigidbody.velocity = Vector3.zero;
        diceRigidbody.angularVelocity = Vector3.zero;
        diceRigidbody.transform.position = diceInitialPosition;
        diceRigidbody.transform.rotation = diceInitialRotation;

        ResetAllStates();
        HideResultUI();

        if (showDebugLogs)
        {
            Debug.Log("주사위 리셋 완료");
        }
    }

    private void ResetAllStates()
    {
        isRolling = false;
        resultShown = false;
        stoppedTimer = 0f;
        isResultDisplayed = false;
        isProcessingResult = false;
        isDetectionActivated = false;
    }

    public void ActivateDiceDetection()
    {
        if (showDebugLogs)
            Debug.Log("주사위 감지 활성화 - 사용자가 주사위를 잡음");
        
        isDetectionActivated = true;
    }

    // 상태 확인 메소드들 
    public bool IsProcessingResult() => isProcessingResult;
    public bool IsRolling() => isRolling;
    public bool IsResultShown() => resultShown;

    public void ForceStopResultProcessing()
    {
        StopAllCoroutines();
        isProcessingResult = false;
        HideResultUI();
        
        if (showDebugLogs)
        {
            Debug.Log("결과 처리 강제 중단됨");
        }
    }


    public void SetUIDisplayDelay(float delay) => uiDisplayDelay = Mathf.Max(0f, delay);
    public void SetMoveCompleteDelay(float delay) => moveCompleteDelay = Mathf.Max(0f, delay);

    public void OnBackButtonPressed()
    {
        if (showDebugLogs)
        {
            Debug.Log("뒤로가기 버튼 눌림 - 씬 종료 요청");
        }

        // 진행 중인 작업이 있으면 중단
        if (isProcessingResult)
        {
            ForceStopResultProcessing();
        }

        // 씬 완료 콜백 호출
        if (onDiceSceneCompleteCallback != null)
        {
            onDiceSceneCompleteCallback.Invoke();
        }
    }
}