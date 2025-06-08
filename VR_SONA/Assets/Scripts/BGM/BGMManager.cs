using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public AudioSource BGMPlayer;    // InteractionScene BGM
    public AudioSource BGM1Player;   // MissionBasketballScene BGM
    public AudioSource BGM2Player;   // MissionWaterRushScene BGM

    public enum BGMType
    {
        None,
        Interaction,
        Basketball,
        WaterRush
    }

    public float fadeDuration = 1.5f;
    private AudioSource currentBGM;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // 기본 시작 시 Interaction BGM
        ActivateBGM(BGMType.Interaction);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MissionBasketballScene")
        {
            ActivateBGM(BGMType.Basketball);
        }
        else if (scene.name == "MissionWaterRushScene")
        {
            ActivateBGM(BGMType.WaterRush);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // 미션 씬이 언로드되면 Interaction BGM 복귀
        if (scene.name == "MissionBasketballScene" || scene.name == "MissionWaterRushScene")
        {
            ActivateBGM(BGMType.Interaction);
        }
    }

    public void ActivateBGM(BGMType newBGM)
    {
        AudioSource newAudioSource = GetAudioSource(newBGM);

        if (currentBGM == newAudioSource)
        {
            return; // 이미 같은 BGM이면 무시
        }

        StartCoroutine(SwitchBGM(newAudioSource));
    }

    private AudioSource GetAudioSource(BGMType type)
    {
        switch (type)
        {
            case BGMType.Interaction: return BGMPlayer;
            case BGMType.Basketball: return BGM1Player;
            case BGMType.WaterRush: return BGM2Player;
            default: return null;
        }
    }

    private IEnumerator SwitchBGM(AudioSource newBGM)
    {
        // 이전 BGM 페이드아웃
        if (currentBGM != null)
        {
            yield return StartCoroutine(FadeOut(currentBGM));
            currentBGM.Stop();
            currentBGM.gameObject.SetActive(false);
        }

        // 새로운 BGM 페이드인
        if (newBGM != null)
        {
            newBGM.gameObject.SetActive(true);
            newBGM.volume = 0f;
            newBGM.Play();

            yield return StartCoroutine(FadeIn(newBGM));
        }

        currentBGM = newBGM;
    }

    private IEnumerator FadeOut(AudioSource audioSource)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
    }

    private IEnumerator FadeIn(AudioSource audioSource)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 1f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1f;
    }
}
