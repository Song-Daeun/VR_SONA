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
    
    private void Start()
    {
        // 주사위 초기 위치 저장
        if (diceRigidbody != null)
        {
            diceInitialPosition = diceRigidbody.transform.position;
            diceInitialRotation = diceRigidbody.transform.rotation;
        }
        
        // 카메라 설정 확인
        if (diceDetector != null && diceDetector.playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                diceDetector.playerCamera = mainCamera;
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log("DiceResultManager Initialization Complete");
        }
    }
    
    private void Update()
    {
        // 실행확인    
        CheckDiceState();
    }
    
    private void CheckDiceState()
    {       
        // 주사위의 현재 속도 값
        float velocity = diceRigidbody.velocity.magnitude;
        float angularVelocity = diceRigidbody.angularVelocity.magnitude;
        
        if (showDebugLogs && Time.frameCount % 60 == 0) // 60프레임마다 로그 출력
        {
            Debug.Log($"Dice Velocity: {velocity}, Angular: {angularVelocity}, isRolling: {isRolling}, resultShown: {resultShown}");
        }

        // 주사위가 움직이면 이전 화면 언로드
        if (velocity > minVelocityThreshold && isResultDisplayed)
        {
            // 이전 결과 초기화
            if (resultUI != null && resultUI.resultPanel != null)
            {
                resultUI.resultPanel.SetActive(false);
            }
            
            // 모든 상태 플래그 초기화
            isResultDisplayed = false;
            resultShown = false;
            stoppedTimer = 0f;
        }
        
        // 주사위가 멈췄는지 확인 (임계값 조정)
        bool isStill = velocity < stoppedVelocityThreshold && angularVelocity < stoppedAngularThreshold;
        
        if (isStill && isRolling)
        {
            stoppedTimer += Time.deltaTime;
      
            // 결과 계산
            if (stoppedTimer >= settleTime && !resultShown)
            {
                ShowDiceResult();
            }
        }
        else if (!isStill)
        {
            // 주사위가 움직이기 시작했을 때
            if (!isRolling)
            {
                isRolling = true;
                
                // 이전 결과 숨기기
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
        
        // 이미 결과가 표시되었다면 실행하지 않음
        if (isResultDisplayed)
        {
            Debug.Log("Results already displayed");
            return;
        }
        
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
        
        // 결과 감지
        int result = diceDetector.GetVisibleNumber();
        
        // UI에 결과 표시
        resultUI.ShowResult(result);
        
        // 상태 업데이트
        resultShown = true;
        isRolling = false;
        isResultDisplayed = true; 
        
        // 결과 확정 이벤트 발생
        OnDiceResultConfirmed(result);
    }
    
    private void OnDiceResultConfirmed(int result)
    {
        // 나중에 게임 로직 확장 시 사용 (플레이어 이동, 점수 계산)
        if (showDebugLogs)
        {
            // Debug.Log($"Prepare to execute game logic for dice result {result}");
        }
    }
    
    // 주사위 초기 위치로 리셋
    public void ResetDice()
    {
        if (diceRigidbody == null) return;
        
        // 물리 상태 리셋
        diceRigidbody.velocity = Vector3.zero;
        diceRigidbody.angularVelocity = Vector3.zero;
        
        // 위치와 회전 리셋
        diceRigidbody.transform.position = diceInitialPosition;
        diceRigidbody.transform.rotation = diceInitialRotation;
        
        // 상태 초기화
        isRolling = false;
        resultShown = false;
        stoppedTimer = 0f;
        
        if (showDebugLogs)
        {
            Debug.Log("주사위가 초기 위치로 리셋됨");
        }
    }
    
    // Scene 뷰에서 디버그 정보 표시
    private void OnDrawGizmos()
    {
        if (!drawDebugVisuals || diceRigidbody == null) return;
        
        // 주사위 속력 표시
        Gizmos.color = Color.cyan;
        Vector3 dicePos = diceRigidbody.transform.position;
        Gizmos.DrawRay(dicePos, diceRigidbody.velocity);
        
        // 정지 상태 표시
        if (isRolling)
        {
            Gizmos.color = Color.yellow;
        }
        else if (resultShown)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.gray;
        }
        
        Gizmos.DrawWireSphere(dicePos, 0.3f);
    }
}
