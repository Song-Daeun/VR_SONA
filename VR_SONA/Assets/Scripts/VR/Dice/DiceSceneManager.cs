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

    // ========================================
    // 새로 추가: 외부 콜백 시스템
    // ========================================
    private System.Action<int> onDiceResultCallback;
    private System.Action onDiceSceneCompleteCallback;

    /// <summary>
    /// DiceManager에서 호출하여 콜백을 설정합니다.
    /// 이를 통해 씬 간의 결합도를 낮출 수 있습니다.
    /// </summary>
    public void SetCallbacks(System.Action<int> resultCallback, System.Action completeCallback)
    {
        onDiceResultCallback = resultCallback;
        onDiceSceneCompleteCallback = completeCallback;
        
        if (showDebugLogs)
        {
            Debug.Log("📞 DiceSceneManager 콜백 설정 완료");
        }
    }

    /// <summary>
    /// PlayerManager를 받아서 씬을 초기화합니다.
    /// DiceManager에서 씬 로드 후 이 메소드를 호출해야 합니다.
    /// </summary>
    public void InitializeScene(PlayerManager player)
    {
        playerManager = player;
        AlignSceneToPlayer();
        
        if (showDebugLogs)
        {
            Debug.Log("DiceScene 초기화 완료");
        }
    }

    /// <summary>
    /// 플레이어 위치에 맞춰 주사위 씬을 정렬합니다.
    public void AlignSceneToPlayer()
    {
        if (planeBottomTransform == null || rootGroupToMove == null || playerManager == null)
        {
            Debug.LogWarning("AlignSceneToPlayer(): 필요한 참조가 없음");
            return;
        }
        Vector3 playerFeet = playerManager.transform.position;
        Vector3 planeBottomPos = planeBottomTransform.position;
        Vector3 offset = playerFeet - planeBottomPos;
        Rigidbody[] rigidbodies = rootGroupToMove.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
            rb.isKinematic = true;
        rootGroupToMove.position += offset;
        StartCoroutine(ReenableRigidbodies(rigidbodies));
        Vector3 planeTop = planeBottomTransform.position + Vector3.up * 0.05f;
        Vector3 current = playerManager.transform.position;
        Vector3 adjusted = new Vector3(current.x, planeTop.y, current.z);
        playerManager.transform.position = adjusted;
        Debug.Log($"Plane 정렬 + 플레이어 위치 완료: {adjusted}");
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

    /// <summary>
    /// 주사위 씬의 초기 설정을 수행합니다.
    /// </summary>
    private void InitializeDiceScene()
    {
        // 주사위 초기 위치 저장 (리셋용)
        if (diceRigidbody != null)
        {
            diceInitialPosition = diceRigidbody.transform.position;
            diceInitialRotation = diceRigidbody.transform.rotation;
        }

        // 카메라 자동 연결
        if (diceDetector != null && diceDetector.playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                diceDetector.playerCamera = mainCamera;
        }

        // 초기 UI 메시지 표시
        if (resultUI != null)
        {
            resultUI.ShowCustomMessage("주사위를 굴려주세요");
        }

        if (showDebugLogs)
        {
            Debug.Log("🎲 DiceScene 컴포넌트 초기화 완료");
        }
    }

    void Update()
    {
        // 사용자가 주사위를 잡으면 감지 활성화
        if (!isDetectionActivated && grabInteractable != null && grabInteractable.isSelected)
            ActivateDiceDetection();

        if (!isDetectionActivated) return;

        CheckDiceState();
    }

    /// <summary>
    /// 주사위의 물리 상태를 지속적으로 모니터링합니다.
    /// 주사위가 멈췄는지, 다시 굴러가기 시작했는지 등을 감지합니다.
    /// </summary>
    private void CheckDiceState()
    {
        float velocity = diceRigidbody.velocity.magnitude;
        float angularVelocity = diceRigidbody.angularVelocity.magnitude;

        // 주사위가 다시 움직이기 시작하면 결과 UI 숨기기
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

    /// <summary>
    /// 주사위가 멈췄을 때 결과를 감지하고 표시합니다.
    /// </summary>
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
            Debug.Log($"🎲 주사위 결과 감지: {result}");
        }

        // 결과 처리 시작
        OnDiceResultDetected(result);
    }

    /// <summary>
    /// 주사위 결과가 확정되었을 때 호출됩니다.
    /// 전체 게임 플로우를 관리하는 핵심 메소드입니다.
    /// </summary>
    public void OnDiceResultDetected(int result)
    {
        if (isProcessingResult)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ 이미 결과 처리 중입니다.");
            }
            return;
        }

        isProcessingResult = true;
        StartCoroutine(HandleDiceResultFlow(result));
    }

    /// <summary>
    /// 주사위 결과부터 플레이어 이동, 미션 표시까지의 전체 흐름을 관리합니다.
    /// 이 코루틴이 게임의 턴 진행을 담당하는 핵심 로직입니다.
    /// </summary>
    private IEnumerator HandleDiceResultFlow(int result)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🎮 게임 플로우 시작 - 주사위 결과: {result}");
        }

        // 1단계: 결과 UI 표시
        if (resultUI != null)
        {
            resultUI.ShowResult(result, null);
            float totalUITime = resultUI.fadeInDuration + 0.5f;
            yield return new WaitForSeconds(totalUITime + uiDisplayDelay);
        }

        // 2단계: 외부 콜백 호출 (주로 GameManager에게 결과 전달)
        if (onDiceResultCallback != null)
        {
            onDiceResultCallback.Invoke(result);
            
            if (showDebugLogs)
            {
                Debug.Log($"📞 결과 콜백 호출됨: {result}");
            }
        }

        // 3단계: 플레이어 이동 처리
        if (playerManager != null)
        {
            if (showDebugLogs)
            {
                Debug.Log("🚶 플레이어 이동 시작");
            }
            
            playerManager.MovePlayer(result);

            // 이동 완료까지 대기
            if (playerManager.IsMoving())
            {
                yield return new WaitUntil(() => !playerManager.IsMoving());
                
                if (showDebugLogs)
                {
                    Debug.Log("✅ 플레이어 이동 완료");
                }
            }
        }
        else
        {
            Debug.LogError("❌ PlayerManager가 설정되지 않았습니다!");
        }

        // 4단계: 씬 완료 콜백 호출
        if (onDiceSceneCompleteCallback != null)
        {
            onDiceSceneCompleteCallback.Invoke();
            
            if (showDebugLogs)
            {
                Debug.Log("📞 씬 완료 콜백 호출됨");
            }
        }

        // 5단계: 미션 메시지 표시 (PlayerManager를 통해)
        if (playerManager != null)
        {
            playerManager.ShowMissionMessage();
            
            if (showDebugLogs)
            {
                Debug.Log("📋 미션 메시지 표시됨");
            }
        }

        isProcessingResult = false;
        
        if (showDebugLogs)
        {
            Debug.Log("🎮 게임 플로우 완료");
        }
    }

    /// <summary>
    /// 주사위를 초기 위치로 리셋합니다.
    /// </summary>
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
            Debug.Log("🔄 주사위 리셋 완료");
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

    /// <summary>
    /// 사용자가 주사위를 잡았을 때 감지를 활성화합니다.
    /// </summary>
    public void ActivateDiceDetection()
    {
        if (showDebugLogs)
            Debug.Log("🎯 주사위 감지 활성화 - 사용자가 주사위를 잡음");
        
        isDetectionActivated = true;
    }

    // ========================================
    // 상태 확인 메소드들 (외부에서 상태 조회용)
    // ========================================
    public bool IsProcessingResult() => isProcessingResult;
    public bool IsRolling() => isRolling;
    public bool IsResultShown() => resultShown;

    /// <summary>
    /// 강제로 결과 처리를 중단합니다. (긴급 상황용)
    /// </summary>
    public void ForceStopResultProcessing()
    {
        StopAllCoroutines();
        isProcessingResult = false;
        HideResultUI();
        
        if (showDebugLogs)
        {
            Debug.Log("⛔ 결과 처리 강제 중단됨");
        }
    }

    // ========================================
    // 설정 조정 메소드들
    // ========================================
    public void SetUIDisplayDelay(float delay) => uiDisplayDelay = Mathf.Max(0f, delay);
    public void SetMoveCompleteDelay(float delay) => moveCompleteDelay = Mathf.Max(0f, delay);

    /// <summary>
    /// 뒤로가기 버튼이 눌렸을 때 호출됩니다.
    /// DiceResultUI에서 호출하거나 외부에서 강제 종료할 때 사용합니다.
    /// </summary>
    public void OnBackButtonPressed()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔙 뒤로가기 버튼 눌림 - 씬 종료 요청");
        }

        // 진행 중인 작업이 있으면 중단
        if (isProcessingResult)
        {
            ForceStopResultProcessing();
        }

        // 씬 완료 콜백 호출 (DiceManager가 씬을 언로드하도록)
        if (onDiceSceneCompleteCallback != null)
        {
            onDiceSceneCompleteCallback.Invoke();
        }
    }
}