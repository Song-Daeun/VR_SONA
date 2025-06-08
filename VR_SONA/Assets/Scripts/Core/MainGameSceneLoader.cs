// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MainGameSceneLoader : MonoBehaviour
// {
//     public string startSceneName = "StartScene";
//     public string sceneToLoad = "UIScene"; // 나중에 MainGameScene으로 변경

//     public void LoadGameScene()
//     {
//         // Additively load the new scene
//         SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);

//         // 코루틴으로 처리
//         StartCoroutine(UnloadStartSceneWhenLoaded());
//     }

//     private System.Collections.IEnumerator UnloadStartSceneWhenLoaded()
//     {
//         // 씬이 완전히 로드될 때까지 기다림
//         yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad).isLoaded);

//         // 새 씬을 활성화 (optional, 필요 시)
//         SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));

//         // StartScene 언로드
//         SceneManager.UnloadSceneAsync(startSceneName);
//     }
// }


using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameSceneLoader : MonoBehaviour
{
    public string sceneToLoad = "UIScene"; // 나중에 MainGameScene으로 변경

    public void LoadGameScene()
    {
        // 단순 씬 전환 - 현재 씬을 언로드하고 새 씬으로 전환
        Debug.Log("씬 전환 전 Quality Level: " + QualitySettings.GetQualityLevel());
        SceneManager.LoadScene(sceneToLoad);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("씬 전환 후 Quality Level: " + QualitySettings.GetQualityLevel());
        Debug.Log("현재 씬: " + scene.name);
    }

    // 비동기 버전 (선택사항 - 로딩 화면 등이 필요한 경우)
    public void LoadGameSceneAsync()
    {
        StartCoroutine(LoadSceneAsyncCoroutine());
    }

    private System.Collections.IEnumerator LoadSceneAsyncCoroutine()
    {
        // 비동기로 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // 로딩이 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            // 여기서 로딩 진행률을 UI에 표시할 수 있음
            // float progress = asyncLoad.progress;
            yield return null;
        }
    }
}