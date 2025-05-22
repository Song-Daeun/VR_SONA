using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceResultDetector : MonoBehaviour
{
    [System.Serializable]
    public class DiceNumber
    {
        public int number;               // 주사위 숫자
        public Vector3 localPosition;    
        public Vector3 localNormal;      
        public Vector3 numberUpDirection;
        public Transform faceObject;    
    }
    
    [Header("Dice Numbers Settings")]
    public DiceNumber[] diceNumbers = new DiceNumber[8];

    [Header("Physics Settings")]
    public Rigidbody diceRigidbody;               
    public float stopThreshold = 0.1f;         
    public float stableTime = 1.0f;
    
    [Header("Camera Reference")]
    public Camera playerCamera; // 플레이어의 시점 카메라
    
    [Header("Debugging Settings")]
    public bool showDebugLogs = true;    
    public bool drawDebugGizmos = true;

    private bool resultConfirmed = false;      
    private int lastResult = -1;               
    
    private void Start()
    {
        // 주사위 면 오브젝트 자동 연결 - 이는 Plane_1부터 Plane_8까지의 자식 오브젝트들을 찾아서 연결합니다
        AutoConnectFaceObjects();
        
        // 오브젝트 기반으로 면 데이터 설정 - 각 면의 위치와 법선 벡터를 계산합니다
        SetupDiceFacesFromObjects();
        
        // 카메라 자동 찾기 - VR 환경에서 메인 카메라를 찾습니다
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Debug.Log($"Camera automatic assignment: {playerCamera.name}");
            }
            else
            {
                Debug.LogWarning("Main camera not found.");
            }
        }

        // 주사위 상태 감시 코루틴 시작 - 이것이 전체 로직의 시작점입니다
        StartCoroutine(WatchDiceUntilStop());
    }

    private IEnumerator WatchDiceUntilStop()
    {
        // Rigidbody가 할당되지 않았다면 오류 메시지 출력 후 종료
        if (diceRigidbody == null)
        {
            Debug.LogError("🎲 Rigidbody not assigned.");
            yield break;
        }

        float timer = 0f; // 주사위가 멈춘 시간을 측정하는 타이머

        // 무한 루프로 주사위의 움직임을 지속적으로 감시합니다
        while (true)
        {
            // 주사위의 선형 속도와 각속도가 모두 임계값 이하인지 확인
            // 이 조건이 만족되면 주사위가 "거의 멈췄다"고 판단합니다
            if (diceRigidbody.velocity.magnitude < stopThreshold &&
                diceRigidbody.angularVelocity.magnitude < stopThreshold)
            {
                timer += Time.deltaTime; // 멈춘 시간 누적

                // 설정된 안정화 시간 이상 멈춰있으면 결과 확정
                if (timer >= stableTime)
                    break; // while 루프 탈출
            }
            else
            {
                // 주사위가 다시 움직이기 시작하면 타이머 리셋
                timer = 0f;
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 이미 결과가 확정되었다면 중복 처리 방지
        if (resultConfirmed) yield break;

        // 주사위 결과 계산 - 가장 아래쪽에 있는 면의 숫자를 찾습니다
        int result = GetVisibleNumber();

        // 같은 결과가 연속으로 나오는 경우 중복 처리 방지
        if (result == lastResult)
        {
            Debug.Log("같은 주사위 결과가 반복됨. 처리 안 함.");
            yield break;
        }

        // 결과 확정 플래그 설정
        resultConfirmed = true;
        lastResult = result;

        Debug.Log($"🎲 주사위 결과: {result}");

        // ✅ 중요한 변경점: 직접 PlayerManager를 호출하지 않고 DiceSceneManager에 결과를 전달
        // 이렇게 하면 DiceSceneManager가 UI 표시와 플레이어 이동의 순서를 제어할 수 있습니다
        DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
        if (sceneManager != null)
        {
            // 결과를 DiceSceneManager에 전달 - 여기서 UI와 이동의 흐름이 시작됩니다
            sceneManager.OnDiceResultDetected(result);
        }
        else
        {
            Debug.LogWarning("DiceSceneManager가 씬에 없습니다.");
        }

        // 주의: DiceManager.OnBackButtonClicked()는 더 이상 여기서 호출하지 않습니다
        // 대신 DiceSceneManager에서 적절한 타이밍에 호출됩니다
    }

    // 주사위 면 숫자 매핑 - Inspector에서 수동으로 실행할 수 있는 컨텍스트 메뉴
    [ContextMenu("Auto Connect Face Objects")]
    private void AutoConnectFaceObjects()
    {    
        // 배열 초기화 - 8면 주사위이므로 8개의 요소가 필요합니다
        if (diceNumbers == null || diceNumbers.Length != 8)
        {
            diceNumbers = new DiceNumber[8];
            for (int i = 0; i < 8; i++)
            {
                diceNumbers[i] = new DiceNumber();
            }
        }
        
        // Plane_1부터 Plane_8까지의 자식 오브젝트를 찾아서 연결
        for (int i = 1; i <= 8; i++)
        {
            string planeName = $"Plane_{i}";
            Transform planeTransform = transform.Find(planeName);
            
            if (planeTransform != null)
            {
                int arrayIndex = i - 1; // 배열 인덱스는 0부터 시작하므로 1을 빼줍니다
                diceNumbers[arrayIndex].number = i;
                diceNumbers[arrayIndex].faceObject = planeTransform;
                // Debug.Log($"면 {i} 연결 성공: {planeName}");
            }
            else
            {
                Debug.LogWarning($"Object not found: {planeName}");
            }
        }
        
        // Debug.Log("주사위 면 오브젝트 자동 연결 완료");
    }
    
    // 면 데이터 설정 - 각 면의 위치와 방향 정보를 계산합니다
    [ContextMenu("Setup Dice Faces From Objects")]
    private void SetupDiceFacesFromObjects()
    { 
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            if (diceNumbers[i].faceObject != null)
            {
                // 월드 좌표를 주사위의 로컬 좌표계로 변환
                // 이렇게 하면 주사위가 회전해도 상대적 위치를 정확히 계산할 수 있습니다
                diceNumbers[i].localPosition = transform.InverseTransformPoint(diceNumbers[i].faceObject.position);
                
                // 면의 법선 벡터 계산 - Plane 오브젝트의 뒷면 방향이 법선입니다
                Vector3 worldNormal = diceNumbers[i].faceObject.TransformDirection(Vector3.back);
                diceNumbers[i].localNormal = transform.InverseTransformDirection(worldNormal);
                
                // 기본 위 방향 설정 (필요시 조정 가능)
                diceNumbers[i].numberUpDirection = Vector3.up;
            }
            else
            {
                Debug.LogWarning($"Object in face {i+1} is not connected.");
            }
        }
        // DebugAllFacePositions();
    }
    
    // 바닥에 닿아있는 면의 숫자를 반환 - 이것이 주사위 결과를 결정하는 핵심 함수입니다
    public int GetVisibleNumber()
    {
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("No dice face information!");
            return -1;
        }
        
        return GetBottomFacingNumber();
    }
    
    // 가장 낮은 위치에 있는 면 찾기 - Y축 좌표가 가장 낮은 면이 바닥에 닿은 면입니다
    private int GetBottomFacingNumber()
    {
        float lowestY = float.MaxValue; // 가장 낮은 Y 좌표값을 저장
        int bottomNumber = 1; // 바닥에 닿은 면의 숫자
        bool hasValidFace = false; // 유효한 면이 하나라도 있는지 확인
        
        // 모든 면을 순회하면서 가장 낮은 위치의 면을 찾습니다
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            DiceNumber face = diceNumbers[i];
            
            if (face.faceObject != null)
            {
                hasValidFace = true;
                Vector3 worldPosition = face.faceObject.position;
                
                // 현재 면이 지금까지 찾은 것보다 더 낮은 위치에 있다면 업데이트
                if (worldPosition.y < lowestY)
                {
                    lowestY = worldPosition.y;
                    bottomNumber = face.number;
                }
            }
            else
            {
                Debug.LogWarning($"Object for face {face.number} is null!");
            }
        }
        
        if (!hasValidFace)
        {
            Debug.LogError("No valid face object!");
            return 1; // 기본값 반환
        }
        
        // Debug.Log($"<color=green>바닥에 닿은 면: {bottomNumber} (Y값: {lowestY:F3})</color>");
        return bottomNumber;
    }
    
    // Scene 뷰에서 기즈모로 시각화 - 개발 중에 주사위 상태를 시각적으로 확인할 수 있습니다
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || diceNumbers == null) return;
        
        // 주사위 중심점을 노란색 구체로 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        
        // 각 면 표시 및 가장 낮은 면 강조
        float lowestY = float.MaxValue;
        int lowestFaceIndex = -1;
        
        // 먼저 가장 낮은 면을 찾습니다
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            
            if (face.faceObject != null)
            {
                Vector3 worldPos = face.faceObject.position;
                
                if (worldPos.y < lowestY)
                {
                    lowestY = worldPos.y;
                    lowestFaceIndex = i;
                }
            }
        }
        
        // 이제 모든 면을 시각화합니다
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            
            if (face.faceObject != null)
            {
                Vector3 worldPos = face.faceObject.position;
                
                // 가장 낮은 면은 빨간색, 나머지는 초록색으로 표시
                Gizmos.color = (i == lowestFaceIndex) ? Color.red : Color.green;
                Gizmos.DrawSphere(worldPos, 0.03f);
                
                // 면의 법선 벡터를 파란색 선으로 표시
                Gizmos.color = Color.blue;
                Vector3 normal = face.faceObject.TransformDirection(Vector3.back);
                Gizmos.DrawRay(worldPos, normal * 0.1f);
                
                #if UNITY_EDITOR
                // 에디터에서만 숫자 라벨 표시 
                UnityEditor.Handles.color = (i == lowestFaceIndex) ? Color.red : Color.green;
                UnityEditor.Handles.Label(worldPos + normal * 0.05f, face.number.ToString());
                #endif
            }
        }
        
        // 바닥 방향을 흰색 화살표로 표시
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
    }
}