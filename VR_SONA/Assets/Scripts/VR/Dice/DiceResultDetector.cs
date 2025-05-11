using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public Camera playerCamera; // 플레이어의 시점 카메라
    
    [Header("디버깅 설정")]
    public bool showDebugLogs = true;    // 디버그 로그 표시 여부
    public bool drawDebugGizmos = true;  // Scene 뷰에서 기즈모 표시 여부
    
    // 초기화 함수 수정 (변수명 오류 수정)
    [ContextMenu("Initialize Dice Faces Based on Layout")]
    private void InitializeDiceFacesBasedOnLayout()
    {
        // 배열 초기화
        if (diceNumbers == null || diceNumbers.Length != 8)
        {
            diceNumbers = new DiceNumber[8];
            for (int i = 0; i < 8; i++)
            {
                diceNumbers[i] = new DiceNumber();
            }
        }
        
        // 현재 주사위의 스케일 고려
        float scaleMultiplier = 0.5f; // 당신의 주사위 스케일
        float radius = 0.577f * scaleMultiplier; // 중심에서 면까지의 거리
        
        // 정면을 Z+ 방향으로 가정
        // 1-7-5-3이 정면에서 오른쪽으로 회전하는 순서
        
        // 위쪽 피라미드 4개 면 (Y가 양수인 면들)
        diceNumbers[0] = new DiceNumber
        {
            number = 1,
            localPosition = new Vector3(0, radius * 0.5f, radius * 0.866f), // 정면 위쪽
            localNormal = new Vector3(0, 0.5f, 0.866f).normalized,
            numberUpDirection = Vector3.up
        };
        
        diceNumbers[1] = new DiceNumber
        {
            number = 7,
            localPosition = new Vector3(radius * 0.866f, radius * 0.5f, 0), // 오른쪽 위쪽
            localNormal = new Vector3(0.866f, 0.5f, 0).normalized,
            numberUpDirection = Vector3.up
        };
        
        diceNumbers[2] = new DiceNumber
        {
            number = 5,
            localPosition = new Vector3(0, radius * 0.5f, -radius * 0.866f), // 뒤쪽 위쪽
            localNormal = new Vector3(0, 0.5f, -0.866f).normalized,
            numberUpDirection = Vector3.up
        };
        
        diceNumbers[3] = new DiceNumber
        {
            number = 3,
            localPosition = new Vector3(-radius * 0.866f, radius * 0.5f, 0), // 왼쪽 위쪽
            localNormal = new Vector3(-0.866f, 0.5f, 0).normalized,
            numberUpDirection = Vector3.up
        };
        
        // 아래쪽 피라미드 4개 면 (Y가 음수인 면들)
        // 4-6-8-2가 대각선 위치에 있다고 가정
        diceNumbers[4] = new DiceNumber
        {
            number = 4,
            localPosition = new Vector3(0, -radius * 0.5f, radius * 0.866f), // 정면 아래쪽
            localNormal = new Vector3(0, -0.5f, 0.866f).normalized,
            numberUpDirection = Vector3.up
        };
        
        diceNumbers[5] = new DiceNumber
        {
            number = 6,
            localPosition = new Vector3(radius * 0.866f, -radius * 0.5f, 0), // 오른쪽 아래쪽
            localNormal = new Vector3(0.866f, -0.5f, 0).normalized,
            numberUpDirection = Vector3.up
        };
        
        diceNumbers[6] = new DiceNumber
        {
            number = 8,
            localPosition = new Vector3(0, -radius * 0.5f, -radius * 0.866f), // 뒤쪽 아래쪽
            localNormal = new Vector3(0, -0.5f, -0.866f).normalized,
            numberUpDirection = Vector3.up
        };
        
        diceNumbers[7] = new DiceNumber
        {
            number = 2,
            localPosition = new Vector3(-radius * 0.866f, -radius * 0.5f, 0), // 왼쪽 아래쪽
            localNormal = new Vector3(-0.866f, -0.5f, 0).normalized,
            numberUpDirection = Vector3.up
        };
        
        Debug.Log("주사위 면 배열에 따른 초기화가 완료되었습니다.");
        
        // 초기화 후 자동으로 디버깅 정보 출력
        DebugAllFacePositions();
    }
    
    // 모든 면의 위치 정보를 출력하는 디버깅 함수
    [ContextMenu("Debug All Face Positions")]
    public void DebugAllFacePositions()
    {
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("주사위 면들이 초기화되지 않았습니다. 먼저 Initialize를 실행해주세요.");
            return;
        }
        
        Debug.Log("=== 주사위 면 매핑 디버깅 정보 ===");
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            string direction = GetDirectionDescription(face.localPosition);
            
            Debug.Log($"숫자 {face.number}: " +
                     $"위치={face.localPosition}, " +
                     $"법선={face.localNormal}, " +
                     $"위방향={face.numberUpDirection}, " +
                     $"설명='{direction}'");
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
    
    // 실시간으로 가장 잘 보이는 면을 디버깅하는 함수
    [ContextMenu("Debug Current Visible Face")]
    public void DebugCurrentVisibleFace()
    {
        if (playerCamera == null)
        {
            Debug.LogError("플레이어 카메라가 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("=== 현재 보이는 면 분석 ===");
        
        float bestScore = float.MinValue;
        int bestFaceIndex = -1;
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            
            // 월드 좌표로 변환
            Vector3 worldPosition = transform.TransformPoint(face.localPosition);
            Vector3 worldNormal = transform.TransformDirection(face.localNormal);
            Vector3 worldUpDirection = transform.TransformDirection(face.numberUpDirection);
            
            // 카메라로의 방향 계산
            Vector3 toCamera = (playerCamera.transform.position - worldPosition).normalized;
            
            // 점수 계산
            float facingCamera = Vector3.Dot(worldNormal, toCamera);
            float uprightScore = Vector3.Dot(worldUpDirection, Vector3.up);
            float totalScore = facingCamera * uprightScore;
            
            // 상세 정보 출력
            Debug.Log($"면 {face.number}: " +
                     $"facing={facingCamera:F3}, " +
                     $"upright={uprightScore:F3}, " +
                     $"total={totalScore:F3}, " +
                     $"worldPos={worldPosition}, " +
                     $"worldNormal={worldNormal}");
            
            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestFaceIndex = i;
            }
        }
        
        if (bestFaceIndex >= 0)
        {
            Debug.Log($"<color=green>결과: 숫자 {diceNumbers[bestFaceIndex].number}이 가장 잘 보입니다! (점수: {bestScore:F3})</color>");
        }
        else
        {
            Debug.Log("<color=red>어떤 면도 제대로 보이지 않습니다.</color>");
        }
        
        Debug.Log("============================");
    }
    
    // 주사위가 특정 각도에 있을 때의 시뮬레이션 함수
    [ContextMenu("Simulate Dice Rolls")]
    public void SimulateDiceRolls()
    {
        if (playerCamera == null)
        {
            Debug.LogError("플레이어 카메라가 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("=== 주사위 회전 시뮬레이션 ===");
        
        // 여러 각도에서 테스트
        Vector3[] testRotations = new Vector3[]
        {
            new Vector3(0, 0, 0),      // 기본
            new Vector3(0, 90, 0),     // 오른쪽으로 90도
            new Vector3(0, 180, 0),    // 뒤쪽으로 180도
            new Vector3(0, 270, 0),    // 왼쪽으로 90도
            new Vector3(90, 0, 0),     // 위로 90도
            new Vector3(-90, 0, 0),    // 아래로 90도
            new Vector3(45, 45, 0),    // 대각선
            new Vector3(-30, 60, 30)   // 복잡한 각도
        };
        
        Transform originalTransform = transform;
        Vector3 originalRotation = transform.eulerAngles;
        
        for (int i = 0; i < testRotations.Length; i++)
        {
            // 주사위 회전
            transform.eulerAngles = testRotations[i];
            
            // 결과 확인
            int visibleNumber = GetVisibleNumber();
            
            Debug.Log($"회전값 {testRotations[i]} -> 보이는 숫자: {visibleNumber}");
        }
        
        // 원래 회전값으로 복구
        transform.eulerAngles = originalRotation;
        
        Debug.Log("============================");
    }
    
    // Scene 뷰에서 기즈모로 시각화
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || diceNumbers == null) return;
        
        // 주사위 중심 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        
        // 각 면 표시
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            
            // 월드 좌표로 변환
            Vector3 worldPos = transform.TransformPoint(face.localPosition);
            Vector3 worldNormal = transform.TransformDirection(face.localNormal);
            Vector3 worldUp = transform.TransformDirection(face.numberUpDirection);
            
            // 면의 위치 표시
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(worldPos, 0.02f);
            
            // 법선 벡터 표시 (파란색)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(worldPos, worldNormal * 0.1f);
            
            // 위 방향 표시 (초록색)
            Gizmos.color = Color.green;
            Gizmos.DrawRay(worldPos, worldUp * 0.1f);
            
            // 숫자 라벨 표시
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(worldPos + worldNormal * 0.05f, face.number.ToString());
            #endif
        }
    }
    
    public int GetVisibleNumber()
    {
        Debug.Log("GetVisibleNumber 함수 시작");
        
        if (playerCamera == null)
        {
            Debug.LogError("playerCamera가 null입니다!");
            return 1;
        }
        
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("diceNumbers 배열이 null이거나 비어있습니다!");
            return 1;
        }
        
        float bestScore = float.MinValue;
        int bestNumber = 1;
        
        Debug.Log($"총 {diceNumbers.Length}개의 주사위 면 검사 시작");
        
        foreach (var diceNumber in diceNumbers)
        {
            if (diceNumber == null)
            {
                Debug.LogWarning("null인 diceNumber 발견, 건너뜀");
                continue;
            }
            
            // 나머지 로직...
        }
        
        Debug.Log($"최종 결과: 숫자 {bestNumber} (점수: {bestScore:F3})");
        return bestNumber;
    }
    
    // Update 함수 추가: 실시간 디버깅
    private void Update()
    {
        // 키보드 단축키로 디버깅 함수 호출
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugCurrentVisibleFace();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            SimulateDiceRolls();
        }
    }
}