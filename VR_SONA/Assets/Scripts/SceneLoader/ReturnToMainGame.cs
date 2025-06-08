using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainGame : MonoBehaviour
{
    public string mainSceneName = "InteractionScene";

    public void Return()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (!currentScene.name.StartsWith("Mission"))
        {
            Debug.Log("⚠️ 현재 씬이 미션 씬으로 판단되지 않음. 언로드 생략");
            return;
        }

        Debug.Log($"🔙 미션 씬 '{currentScene.name}' → '{mainSceneName}' 복귀 시작");

        Scene mainScene = SceneManager.GetSceneByName(mainSceneName);
        if (mainScene.IsValid() && mainScene.isLoaded)
        {
            SceneManager.SetActiveScene(mainScene);
            Debug.Log($"✅ 메인 씬 '{mainSceneName}' 활성화 완료");
        }
        else
        {
            Debug.LogWarning($"⚠️ 메인 씬 '{mainSceneName}' 이 로드되어 있지 않음.");
        }

        SceneManager.UnloadSceneAsync(currentScene);
    }
}
