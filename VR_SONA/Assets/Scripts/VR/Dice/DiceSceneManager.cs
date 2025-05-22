using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSceneManager : MonoBehaviour
{
    [Header("Reference")]
    public Rigidbody diceRigidbody;              // 주사위 Rigidbody
    public DiceResultDetector diceDetector;      // 결과 감지
    public DiceResultUI resultUI;                // 결과 표시 UI
    
    [Header("Result Detection settings")]
    public float stoppedVelocityThreshold = 0.1f;
    public float stoppedAngularThreshold = 0.1f;
    public float settleTime = 1.0f;              // 멈춘 상태 유지
    
    [Header("Game Flow Settings")]
    public float uiDisplayDelay = 1.0f;          // UI 표시 완료 후 대기 시간
    public float moveCompleteDelay = 2.0f;       // 플레이어 이동 완료 후 대기 시간
    
    [Header("Debugging")]
    public bool showDebugLogs = true;
    public bool drawDebugVisuals = true;
    
    private bool isRolling = false;
    private bool resultShown = false;
    private float stoppedTimer = 0f;
    private Vector3 diceInitialPosition;
    private Quaternion diceInitialRotation;

    private bool isResultDisplayed = false; // 결과 표시 여부
    private float minVelocityThreshold = 0.1f; // 움직임 감지 임계값
    
    // 새로 추가: 현재 게임 상태를 추적하는 변수
    private bool isProcessingResult = false; // 결과 처리 중인지 확인
    
    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log("DiceSceneManager 초기화 시작");
        }
        
        // 주사위 초기 위치 저장 - 나중에 리셋할 때 사용됩니다
        if (diceRigidbody != null)
        {
            diceInitialPosition = diceRigidbody.transform.position;
            diceInitialRotation = diceRigidbody.transform.rotation;
            
            if (showDebugLogs)
            {
                Debug.Log($"주사위 초기 위치 저장: {diceInitialPosition}");
            }
        }
        else
        {
            Debug.LogError("DiceRigidbody가 할당되지 않았습니다!");
        }
        
        // 카메라 설정 확인 및 자동 할당
        if (diceDetector != null && diceDetector.playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                diceDetector.playerCamera = mainCamera;
                if (showDebugLogs)
                {
                    Debug.Log($"DiceDetector에 카메라 자동 할당: {mainCamera.name}");
                }
            }
            else
            {
                Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
            }
        }
        
        // UI 컴포넌트 확인
        if (resultUI == null)
        {
            Debug.LogError("DiceResultUI가 할당되지 않았습니다!");
        }
        
        if (showDebugLogs)
        {
            Debug.Log("DiceResultManager Initialization Complete");
        }
    }
    
    private void Update()
    {
        // 기존의 주사위 상태 체크 로직 유지
        CheckDiceState();
    }
    
    // 기존 CheckDiceState 메서드는 그대로 유지하되, 주석을 더 자세히 추가
    private void CheckDiceState()
    {       
        // 주사위의 현재 물리적 상태 확인
        float velocity = diceRigidbody.velocity.magnitude;        // 선형 속도
        float angularVelocity = diceRigidbody.angularVelocity.magnitude; // 각속도
        
        // 디버그 로그 출력 (60프레임마다, 즉 1초에 한 번씩)
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Dice Velocity: {velocity}, Angular: {angularVelocity}, isRolling: {isRolling}, resultShown: {resultShown}");
        }

        // 주사위가 움직이기 시작하면 이전 결과 화면을 정리합니다
        if (velocity > minVelocityThreshold && isResultDisplayed)
        {
            if (showDebugLogs)
            {
                Debug.Log("주사위가 다시 움직이기 시작함. 이전 결과 화면 정리.");
            }
            
            // 이전 결과 UI 숨기기
            if (resultUI != null && resultUI.resultPanel != null)
            {
                resultUI.resultPanel.SetActive(false);
            }
            
            // 모든 상태 플래그 초기화
            isResultDisplayed = false;
            resultShown = false;
            stoppedTimer = 0f;
            isProcessingResult = false; // 새로운 결과 처리 준비
        }
        
        // 주사위가 멈췄는지 확인 (물리적 임계값 기준)
        bool isStill = velocity < stoppedVelocityThreshold && angularVelocity < stoppedAngularThreshold;
        
        if (isStill && isRolling)
        {
            // 주사위가 멈춘 상태로 일정 시간 유지되고 있는지 확인
            stoppedTimer += Time.deltaTime;
      
            // 결과 계산 및 표시 (한 번만 실행되도록 보장)
            if (stoppedTimer >= settleTime && !resultShown)
            {
                ShowDiceResult();
            }
        }
        else if (!isStill)
        {
            // 주사위가 움직이기 시작했을 때의 초기화 작업
            if (!isRolling)
            {
                isRolling = true;
                
                if (showDebugLogs)
                {
                    Debug.Log("주사위 굴리기 시작");
                }
                
                // 이전 결과 UI가 표시되어 있다면 숨기기
                if (resultUI != null && resultUI.resultPanel != null && resultUI.resultPanel.activeSelf)
                {
                    resultUI.resultPanel.SetActive(false);
                }
            }
            
            stoppedTimer = 0f; // 타이머 리셋
        }
    }
    
    // 기존 ShowDiceResult 메서드 유지 (이 메서드는 DiceResultDetector가 아닌 여기서만 사용됩니다)
    private void ShowDiceResult()
    {
        // 중복 실행 방지
        if (isResultDisplayed)
        {
            Debug.Log("Results already displayed");
            return;
        }
        
        // 필수 컴포넌트 확인
        if (diceDetector == null)
        {
            Debug.LogError("diceDetector is null!");
            return;
        }
        
        if (resultUI == null)
        {
            Debug.LogError("resultUI is null!");
            return;
        }
        
        // 결과 감지 및 UI 표시
        int result = diceDetector.GetVisibleNumber();
        
        if (showDebugLogs)
        {
            Debug.Log($"주사위 결과 UI 표시 준비: {result}");
        }
        
        // UI에 결과 표시 (콜백 함수 없이)
        resultUI.ShowResult(result);
        
        // 상태 업데이트
        resultShown = true;
        isRolling = false;
        isResultDisplayed = true; 
        
        // 결과 확정 이벤트 발생 (추가 게임 로직을 위한 확장 포인트)
        OnDiceResultConfirmed(result);
    }
    
    // ✅ 새로 추가된 핵심 메서드: DiceResultDetector에서 호출됩니다
    public void OnDiceResultDetected(int result)
    {
        // 이미 결과를 처리 중이라면 중복 실행 방지
        if (isProcessingResult)
        {
            Debug.Log("이미 결과 처리 중입니다.");
            return;
        }
        
        isProcessingResult = true; // 결과 처리 시작 플래그 설정
        
        if (showDebugLogs)
        {
            Debug.Log($"주사위 결과 감지됨: {result}. UI 표시 및 게임 로직 시작.");
        }
        
        // 결과 처리 코루틴 시작 - 이것이 새로운 게임 흐름의 시작점입니다
        StartCoroutine(HandleDiceResultFlow(result));
    }
    
    // ✅ 새로 추가: 주사위 결과부터 씬 정리까지의 전체 흐름을 관리하는 코루틴
    private IEnumerator HandleDiceResultFlow(int result)
    {
        if (showDebugLogs)
        {
            Debug.Log("=== 주사위 결과 처리 흐름 시작 ===");
        }
        
        // 1단계: UI에 결과 표시 (콜백 함수와 함께)
        if (resultUI != null)
        {
            // 중요: UI 표시가 완료되면 실행될 콜백 함수를 정의합니다
            System.Action onUIComplete = () => {
                if (showDebugLogs)
                {
                    Debug.Log("UI 표시 완료 콜백 실행됨");
                }
                // 이 콜백은 DiceResultUI.DisplayResultCoroutine에서 적절한 타이밍에 호출됩니다
            };
            
            // UI 표시 시작 (콜백 함수와 함께)
            resultUI.ShowResult(result, onUIComplete);
            
            if (showDebugLogs)
            {
                Debug.Log("UI 결과 표시 요청 완료");
            }
        }
        else
        {
            Debug.LogError("ResultUI가 null입니다!");
            yield break;
        }
        
        // 2단계: UI 표시가 완전히 완료될 때까지 대기
        // 이 시간은 DiceResultUI의 fadeInDuration + 추가 대기 시간과 일치해야 합니다
        float totalUITime = (resultUI.fadeInDuration + 0.5f); // 페이드인 + UI 안정화 시간
        yield return new WaitForSeconds(totalUITime + uiDisplayDelay);
        
        if (showDebugLogs)
        {
            Debug.Log("UI 표시 완료. 플레이어 이동 시작.");
        }
        
        // 3단계: 플레이어 이동 실행
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.MovePlayer(result);
            
            if (showDebugLogs)
            {
                Debug.Log($"플레이어 {result}칸 이동 명령 전송");
            }
        }
        else
        {
            Debug.LogWarning("PlayerManager가 씬에 없습니다.");
        }
        
        // 4단계: 플레이어 이동 완료 대기
        // PlayerManager의 moveDuration과 일치하거나 약간 더 길게 설정
        float estimatedMoveTime = playerManager != null ? playerManager.moveDuration : 0.5f;
        yield return new WaitForSeconds(estimatedMoveTime + moveCompleteDelay);
        
        if (showDebugLogs)
        {
            Debug.Log("플레이어 이동 완료. 씬 정리 시작.");
        }
        
        // 5단계: 주사위 씬 정리 및 메인 씬으로 복귀
        DiceManager diceManager = FindObjectOfType<DiceManager>();
        if (diceManager != null)
        {
            diceManager.OnBackButtonClicked();
            
            if (showDebugLogs)
            {
                Debug.Log("DiceManager.OnBackButtonClicked() 호출됨");
            }
        }
        else
        {
            Debug.LogWarning("DiceManager가 씬에 없습니다.");
        }
        
        if (showDebugLogs)
        {
            Debug.Log("=== 주사위 결과 처리 흐름 완료 ===");
        }
        
        // 처리 완료 플래그 해제
        isProcessingResult = false;
    }
    
    // 기존 OnDiceResultConfirmed 메서드 유지
    private void OnDiceResultConfirmed(int result)
    {
        // 나중에 게임 로직 확장 시 사용할 수 있는 이벤트 포인트
        // 예: 점수 계산, 특수 효과, 사운드 재생 등
        if (showDebugLogs)
        {
            Debug.Log($"주사위 결과 확정: {result}");
        }
    }
    
    // 기존 ResetDice 메서드 유지
    public void ResetDice()
    {
        if (diceRigidbody == null) return;
        
        if (showDebugLogs)
        {
            Debug.Log("주사위 리셋 시작");
        }
        
        // 물리 상태 완전 초기화
        diceRigidbody.velocity = Vector3.zero;
        diceRigidbody.angularVelocity = Vector3.zero;
        
        // 위치와 회전 리셋
        diceRigidbody.transform.position = diceInitialPosition;
        diceRigidbody.transform.rotation = diceInitialRotation;
        
        // 모든 상태 플래그 초기화
        isRolling = false;
        resultShown = false;
        stoppedTimer = 0f;
        isResultDisplayed = false;
        isProcessingResult = false;
        
        // UI도 함께 숨기기
        if (resultUI != null && resultUI.resultPanel != null)
        {
            resultUI.resultPanel.SetActive(false);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("주사위 리셋 완료");
        }
    }
    
    // 기존 OnDrawGizmos 메서드 유지
    private void OnDrawGizmos()
    {
        if (!drawDebugVisuals || diceRigidbody == null) return;
        
        // 주사위 속력을 청록색 화살표로 표시
        Gizmos.color = Color.cyan;
        Vector3 dicePos = diceRigidbody.transform.position;
        Gizmos.DrawRay(dicePos, diceRigidbody.velocity);
        
        // 주사위 상태에 따른 색상 변경으로 현재 상태를 시각적으로 표시
        if (isProcessingResult)
        {
            // 결과 처리 중일 때는 보라색
            Gizmos.color = Color.magenta;
        }
        else if (isRolling)
        {
            // 굴러가는 중일 때는 노란색
            Gizmos.color = Color.yellow;
        }
        else if (resultShown)
        {
            // 결과가 표시된 후에는 초록색
            Gizmos.color = Color.green;
        }
        else
        {
            // 대기 상태일 때는 회색
            Gizmos.color = Color.gray;
        }
        
        // 주사위 주위에 상태를 나타내는 원형 와이어프레임 표시
        Gizmos.DrawWireSphere(dicePos, 0.3f);
        
        // 초기 위치를 흰색 십자가로 표시 (리셋 위치 확인용)
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(diceInitialPosition, Vector3.one * 0.1f);
        
        #if UNITY_EDITOR
        // 에디터에서만 현재 상태를 텍스트로 표시
        string statusText = "";
        if (isProcessingResult) statusText = "Processing Result";
        else if (isRolling) statusText = "Rolling";
        else if (resultShown) statusText = "Result Shown";
        else statusText = "Waiting";
        
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(dicePos + Vector3.up * 0.5f, $"Dice Status: {statusText}");
        
        // 속도 정보도 텍스트로 표시
        float velocity = diceRigidbody.velocity.magnitude;
        float angularVel = diceRigidbody.angularVelocity.magnitude;
        UnityEditor.Handles.Label(dicePos + Vector3.up * 0.7f, 
            $"V: {velocity:F2} | AV: {angularVel:F2}");
        #endif
    }
    
    // ✅ 새로 추가: 외부에서 현재 상태를 확인할 수 있는 공개 메서드들
    public bool IsProcessingResult()
    {
        return isProcessingResult;
    }
    
    public bool IsRolling()
    {
        return isRolling;
    }
    
    public bool IsResultShown()
    {
        return resultShown;
    }
    
    // ✅ 새로 추가: 강제로 결과 처리를 중단하는 메서드 (비상시 사용)
    public void ForceStopResultProcessing()
    {
        if (showDebugLogs)
        {
            Debug.Log("결과 처리 강제 중단");
        }
        
        StopAllCoroutines(); // 모든 코루틴 중단
        isProcessingResult = false;
        
        // UI도 강제로 숨기기
        if (resultUI != null && resultUI.resultPanel != null)
        {
            resultUI.resultPanel.SetActive(false);
        }
    }
    
    // ✅ 새로 추가: 게임 플로우 설정값들을 런타임에 조정할 수 있는 메서드들
    public void SetUIDisplayDelay(float delay)
    {
        uiDisplayDelay = Mathf.Max(0f, delay);
        if (showDebugLogs)
        {
            Debug.Log($"UI 표시 지연 시간 변경: {uiDisplayDelay}초");
        }
    }
    
    public void SetMoveCompleteDelay(float delay)
    {
        moveCompleteDelay = Mathf.Max(0f, delay);
        if (showDebugLogs)
        {
            Debug.Log($"이동 완료 대기 시간 변경: {moveCompleteDelay}초");
        }
    }
}