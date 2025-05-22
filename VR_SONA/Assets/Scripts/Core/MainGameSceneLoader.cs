using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameSceneLoader : MonoBehaviour
{
    public string startSceneName = "StartScene";
    public string sceneToLoad = "UIScene"; // 나중에 MainGameScene으로 변경

    public void LoadGameScene()
    {
        // Additively load the new scene
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);

        // 코루틴으로 처리
        StartCoroutine(UnloadStartSceneWhenLoaded());
    }

    private System.Collections.IEnumerator UnloadStartSceneWhenLoaded()
    {
        // 씬이 완전히 로드될 때까지 기다림
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad).isLoaded);

        // 새 씬을 활성화 (optional, 필요 시)
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));

        // StartScene 언로드
        SceneManager.UnloadSceneAsync(startSceneName);
    }
}
