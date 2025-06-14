using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMSceneController : MonoBehaviour
{
    public AudioSource bgmMain;
    public AudioSource bgmBasketball;
    public AudioSource bgmWaterRush;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Loaded Scene: {scene.name}");

        switch (scene.name)
        {
            case "MissionWaterRushScene":
                SwitchToBGM(bgmWaterRush);
                break;

            case "MissionBasketballScene":
                SwitchToBGM(bgmBasketball);
                break;

            case "MainGameScene":
            case "InteractionScene":
                SwitchToBGM(bgmMain);
                break;
        }
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"Unloaded Scene: {scene.name}");

        if (scene.name == "MissionWaterRushScene" || scene.name == "MissionBasketballScene")
        {
            // 미션씬이 언로드되면 다시 메인 BGM으로 전환
            SwitchToBGM(bgmMain);
        }
    }

    void SwitchToBGM(AudioSource bgmToPlay)
    {
        // 모두 정지
        bgmMain.Stop();
        bgmBasketball.Stop();
        bgmWaterRush.Stop();

        // 재생할 것만 플레이
        if (bgmToPlay != null && !bgmToPlay.isPlaying)
        {
            bgmToPlay.Play();
        }
    }
}
