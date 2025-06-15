using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneLoader : MonoBehaviour
{
    public string mainGameSceneName = "MainGameScene 1";
    
    public void LoadMainGame()
    {
        Debug.Log("메인 게임으로 전환 - DontDestroyOnLoad 정리");
        
        // DontDestroyOnLoad 오브젝트들을 일반 씬 오브젝트로 되돌리기
        GameObject[] dontDestroyObjects = GetDontDestroyOnLoadObjects();
        
        foreach (GameObject obj in dontDestroyObjects)
        {
            if (obj.name.Contains("XR Origin") || obj.name.Contains("Player"))
            {
                Debug.Log($"DontDestroyOnLoad 해제: {obj.name}");
                SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
            }
        }
        
        // 씬 전환 (이제 XR Origin이 자동으로 제거됨)
        SceneManager.LoadScene(mainGameSceneName);
    }
    
    private GameObject[] GetDontDestroyOnLoadObjects()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        System.Collections.Generic.List<GameObject> dontDestroyObjects = 
            new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                dontDestroyObjects.Add(obj);
            }
        }
        
        return dontDestroyObjects.ToArray();
    }
}