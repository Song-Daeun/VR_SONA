using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static SceneLoader Instance;

    private string currentMissionScene = "";
    private bool isMissionSceneLoaded = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ================================ //
    // 미션 씬 로딩
    // ================================ //
    public void LoadMissionScene(string sceneName)
    {
        if (isMissionSceneLoaded)
        {
            Debug.LogWarning("이미 미션 씬이 로드되어 있습니다. 기존 씬을 먼저 언로드합니다.");
            UnloadMissionScene();
        }

        Debug.Log($"미션 씬 로딩 시작: {sceneName}");
        
        currentMissionScene = sceneName;
        StartCoroutine(LoadSceneAdditive(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneAdditive(string sceneName)
    {
        // Additive 모드로 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        if (asyncLoad == null)
        {
            Debug.LogError($"씬을 찾을 수 없습니다: {sceneName}");
            yield break;
        }

        // 로딩 완료까지 대기
        while (!asyncLoad.isDone)
        {
            Debug.Log($"로딩 진행률: {asyncLoad.progress * 100:F1}%");
            yield return null;
        }

        isMissionSceneLoaded = true;
        Debug.Log($"미션 씬 로딩 완료: {sceneName}");

        // 로드된 씬을 활성 씬으로 설정 (선택사항)
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            Debug.Log($"활성 씬 변경: {sceneName}");
        }

        // 미션 씬 로드 완료 후 카메라 전환 수행
        yield return new WaitForSeconds(0.1f); // 씬 초기화 대기
        MissionCameraManager.SetupCameraForMission();
    }

    // ================================ //
    // 미션 씬 언로딩
    // ================================ //
    public void UnloadMissionScene()
    {
        if (!isMissionSceneLoaded || string.IsNullOrEmpty(currentMissionScene))
        {
            Debug.LogWarning("언로드할 미션 씬이 없습니다.");
            return;
        }

        Debug.Log($"미션 씬 언로딩 시작: {currentMissionScene}");
        StartCoroutine(UnloadSceneAsync(currentMissionScene));
    }

    private System.Collections.IEnumerator UnloadSceneAsync(string sceneName)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        
        if (asyncUnload == null)
        {
            Debug.LogError($"씬 언로드 실패: {sceneName}");
            yield break;
        }

        // 언로딩 완료까지 대기
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        // 메모리 정리
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        isMissionSceneLoaded = false;
        currentMissionScene = "";
        
        Debug.Log($"미션 씬 언로딩 완료: {sceneName}");

        // 메인 씬을 다시 활성 씬으로 설정
        Scene mainScene = SceneManager.GetSceneByBuildIndex(0); // 또는 이름으로 찾기
        if (mainScene.IsValid())
        {
            SceneManager.SetActiveScene(mainScene);
            Debug.Log($"메인 씬으로 복귀");
        }
    }

    // ================================ //
    // 상태 확인 유틸리티
    // ================================ //
    public bool IsMissionSceneLoaded()
    {
        return isMissionSceneLoaded;
    }

    public string GetCurrentMissionScene()
    {
        return currentMissionScene;
    }
}