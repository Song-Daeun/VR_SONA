using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class DiceManager : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject backButton;
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
        Debug.Log("Dice 버튼 눌림!");
        StartCoroutine(LoadDiceScene());
    }
    
    private IEnumerator LoadDiceScene()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        foreach (var obj in diceScene.GetRootGameObjects())
        {
            // 플레이어 위치에 DiceScene의 모든 요소를 이동
            if (obj.name == "Plane")
            {
                // Plane을 플레이어 위치로 이동
                obj.transform.position = playerTransform.position;
                obj.transform.rotation = Quaternion.LookRotation(playerTransform.forward);
            }

            // Canvas도 Plane과 함께 적절한 위치로 이동
            if (obj.name == "Canvas")
            {
                // Canvas를 플레이어 앞 적절한 위치로 배치
                Vector3 playerForward = playerTransform.forward;
                float canvasDistance = 5.0f; // 플레이어로부터의 거리
                
                obj.transform.position = playerTransform.position + playerForward * canvasDistance;
                
                // Canvas가 플레이어를 바라보도록 설정
                obj.transform.LookAt(playerTransform);
                obj.transform.Rotate(0, 180, 0);

                var found = obj.transform.Find("BackButton");
                if (found != null)
                {
                    backButton = found.gameObject;
                    backButton.SetActive(true);
                }
                
                // 가장 중요한 부분: Event Camera 자동 할당
                // Canvas canvas = obj.GetComponent<Canvas>();
                // if (canvas != null)
                // {
                //     // 현재 메인 카메라를 찾아서 할당
                //     Camera mainCamera = Camera.main;
                //     if (mainCamera != null)
                //     {
                //         canvas.worldCamera = mainCamera;
                //         Debug.Log("Canvas에 Event Camera가 자동으로 할당되었습니다.");
                //     }
                // }
            } // 이 중괄호가 "Canvas" if 블록을 닫습니다
        } // 이 중괄호가 foreach 블록을 닫습니다

        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    } // 이 중괄호가 LoadDiceScene 메서드를 닫습니다
    
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
        backButton = null; 
    }
} // 이 중괄호가 DiceManager 클래스를 닫습니다