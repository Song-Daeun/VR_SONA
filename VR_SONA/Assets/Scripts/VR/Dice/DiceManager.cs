using System.Collections;
using System.Linq; 
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
        StartCoroutine(LoadDiceScene());
    }

    private IEnumerator LoadDiceScene()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);
        
        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        
        // 카메라 위치 찾기
        Camera mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindObjectOfType<Camera>();
        
        // 플레이어의 위치와 방향 정보
        Vector3 targetPosition = playerTransform.position; 
        Vector3 cameraForward = playerTransform.forward; 

        GameObject container = new GameObject("DiceSceneContainer");
        
        foreach (var rootObject in diceScene.GetRootGameObjects())
        {
            rootObject.transform.SetParent(container.transform, true);
        }
        
        // Plane_bottom
        Transform planeBottomTransform = null;
        foreach (var rootObject in container.GetComponentsInChildren<Transform>())
        {
            if (rootObject.name == "Plane")
            {
                planeBottomTransform = rootObject.Find("Plane_bottom");
                if (planeBottomTransform != null)
                {
                    break;
                }
            }
        }
        if (planeBottomTransform != null)
        {
            // Plane_bottom의 중앙으로 플레이어가 배치되도록 
            Vector3 planeBottomWorldPos = planeBottomTransform.position;
            Vector3 offsetToApply = targetPosition - planeBottomWorldPos;
            container.transform.position += offsetToApply;
            
        }
        else
        {
            container.transform.position = targetPosition;
        }
        
        // 거리 조정
        float distanceAdjustment = 3.5f; 
        container.transform.position += cameraForward * distanceAdjustment;
        
        // 높이 조정 (필요한 경우)
        // float heightAdjustment = 0.0f;
        // container.transform.position += Vector3.up * heightAdjustment;
        
        // Canvas 설정
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform backButtonTransform = canvasObj.transform.Find("BackButton");
            if (backButtonTransform != null)
            {
                backButton = backButtonTransform.gameObject;
                backButton.SetActive(true);
                
                // 백버튼에 클릭 이벤트 추가
                Button backButtonComponent = backButton.GetComponent<Button>();
                if (backButtonComponent != null)
                {
                    backButtonComponent.onClick.RemoveAllListeners();
                    backButtonComponent.onClick.AddListener(OnBackButtonClicked);
                }
            }
            
            // Canvas 설정 확인
            // Canvas canvasComponent = canvasObj.GetComponent<Canvas>();
            // if (canvasComponent != null && mainCamera != null)
            // {
            //     canvasComponent.worldCamera = mainCamera;
            //     Debug.Log("Canvas에 카메라 할당됨");
            // }
        }
        
        // 주사위 버튼 비활성화
        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        StartCoroutine(UnloadDiceScene());
    }
    
    private IEnumerator UnloadDiceScene()
    {
        // 컨테이너 찾기 및 삭제
        GameObject container = GameObject.Find("DiceSceneContainer");
        if (container != null)
        {
            Destroy(container);
        }

        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        
        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            diceButton?.gameObject.SetActive(true);  
            backButton = null;
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);

        diceButton?.gameObject.SetActive(true);  
        backButton = null; 
    }
} 