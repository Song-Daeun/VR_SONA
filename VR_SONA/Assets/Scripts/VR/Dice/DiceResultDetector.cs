using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceResultDetector : MonoBehaviour
{
    [System.Serializable]
    public class DiceNumber
    {
        public int number;               // 주사위 숫자 (1~8)
        public Vector3 localPosition;    // 주사위 로컬 공간에서 숫자의 위치
        public Vector3 localNormal;      // 숫자가 그려진 면의 법선 벡터
        public Vector3 numberUpDirection; // 숫자의 "위" 방향 (정방향 판별용)
    }
    
    [Header("주사위 숫자 설정")]
    public DiceNumber[] diceNumbers = new DiceNumber[8];
    
    [Header("카메라 참조")]
    public Camera playerCamera; // 플레이어의 시점 카메라 (기존 방식 유지)
    
    [Header("디버깅 설정")]
    public bool showDebugLogs = true;    // 디버그 로그 표시 여부
    public bool drawDebugGizmos = true;  // Scene 뷰에서 기즈모 표시 여부
    
    // Start 함수 - 기존 카메라 자동 찾기 로직 유지
    private void Start()
    {
        Debug.Log("=== DiceResultDetector Start 호출 ===");
        
        // 주사위 면이 초기화되지 않았다면 자동 초기화
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.Log("주사위 면 정보가 없어서 자동 초기화를 시작합니다.");
            InitializeDiceFacesBasedOnLayout();
        }
        else
        {
            Debug.Log($"기존 주사위 면 데이터가 있습니다. 개수: {diceNumbers.Length}");
            // 기존 데이터 검증
            CheckExistingData();
        }
        
        // 카메라 자동 찾기 (기존 방식)
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Debug.Log($"카메라 자동 할당: {playerCamera.name}");
            }
            else
            {
                Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
            }
        }
        
        Debug.Log("=== DiceResultDetector Start 완료 ===");
    }

    // 기존 데이터 검증 함수 추가
    private void CheckExistingData()
    {
        Debug.Log("=== 기존 주사위 면 데이터 검증 ===");
        bool needsReinit = false;
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            if (diceNumbers[i] == null)
            {
                Debug.LogWarning($"면 {i}이 null입니다.");
                needsReinit = true;
            }
            else if (diceNumbers[i].localPosition.magnitude < 0.01f)
            {
                Debug.LogWarning($"면 {diceNumbers[i].number}의 로컬 위치가 0입니다: {diceNumbers[i].localPosition}");
                needsReinit = true;
            }
            else
            {
                Debug.Log($"면 {diceNumbers[i].number}: 로컬 위치={diceNumbers[i].localPosition}");
            }
        }
        
        if (needsReinit)
        {
            Debug.LogError("데이터가 손상되어 재초기화가 필요합니다.");
            InitializeDiceFacesBasedOnLayout();
        }
    }
    
    // 주사위 면 초기화 함수
    [ContextMenu("Initialize Dice Faces Based on Layout")]
    private void InitializeDiceFacesBasedOnLayout()
    {
        Debug.Log("=== 주사위 면 초기화 시작 ===");
        
        // 배열 초기화
        if (diceNumbers == null || diceNumbers.Length != 8)
        {
            Debug.Log("새로운 배열 생성");
            diceNumbers = new DiceNumber[8];
            for (int i = 0; i < 8; i++)
            {
                diceNumbers[i] = new DiceNumber();
            }
        }
        else
        {
            Debug.Log("기존 배열 재사용");
        }
        
        // 현재 주사위의 스케일 고려
        float scaleMultiplier = 0.5f; // 당신의 주사위 스케일
        float radius = 0.577f * scaleMultiplier; // 중심에서 면까지의 거리
        
        Debug.Log($"스케일 배수: {scaleMultiplier}, 반지름: {radius}");
        
        // X축 90도 회전을 고려한 면 배치
        // 위쪽 면들 (Y가 양수, 정면부터 시계방향으로 1-7-5-3)
        
        // 면 1: 정면 위쪽 (X축 90도 회전 후)
        diceNumbers[0] = new DiceNumber
        {
            number = 1,
            localPosition = new Vector3(0, 0.577f * scaleMultiplier * 0.866f, radius),
            localNormal = new Vector3(0, 0.866f, 0.5f).normalized,
            numberUpDirection = Vector3.forward  // X축 회전 후 Z축이 위쪽
        };
        Debug.Log($"면 1 초기화: 위치={diceNumbers[0].localPosition}, 법선={diceNumbers[0].localNormal}");
        
        // 면 7: 오른쪽 위쪽 (정면에서 시계방향 90도)
        diceNumbers[1] = new DiceNumber
        {
            number = 7,
            localPosition = new Vector3(radius, 0.577f * scaleMultiplier * 0.866f, 0),
            localNormal = new Vector3(0.5f, 0.866f, 0).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 7 초기화: 위치={diceNumbers[1].localPosition}, 법선={diceNumbers[1].localNormal}");
        
        // 면 5: 뒤쪽 위쪽 (정면에서 시계방향 180도)
        diceNumbers[2] = new DiceNumber
        {
            number = 5,
            localPosition = new Vector3(0, 0.577f * scaleMultiplier * 0.866f, -radius),
            localNormal = new Vector3(0, 0.866f, -0.5f).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 5 초기화: 위치={diceNumbers[2].localPosition}, 법선={diceNumbers[2].localNormal}");
        
        // 면 3: 왼쪽 위쪽 (정면에서 시계방향 270도)
        diceNumbers[3] = new DiceNumber
        {
            number = 3,
            localPosition = new Vector3(-radius, 0.577f * scaleMultiplier * 0.866f, 0),
            localNormal = new Vector3(-0.5f, 0.866f, 0).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 3 초기화: 위치={diceNumbers[3].localPosition}, 법선={diceNumbers[3].localNormal}");
        
        // 아래쪽 면들 (Y가 음수, 정면부터 시계방향으로 4-6-8-2)
        
        // 면 4: 정면 아래쪽
        diceNumbers[4] = new DiceNumber
        {
            number = 4,
            localPosition = new Vector3(0, -0.577f * scaleMultiplier * 0.866f, radius),
            localNormal = new Vector3(0, -0.866f, 0.5f).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 4 초기화: 위치={diceNumbers[4].localPosition}, 법선={diceNumbers[4].localNormal}");
        
        // 면 6: 오른쪽 아래쪽
        diceNumbers[5] = new DiceNumber
        {
            number = 6,
            localPosition = new Vector3(radius, -0.577f * scaleMultiplier * 0.866f, 0),
            localNormal = new Vector3(0.5f, -0.866f, 0).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 6 초기화: 위치={diceNumbers[5].localPosition}, 법선={diceNumbers[5].localNormal}");
        
        // 면 8: 뒤쪽 아래쪽
        diceNumbers[6] = new DiceNumber
        {
            number = 8,
            localPosition = new Vector3(0, -0.577f * scaleMultiplier * 0.866f, -radius),
            localNormal = new Vector3(0, -0.866f, -0.5f).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 8 초기화: 위치={diceNumbers[6].localPosition}, 법선={diceNumbers[6].localNormal}");
        
        // 면 2: 왼쪽 아래쪽
        diceNumbers[7] = new DiceNumber
        {
            number = 2,
            localPosition = new Vector3(-radius, -0.577f * scaleMultiplier * 0.866f, 0),
            localNormal = new Vector3(-0.5f, -0.866f, 0).normalized,
            numberUpDirection = Vector3.forward
        };
        Debug.Log($"면 2 초기화: 위치={diceNumbers[7].localPosition}, 법선={diceNumbers[7].localNormal}");
        
        Debug.Log("X축 90도 회전을 고려한 주사위 면 초기화가 완료되었습니다.");
        
        // 초기화 후 자동으로 디버깅 정보 출력
        DebugAllFacePositions();
    }

    // 데이터 무결성 테스트 (새로 추가한 함수)
    private void TestDataIntegrity()
    {
        Debug.Log("=== 데이터 무결성 테스트 ===");
        
        bool allValid = true;
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            bool valid = diceNumbers[i] != null && 
                        diceNumbers[i].localPosition.magnitude > 0.01f &&
                        diceNumbers[i].number >= 1 && diceNumbers[i].number <= 8;
            
            string status = valid ? "<color=green>유효</color>" : "<color=red>무효</color>";
            Debug.Log($"면 {i + 1}: {status} - 번호={diceNumbers[i]?.number}, 위치={diceNumbers[i]?.localPosition}");
            
            if (!valid) allValid = false;
        }
        
        if (allValid)
        {
            Debug.Log("<color=green>모든 데이터가 유효합니다!</color>");
        }
        else
        {
            Debug.LogError("<color=red>일부 데이터가 무효합니다!</color>");
        }
    }
    
    // =================================================================
    // 핵심 함수: 바닥에 닿아있는 면의 숫자를 반환 (기존 GetVisibleNumber 교체)
    // =================================================================
    public int GetVisibleNumber()
    {
        Debug.Log("GetVisibleNumber 함수 시작 (바닥 면 감지 방식)");
        
        // 필수 컴포넌트 체크 (기존 방식 유지)
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("주사위 면 정보가 없습니다!");
            return -1;
        }
        
        // 카메라 체크는 더 이상 필수가 아니지만, 기존 호환성을 위해 유지
        if (playerCamera == null)
        {
            Debug.LogWarning("플레이어 카메라가 설정되지 않았습니다. 바닥 면 감지 방식을 사용합니다.");
            playerCamera = Camera.main;
        }
        
        // 바닥에 닿은 면 찾기 (새로운 방식)
        return GetBottomFacingNumber();
    }
    
    // 가장 낮은 위치에 있는 면 찾기 (바닥에 닿은 면)
    private int GetBottomFacingNumber()
    {
        Debug.Log("바닥에 닿은 면 감지 시작");
        
        // 1. 데이터 유효성 검사
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("주사위 면 데이터가 초기화되지 않았습니다!");
            return 1;
        }
        
        // 2. Transform 상태 확인
        if (transform.localScale.magnitude < 0.01f)
        {
            Debug.LogWarning("주사위 스케일이 너무 작습니다!");
        }
        
        float lowestY = float.MaxValue;
        int bottomNumber = 1;
        bool hasValidData = false;
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            DiceNumber face = diceNumbers[i];
            
            // 3. 로컬 위치의 유효성 확인
            if (face.localPosition.magnitude > 0.01f)
            {
                hasValidData = true;
                Vector3 worldPosition = transform.TransformPoint(face.localPosition);
                
                // 4. 변환 결과의 유효성 확인
                if (worldPosition.y < lowestY)
                {
                    lowestY = worldPosition.y;
                    bottomNumber = face.number;
                }
                
                Debug.Log($"[면 {face.number}] 로컬: {face.localPosition} → 월드: {worldPosition}");
            }
            else
            {
                Debug.LogWarning($"면 {face.number}의 로컬 위치가 0입니다!");
            }
        }
        
        if (!hasValidData)
        {
            Debug.LogError("유효한 면 데이터가 없습니다! 초기화를 다시 실행하세요.");
            return 1;
        }
        
        Debug.Log($"<color=green>바닥에 닿은 면: {bottomNumber} (Y값: {lowestY:F3})</color>");
        return bottomNumber;
    }
    
    // 레이캐스트를 사용한 바닥 면 감지 (대안 방법)
    private int GetBottomFaceUsingRaycast()
    {
        Debug.Log("레이캐스트를 사용한 바닥 면 감지");
        
        // 주사위 중심에서 아래로 레이 발사
        Vector3 rayStart = transform.position;
        Ray ray = new Ray(rayStart, Vector3.down);
        
        // 주사위 표면까지의 거리를 고려하여 약간 긴 레이 사용
        float rayLength = 1.0f;
        
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
        {
            Debug.Log($"레이 충돌 지점: {hit.point}");
            Debug.Log($"충돌한 오브젝트: {hit.collider.name}");
            
            // 충돌한 면의 번호 추출
            return ExtractNumberFromCollider(hit.collider);
        }
        
        Debug.LogWarning("바닥 면을 찾을 수 없습니다");
        return GetBottomFacingNumber(); // 대안 방법으로 전환
    }
    
    // 콜라이더 이름에서 숫자 추출
    private int ExtractNumberFromCollider(Collider collider)
    {
        // 콜라이더 이름에서 숫자 추출 (예: "Face_1", "Number1" 등)
        string name = collider.name;
        var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
        
        if (match.Success && int.TryParse(match.Value, out int number))
        {
            return number;
        }
        
        // 이름에서 번호를 찾을 수 없으면 가장 낮은 면으로 대체
        return GetBottomFacingNumber();
    }
    
    // =================================================================
    // 디버깅 및 테스트 함수들 (기존 함수들 일부 유지)
    // =================================================================
    
    [ContextMenu("Test Current Dice Result")]
    public void TestCurrentDiceResult()
    {
        Debug.Log("=== 현재 주사위 결과 테스트 ===");
        
        int result = GetVisibleNumber();
        
        Debug.Log($"<color=yellow>주사위 결과: {result}</color>");
        Debug.Log("==========================================");
    }
    
    [ContextMenu("Debug All Face Positions")]
    public void DebugAllFacePositions()
    {
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("주사위 면들이 초기화되지 않았습니다.");
            return;
        }
        
        Debug.Log("=== 주사위 면 매핑 디버깅 정보 ===");
        Debug.Log($"주사위 현재 위치: {transform.position}");
        Debug.Log($"주사위 현재 회전: {transform.eulerAngles}");
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            Vector3 worldPos = transform.TransformPoint(face.localPosition);
            
            string description = GetDirectionDescription(face.localPosition);
            
            Debug.Log($"숫자 {face.number}: " +
                     $"로컬위치={face.localPosition}, " +
                     $"월드위치={worldPos}, " +
                     $"Y값={worldPos.y:F3}, " +
                     $"설명='{description}'");
        }
        
        Debug.Log("================================");
    }
    
    // 위치를 문자로 설명하는 도우미 함수
    private string GetDirectionDescription(Vector3 localPos)
    {
        string desc = "";
        
        // Y축 방향 (위/아래)
        if (localPos.y > 0.1f) desc += "위쪽 ";
        else if (localPos.y < -0.1f) desc += "아래쪽 ";
        
        // X축 방향 (좌/우)
        if (localPos.x > 0.1f) desc += "오른쪽 ";
        else if (localPos.x < -0.1f) desc += "왼쪽 ";
        
        // Z축 방향 (앞/뒤)
        if (localPos.z > 0.1f) desc += "정면";
        else if (localPos.z < -0.1f) desc += "뒤쪽";
        else desc += "측면";
        
        return desc.Trim();
    }
    
    // VR 카메라 디버깅 정보 (기존 함수 유지)
    private void DebugVRCameraInfo()
    {
        if (playerCamera == null) return;
        
        // VR 헤드셋 정보
        Debug.Log("=== VR 카메라 디버깅 정보 ===");
        Debug.Log($"카메라 이름: {playerCamera.name}");
        Debug.Log($"카메라 태그: {playerCamera.tag}");
        Debug.Log($"카메라 레이어 마스크: {playerCamera.cullingMask}");
        
        // VR에서는 실제 플레이어 머리 위치와 카메라 위치가 다를 수 있음
        Transform cameraRig = playerCamera.transform.parent;
        if (cameraRig != null)
        {
            Debug.Log($"카메라 부모: {cameraRig.name}");
            Debug.Log($"부모 위치: {cameraRig.position}");
            
            // XR Origin까지 올라가기
            Transform origin = cameraRig.parent;
            if (origin != null && origin.name.Contains("XR Origin"))
            {
                Debug.Log($"XR Origin 위치: {origin.position}");
            }
        }
    }
    
    // Scene 뷰에서 기즈모로 시각화
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || diceNumbers == null) return;
        
        // 주사위 중심 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        
        // 각 면 표시 및 가장 낮은 면 강조
        float lowestY = float.MaxValue;
        int lowestFaceIndex = -1;
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            
            // 월드 좌표로 변환
            Vector3 worldPos = transform.TransformPoint(face.localPosition);
            Vector3 worldNormal = transform.TransformDirection(face.localNormal);
            
            // 가장 낮은 면 찾기
            if (worldPos.y < lowestY)
            {
                lowestY = worldPos.y;
                lowestFaceIndex = i;
            }
            
            // 면의 위치 표시
            Gizmos.color = (i == lowestFaceIndex) ? Color.red : Color.white;
            Gizmos.DrawSphere(worldPos, 0.02f);
            
            // 아래로 향하는 면의 법선 벡터 표시 (파란색)
            if (worldNormal.y < -0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(worldPos, worldNormal * 0.1f);
            }
            
            #if UNITY_EDITOR
            // 숫자 라벨 표시 (가장 낮은 면은 빨간색)
            UnityEditor.Handles.color = (i == lowestFaceIndex) ? Color.red : Color.white;
            UnityEditor.Handles.Label(worldPos + worldNormal * 0.05f, face.number.ToString());
            #endif
        }
        
        // 바닥 방향 화살표 표시
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
    }
    
    // Update 함수 - 테스트용 키보드 입력 (기존 코드 일부 유지)
    private void Update()
    {
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            TestCurrentDiceResult();
        }
        
        // VR 카메라 정보 디버깅 (V키)
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            DebugVRCameraInfo();
        }
    }
}