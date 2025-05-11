using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSceneManager : MonoBehaviour
{
    [Header("참조")]
    public Rigidbody diceRigidbody;              // 주사위의 Rigidbody
    public DiceResultDetector diceDetector;      // 결과 감지기
    public DiceResultUI resultUI;                // 결과 표시 UI
    
    [Header("감지 설정")]
    public float stoppedVelocityThreshold = 0.1f;
    public float stoppedAngularThreshold = 0.1f;
    public float settleTime = 1.0f;              // 멈춘 상태 유지해야 하는 시간
    
    [Header("디버깅")]
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
                if (showDebugLogs)
                {
                    Debug.Log($"DiceResultManager: 카메라 자동 할당 - {mainCamera.name}");
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log("DiceResultManager 초기화 완료");
        }
    }
    
    private void Update()
    {
        // 실행확인
        if (Time.timeSinceLevelLoad < 5f && Time.frameCount % 60 == 0)
        {
            Debug.Log($"DiceSceneManager Update 실행 중 - 시간: {Time.timeSinceLevelLoad:F1}초");
        }
    
        CheckDiceState();
    }
    
    private void CheckDiceState()
    {
        if (diceRigidbody == null)
        {
            Debug.LogError("diceRigidbody가 null입니다!");
            return;
        }
        
        // 주사위의 현재 속도 값들을 한 번만 가져오기
        float velocity = diceRigidbody.velocity.magnitude;
        float angularVelocity = diceRigidbody.angularVelocity.magnitude;
        
        // 주사위가 움직이기 시작했는데 아직 결과가 표시되어 있다면
        if (velocity > minVelocityThreshold && isResultDisplayed)
        {
            // 이전 결과 숨기기
            if (resultUI != null && resultUI.resultPanel != null && resultUI.resultPanel.activeSelf)
            {
                resultUI.resultPanel.SetActive(false);
                Debug.Log("주사위가 다시 움직이기 시작 - 결과 초기화");
            }
            isResultDisplayed = false;
        }

        // 속도 변화가 클 때만 로그 출력 (성능 최적화)
        if (Time.frameCount % 60 == 0) // 약 1초마다
        {
            Debug.Log($"주사위 상태 - 속도: {velocity:F3}, 회전속도: {angularVelocity:F3}, isRolling: {isRolling}");
        }
        
        // 주사위가 멈췄는지 확인
        bool isStill = velocity < stoppedVelocityThreshold && angularVelocity < stoppedAngularThreshold;
        
        if (isStill && isRolling)
        {
            // 멈춘 시간 누적
            stoppedTimer += Time.deltaTime;
            
            // 정지 상태 로그 (0.2초마다)
            if (stoppedTimer % 0.2f < Time.deltaTime)
            {
                Debug.Log($"주사위 정지 중... ({stoppedTimer:F1}초 / {settleTime}초 필요)");
            }
            
            // 충분히 멈춘 후 결과 계산
            if (stoppedTimer >= settleTime && !resultShown)
            {
                Debug.Log("===== 결과 감지 시작! =====");
                ShowDiceResult();
            }
        }
        else if (!isStill)
        {
            // 주사위가 움직이기 시작했을 때
            if (!isRolling)
            {
                Debug.Log("주사위가 처음으로 움직이기 시작함");
                isRolling = true;
            }
            
            stoppedTimer = 0f;
            resultShown = false;
            
            // 이전 결과 UI 숨기기
            if (resultUI != null && resultUI.resultPanel != null && resultUI.resultPanel.activeSelf)
            {
                resultUI.resultPanel.SetActive(false);
                Debug.Log("이전 결과 UI 숨김");
            }
        }
    }
    
    private void ShowDiceResult()
    {
        Debug.Log("ShowDiceResult 함수 진입");
        
        // 이미 결과가 표시되었다면 실행하지 않음
        if (isResultDisplayed)
        {
            Debug.Log("이미 결과가 표시됨 - 반환");
            return;
        }
        
        if (diceDetector == null)
        {
            Debug.LogError("diceDetector가 null입니다!");
            return;
        }
        
        if (resultUI == null)
        {
            Debug.LogError("resultUI가 null입니다!");
            return;
        }
        
        // 결과 감지
        int result = diceDetector.GetVisibleNumber();
        Debug.Log($"감지된 주사위 결과: {result}");
        
        // UI에 결과 표시
        resultUI.ShowResult(result);
        Debug.Log("UI에 결과 표시 요청 완료");
        
        // 상태 업데이트
        resultShown = true;
        isRolling = false;
        isResultDisplayed = true; // 이 줄 추가!
        Debug.Log("결과 표시 완료");
        
        // 결과 확정 이벤트 발생
        OnDiceResultConfirmed(result);
    }
    
    private void OnDiceResultConfirmed(int result)
    {
        // 나중에 게임 로직 확장 시 사용
        // 예: 플레이어 이동, 점수 계산 등
        if (showDebugLogs)
        {
            Debug.Log($"주사위 결과 {result}에 대한 게임 로직 실행 준비");
        }
    }
    
    // 주사위를 초기 위치로 리셋
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
