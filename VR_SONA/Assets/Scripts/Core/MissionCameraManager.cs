using UnityEngine;

public class MissionCameraManager : MonoBehaviour
{
    // Singleton & References
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

    // 미션 진입 시 카메라 설정
    public void SetupMissionCamera()
    {
        // 미션 씬의 EntryPoint 찾기
        GameObject entryPoint = FindMissionEntryPoint();
        GameObject xrOrigin = FindUIPlayer();

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
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name == "MissionBasketballScene" || activeScene.name == "MissionWaterRushScene")
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                if (root.name == "EntryPoint")
                {
                    return root;
                }
                
                GameObject childEntryPoint = FindInChildren(root, "EntryPoint");
                if (childEntryPoint != null)
                {
                    return childEntryPoint;
                }
            }
        }
        return null;
    }

    private GameObject FindUIPlayer()
    {
        
        UnityEngine.SceneManagement.Scene uiScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainGameScene");
        
        if (uiScene.isLoaded)
        {
            GameObject[] rootObjects = uiScene.GetRootGameObjects();

            foreach (GameObject root in rootObjects)
            {
                // 직접 검색
                GameObject xrOrigin = SearchForXROrigin(root);
                if (xrOrigin != null)
                {
                    return xrOrigin;
                }
            }
        }
        return null;
    }
    
    private GameObject SearchForXROrigin(GameObject obj)
    {
        if (obj.name.Contains("XR Origin") || obj.name.Contains("XROrigin"))
        {
            return obj;
        }

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

    // Player를 EntryPoint로 이동 및 방향 설정
    private void TransferPlayerToEntryPoint(GameObject entryPoint, GameObject xrOrigin)
    {
        Vector3 entryPosition = entryPoint.transform.position;
        Vector3 finalPosition = entryPosition + Vector3.up * 1.0f;
        xrOrigin.transform.position = finalPosition;

        Quaternion targetRotation = GetMissionSpecificRotation();
        xrOrigin.transform.rotation = targetRotation;

        //DeactivateMissionCamera();

        if (!xrOrigin.activeInHierarchy)
        {
            xrOrigin.SetActive(true);
        }
        ActivatePlayerCamera(xrOrigin);
        // StartCoroutine(CheckPlayerPositionAfterDelay(xrOrigin, 0.1f));
    }

    private Quaternion GetMissionSpecificRotation()
    {
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        switch (activeScene.name)
        {
            case "MissionBasketballScene":
                return Quaternion.Euler(0, 180, 0);
                
            case "MissionWaterRushScene":
                return Quaternion.Euler(0, 90, 0);
                
            default:
                return Quaternion.identity; // 기본 방향 (Z축 +방향)
        }
    }

    // private void DeactivateMissionCamera()
    // {
    //     // 미션 씬의 MainCamera 찾아서 비활성화
    //     UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
    //     if (activeScene.name == "MissionBasketballScene" || activeScene.name == "MissionWaterRushScene")
    //     {
    //         GameObject[] rootObjects = activeScene.GetRootGameObjects();
    //         foreach (GameObject root in rootObjects)
    //         {
    //             if (root.name == "MainCamera")
    //             {
    //                 root.SetActive(false);
    //                 LogDebug($"미션 씬 MainCamera 비활성화: {root.name}");
    //                 return;
    //             }
    //         }
    //     }
        
    //     LogDebug("미션 씬에서 MainCamera를 찾지 못했습니다. (정상일 수 있음)");
    // }

    private void ActivatePlayerCamera(GameObject xrOrigin)
    {
        Camera[] cameras = xrOrigin.GetComponentsInChildren<Camera>(true);
        
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(true);
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

    // // ================================ //
    // // 디버그용 코루틴
    // // ================================ //
    // private System.Collections.IEnumerator CheckPlayerPositionAfterDelay(GameObject player, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     LogDebug($"🔍 {delay}초 후 Player 최종 위치: {player.transform.position}");
    //     LogDebug($"🔍 Player 활성 상태: {player.activeInHierarchy}");
        
    //     // 물리 컴포넌트 확인
    //     Rigidbody rb = player.GetComponent<Rigidbody>();
    //     if (rb != null)
    //     {
    //         LogDebug($"🔍 Rigidbody 상태 - IsKinematic: {rb.isKinematic}, UseGravity: {rb.useGravity}");
    //     }
        
    //     CharacterController cc = player.GetComponent<CharacterController>();
    //     if (cc != null)
    //     {
    //         LogDebug($"🔍 CharacterController 발견 - Enabled: {cc.enabled}");
    //     }
    // }

    // 공개 메소드 
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