using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneLoader : MonoBehaviour
{
    public string mainGameSceneName = "MainGameScene";
    
    public void LoadMainGame()
    {
        GameObject[] dontDestroyObjects = GetDontDestroyOnLoadObjects();
        
        foreach (GameObject obj in dontDestroyObjects)
        {
            if (obj.name.Contains("XR Origin") || obj.name.Contains("Player"))
            {
                SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
            }
        }
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