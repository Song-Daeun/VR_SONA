using UnityEngine;

public class MissionCameraManager : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static MissionCameraManager Instance;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ================================ //
    // 미션 진입 시 카메라 설정 (EntryPoint 기준)
    // ================================ //
    public void SetupMissionCamera()
    {
        // 미션 씬의 EntryPoint 찾기
        GameObject entryPoint = FindMissionEntryPoint();
        
        // UIScene의 XR Origin 찾기
        GameObject xrOrigin = FindUIPlayer(); // 실제로는 XR Origin을 찾음

        if (entryPoint != null && xrOrigin != null)
        {
            TransferPlayerToEntryPoint(entryPoint, xrOrigin);
        }
        else
        {
            LogError($"EntryPoint: {(entryPoint != null ? "찾음" : "null")}, XR Origin: {(xrOrigin != null ? "찾음" : "null")}");
        }
    }

    private GameObject FindMissionEntryPoint()
    {
        // 현재 활성 씬(미션 씬)에서 EntryPoint 찾기
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        // 미션 씬인지 확인
        if (activeScene.name == "MissionBasketballScene" || activeScene.name == "MissionWaterRushScene")
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                if (root.name == "EntryPoint")
                {
                    LogDebug($"EntryPoint 찾기 성공: {root.name} (씬: {activeScene.name})");
                    return root;
                }
                
                // 자식에서 EntryPoint 찾기
                GameObject childEntryPoint = FindInChildren(root, "EntryPoint");
                if (childEntryPoint != null)
                {
                    LogDebug($"EntryPoint 찾기 성공 (자식): {childEntryPoint.name} (씬: {activeScene.name})");
                    return childEntryPoint;
                }
            }
        }
        
        LogError($"활성 씬 '{activeScene.name}'에서 EntryPoint를 찾을 수 없습니다!");
        return null;
    }

    private GameObject FindUIPlayer()
    {
        // UIScene에서 XR Origin (XR Rig) 직접 찾기
        // UnityEngine.SceneManagement.Scene uiScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("UIScene");
        UnityEngine.SceneManagement.Scene uiScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainGameScene");
        
        if (uiScene.isLoaded)
        {
            GameObject[] rootObjects = uiScene.GetRootGameObjects();

            // 모든 오브젝트를 순회하면서 XR Origin 찾기
            foreach (GameObject root in rootObjects)
            {
                // 직접 검색
                GameObject xrOrigin = SearchForXROrigin(root);
                if (xrOrigin != null)
                {
                    LogDebug($"XR Origin 찾기 성공: {xrOrigin.name} (씬: {uiScene.name})");
                    LogDebug($"XR Origin 경로: {GetGameObjectPath(xrOrigin)}");
                    return xrOrigin;
                }
            }
        }
        
        LogError("UIScene에서 XR Origin을 찾을 수 없습니다!");
        return null;
    }
    
    private GameObject SearchForXROrigin(GameObject obj)
    {
        // XR Origin 이름 패턴 체크
        if (obj.name.Contains("XR Origin") || obj.name.Contains("XROrigin"))
        {
            return obj;
        }
        
        // 자식들에서 재귀적으로 검색
        foreach (Transform child in obj.transform)
        {
            GameObject result = SearchForXROrigin(child.gameObject);
            if (result != null)
                return result;
        }
        
        return null;
    }

    private GameObject FindInChildren(GameObject parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent.transform)
        {
            GameObject result = FindInChildren(child.gameObject, name);
            if (result != null)
                return result;
        }
        return null;
    }

    // ================================ //
    // Player를 EntryPoint로 이동 및 방향 설정
    // ================================ //
    private void TransferPlayerToEntryPoint(GameObject entryPoint, GameObject xrOrigin)
    {
        // 0. 디버그 정보 출력
        LogDebug($"=== XR Origin 이동 시작 ===");
        LogDebug($"EntryPoint 위치: {entryPoint.transform.position}");
        LogDebug($"XR Origin 이동 전 위치: {xrOrigin.transform.position}");
        LogDebug($"XR Origin 오브젝트: {xrOrigin.name}");

        // 1. EntryPoint 위치로 XR Origin 이동 (높이 오프셋 추가)
        Vector3 entryPosition = entryPoint.transform.position;
        
        // 높이 오프셋 추가 (Player가 바닥에 묻히지 않도록)
        Vector3 finalPosition = entryPosition + Vector3.up * 1.0f;
        xrOrigin.transform.position = finalPosition;

        LogDebug($"XR Origin 위치를 EntryPoint로 설정: {finalPosition} (원본: {entryPosition})");
        
        // 1.5. 이동 직후 위치 확인
        LogDebug($"XR Origin 이동 직후 위치: {xrOrigin.transform.position}");

        // 2. 미션별 방향 설정
        Quaternion targetRotation = GetMissionSpecificRotation();
        xrOrigin.transform.rotation = targetRotation;

        LogDebug($"XR Origin 회전 설정: {targetRotation.eulerAngles}");

        // 3. 미션 씬의 MainCamera 비활성화 (있다면)
        DeactivateMissionCamera();

        // 4. XR Origin 활성화 확인
        if (!xrOrigin.activeInHierarchy)
        {
            xrOrigin.SetActive(true);
            LogDebug("XR Origin 활성화 완료");
        }

        // 5. XR Origin 내부의 카메라 컴포넌트 확인 및 활성화
        ActivatePlayerCamera(xrOrigin);
        
        // 6. 최종 위치 확인 (약간의 지연 후)
        StartCoroutine(CheckPlayerPositionAfterDelay(xrOrigin, 0.1f));
    }

    private Quaternion GetMissionSpecificRotation()
    {
        // 현재 활성 씬 확인
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        switch (activeScene.name)
        {
            case "MissionBasketballScene":
                // Z 기준 -방향 (180도 회전)
                LogDebug("Basketball 씬: Z축 -방향 설정");
                return Quaternion.Euler(0, 180, 0);
                
            case "MissionWaterRushScene":
                // X 기준 +방향 (90도 회전)
                LogDebug("WaterRush 씬: X축 +방향 설정");
                return Quaternion.Euler(0, 90, 0);
                
            default:
                LogWarning($"알 수 없는 미션 씬: {activeScene.name}, 기본 방향 사용");
                return Quaternion.identity; // 기본 방향 (Z축 +방향)
        }
    }

    private void DeactivateMissionCamera()
    {
        // 미션 씬의 MainCamera 찾아서 비활성화
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        if (activeScene.name == "MissionBasketballScene" || activeScene.name == "MissionWaterRushScene")
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                if (root.name == "MainCamera")
                {
                    root.SetActive(false);
                    LogDebug($"미션 씬 MainCamera 비활성화: {root.name}");
                    return;
                }
            }
        }
        
        LogDebug("미션 씬에서 MainCamera를 찾지 못했습니다. (정상일 수 있음)");
    }

    private void ActivatePlayerCamera(GameObject xrOrigin)
    {
        // XR Origin 내부의 카메라 찾기 및 활성화 (더 깊이 검색)
        Camera[] cameras = xrOrigin.GetComponentsInChildren<Camera>(true);
        
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(true);
            LogDebug($"XR Origin 카메라 활성화: {cam.name} (경로: {GetGameObjectPath(cam.gameObject)})");
        }

        if (cameras.Length == 0)
        {
            LogWarning("XR Origin에서 카메라 컴포넌트를 찾을 수 없습니다!");
        }
        else
        {
            LogDebug($"총 {cameras.Length}개의 카메라를 활성화했습니다.");
        }
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    // ================================ //
    // 디버그용 코루틴
    // ================================ //
    private System.Collections.IEnumerator CheckPlayerPositionAfterDelay(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);
        LogDebug($"🔍 {delay}초 후 Player 최종 위치: {player.transform.position}");
        LogDebug($"🔍 Player 활성 상태: {player.activeInHierarchy}");
        
        // 물리 컴포넌트 확인
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            LogDebug($"🔍 Rigidbody 상태 - IsKinematic: {rb.isKinematic}, UseGravity: {rb.useGravity}");
        }
        
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            LogDebug($"🔍 CharacterController 발견 - Enabled: {cc.enabled}");
        }
    }

    // ================================ //
    // 공개 메소드 (다른 스크립트에서 호출)
    // ================================ //
    public static void SetupCameraForMission()
    {
        if (Instance != null)
        {
            Instance.SetupMissionCamera();
        }
        else
        {
            Debug.LogError("MissionCameraManager.Instance가 null입니다!");
        }
    }

    // ================================ //
    // 디버그 로깅
    // ================================ //
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MissionCameraManager] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogs)
        {
            Debug.LogWarning($"[MissionCameraManager] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[MissionCameraManager] {message}");
    }
}