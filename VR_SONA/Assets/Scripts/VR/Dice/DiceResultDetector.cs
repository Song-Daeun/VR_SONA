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
    
    [Header("주사위 숫자 설정")]
    public DiceNumber[] diceNumbers = new DiceNumber[8];
    
    [Header("카메라 참조")]
    public Camera playerCamera; // 플레이어의 시점 카메라
    
    [Header("디버깅 설정")]
    public bool showDebugLogs = true;    // 디버그 로그 표시 여부
    public bool drawDebugGizmos = true;  // Scene 뷰에서 기즈모 표시 여부
    
    private void Start()
    {
        // 주사위 면 오브젝트 자동 연결
        AutoConnectFaceObjects();
        
        // 오브젝트 기반으로 면 데이터 설정
        SetupDiceFacesFromObjects();
        
        // 카메라 자동 찾기
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
    }

    // 주사위 면 숫자 매핑
    [ContextMenu("Auto Connect Face Objects")]
    private void AutoConnectFaceObjects()
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
        
        for (int i = 1; i <= 8; i++)
        {
            string planeName = $"Plane_{i}";
            Transform planeTransform = transform.Find(planeName);
            
            if (planeTransform != null)
            {
                int arrayIndex = i - 1; // 배열은 0부터 시작
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
    
    // 면 데이터 설정
    [ContextMenu("Setup Dice Faces From Objects")]
    private void SetupDiceFacesFromObjects()
    { 
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            if (diceNumbers[i].faceObject != null)
            {
                // 오브젝트의 로컬 위치를 주사위 기준으로 계산
                diceNumbers[i].localPosition = transform.InverseTransformPoint(diceNumbers[i].faceObject.position);
                
                // 오브젝트의 법선 방향 계산 (y축 방향이 법선)
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
    
    // 바닥에 닿아있는 면의 숫자를 반환
    public int GetVisibleNumber()
    {
        if (diceNumbers == null || diceNumbers.Length == 0)
        {
            Debug.LogError("No dice face information!");
            return -1;
        }
        
        return GetBottomFacingNumber();
    }
    
    // 가장 낮은 위치에 있는 면 찾기
    private int GetBottomFacingNumber()
    {
        float lowestY = float.MaxValue;
        int bottomNumber = 1;
        bool hasValidFace = false;
        
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            DiceNumber face = diceNumbers[i];
            
            if (face.faceObject != null)
            {
                hasValidFace = true;
                Vector3 worldPosition = face.faceObject.position;
                
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
            return 1;
        }
        
        // Debug.Log($"<color=green>바닥에 닿은 면: {bottomNumber} (Y값: {lowestY:F3})</color>");
        return bottomNumber;
    }
    
    // 디버깅 함수들
    // [ContextMenu("Test Current Dice Result")]
    // public void TestCurrentDiceResult()
    // {
    //     Debug.Log("=== 현재 주사위 결과 테스트 ===");
        
    //     int result = GetVisibleNumber();
        
    //     Debug.Log($"<color=yellow>주사위 결과: {result}</color>");
    //     Debug.Log("==========================================");
    // }
    
    // [ContextMenu("Debug All Face Positions")]
    // public void DebugAllFacePositions()
    // {
    //     if (diceNumbers == null || diceNumbers.Length == 0)
    //     {
    //         Debug.LogError("주사위 면들이 초기화되지 않았습니다.");
    //         return;
    //     }
        
    //     Debug.Log("=== 주사위 면 매핑 디버깅 정보 ===");
    //     Debug.Log($"주사위 현재 위치: {transform.position}");
    //     Debug.Log($"주사위 현재 회전: {transform.eulerAngles}");
        
    //     for (int i = 0; i < diceNumbers.Length; i++)
    //     {
    //         var face = diceNumbers[i];
    //         if (face.faceObject != null)
    //         {
    //             Debug.Log($"숫자 {face.number}: " +
    //                      $"오브젝트={face.faceObject.name}, " +
    //                      $"월드위치={face.faceObject.position}, " +
    //                      $"Y값={face.faceObject.position.y:F3}");
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"숫자 {face.number}: 오브젝트가 연결되지 않았습니다!");
    //         }
    //     }
        
    //     Debug.Log("================================");
    // }
    
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
            
            if (face.faceObject != null)
            {
                Vector3 worldPos = face.faceObject.position;
                
                // 가장 낮은 면 찾기
                if (worldPos.y < lowestY)
                {
                    lowestY = worldPos.y;
                    lowestFaceIndex = i;
                }
                
                // 면의 위치 표시
                Gizmos.color = (i == lowestFaceIndex) ? Color.red : Color.green;
                Gizmos.DrawSphere(worldPos, 0.03f);
                
                // 면의 법선 벡터 표시
                Gizmos.color = Color.blue;
                Vector3 normal = face.faceObject.TransformDirection(Vector3.back);
                Gizmos.DrawRay(worldPos, normal * 0.1f);
                
                #if UNITY_EDITOR
                // 숫자 라벨 표시 
                UnityEditor.Handles.color = (i == lowestFaceIndex) ? Color.red : Color.green;
                UnityEditor.Handles.Label(worldPos + normal * 0.05f, face.number.ToString());
                #endif
            }
        }
        
        // 바닥 방향 화살표 표시
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
    }
    
    // Update 함수 - 테스트용 키보드 입력
    // private void Update()
    // {
    //     if (Keyboard.current != null)
    //     {
    //         if (Keyboard.current.dKey.wasPressedThisFrame)
    //         {
    //             TestCurrentDiceResult();
    //         }
            
    //         if (Keyboard.current.vKey.wasPressedThisFrame)
    //         {
    //             DebugVRCameraInfo();
    //         }
    //     }
    // }
    
    // // VR 카메라 디버깅
    // private void DebugVRCameraInfo()
    // {
    //     if (playerCamera == null) return;
        
    //     Debug.Log("=== VR 카메라 디버깅 정보 ===");
    //     Debug.Log($"카메라 이름: {playerCamera.name}");
    //     Debug.Log($"카메라 태그: {playerCamera.tag}");
    //     Debug.Log($"카메라 레이어 마스크: {playerCamera.cullingMask}");
        
    //     Transform cameraRig = playerCamera.transform.parent;
    //     if (cameraRig != null)
    //     {
    //         Debug.Log($"카메라 부모: {cameraRig.name}");
    //         Debug.Log($"부모 위치: {cameraRig.position}");
            
    //         Transform origin = cameraRig.parent;
    //         if (origin != null && origin.name.Contains("XR Origin"))
    //         {
    //             Debug.Log($"XR Origin 위치: {origin.position}");
    //         }
    //     }
    // }
}