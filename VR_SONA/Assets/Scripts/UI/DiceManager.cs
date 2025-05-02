using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DiceManager : MonoBehaviour
{
    public Transform playerTransform;
    public  GameObject backButton;
    public Button diceButton;
    
    private void Start()
    {
        // dicebutton 자동 연결
        if (diceButton == null)
        {
            GameObject found = GameObject.Find("DiceButton");
            if (found != null)
                diceButton = found.GetComponent<Button>();
        }

        if (diceButton != null)
        {
            diceButton.onClick.RemoveAllListeners();
            diceButton.onClick.AddListener(DiceButton_clicked);
        }
        else
        {
            Debug.LogWarning("⚠ DiceButton 못 찾음");
        }

        // backButton 비활성화
        if (backButton != null)
            backButton.SetActive(false);
    }
    
    public void DiceButton_clicked()
    {
        StartCoroutine(LoadDiceScene());
    }

    private IEnumerator LoadDiceScene()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        foreach (var obj in diceScene.GetRootGameObjects())
        {
            // 플레이어 위치에 DiceScene 로드
            if (obj.name == "Plane")
            {
                obj.transform.position = playerTransform.position;
            }

            // BackButton 자동 연결
            if (obj.name == "Canvas")
            {
                var found = obj.transform.Find("BackButton");
                if (found != null)
                {
                    backButton = found.gameObject;
                    backButton.SetActive(true);
                }
            }
        }

        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        StartCoroutine(UnloadDiceScene());
    }

    private IEnumerator UnloadDiceScene()
    {
        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        
        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            Debug.LogWarning("DiceScene이 유효하지 않거나 이미 unload됨");
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);

        diceButton?.gameObject.SetActive(true);  
        //backButton?.SetActive(false);
        backButton = null; 
    }
}
