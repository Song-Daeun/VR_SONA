using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScenePlayer : MonoBehaviour
{
    [Header("씬별 XR Origin 관리")]
    public bool destroyOnSceneChange = true;  // 씬 전환시 이 XR Origin 제거 여부
    public string[] scenesToDestroyIn = { "MainGameScene 1" };  // 제거할 씬 목록
    
    private void Start()
    {
        // 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드됨: {scene.name}");
        
        if (destroyOnSceneChange)
        {
            // 지정된 씬에서 이 XR Origin 제거
            foreach (string sceneName in scenesToDestroyIn)
            {
                if (scene.name == sceneName)
                {
                    Debug.Log($"씬 '{scene.name}'에서 StartScene XR Origin 제거");
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}