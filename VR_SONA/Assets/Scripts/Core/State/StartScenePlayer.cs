using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScenePlayer : MonoBehaviour
{
    [Header("씬별 XR Origin 관리")]
    public bool destroyOnSceneChange = true;  // 씬 전환시 이 XR Origin 제거 여부
    public string[] scenesToDestroyIn = { "MainGameScene" };
    
    private void Start()
    {
        // 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {        
        if (destroyOnSceneChange)
        {
            foreach (string sceneName in scenesToDestroyIn)
            {
                if (scene.name == sceneName)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}