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
        Debug.Log("Dice 버튼 눌림!");
        StartCoroutine(LoadDiceScene());
    }

    // private IEnumerator LoadDiceScene()
    // {
    //     var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
    //     yield return new WaitUntil(() => asyncLoad.isDone);
        
    //     Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        
    //     // 카메라 위치 찾기
    //     Camera mainCamera = Camera.main;
    //     if (mainCamera == null) mainCamera = FindObjectOfType<Camera>();
        
    //     // 카메라 위치와 방향
    //     Vector3 cameraPosition = playerTransform.position; 
    //     Vector3 cameraForward = playerTransform.forward; 
        
    //     // DiceScene 로드
    //     GameObject container = new GameObject("DiceSceneContainer");
        
    //     foreach (var rootObject in diceScene.GetRootGameObjects())
    //     {
    //         rootObject.transform.SetParent(container.transform, true);
    //     }
        
    //     // 컨테이너 위치를 플레이어 위치로 설정
    //     container.transform.position = cameraPosition;
        
    //     // 필요하다면 컨테이너의 회전도 조정 (앞쪽이 플레이어를 향하도록)
    //     // container.transform.rotation = Quaternion.LookRotation(cameraForward);
        
    //     // 거리 조정 - 플레이어 앞쪽으로 씬 배치
    //     float distanceAdjustment = 3.5f; 
    //     container.transform.position += cameraForward * distanceAdjustment;
        
    //     float heightAdjustment = 0.0f;
    //     container.transform.position += Vector3.up * heightAdjustment;
        
    //     // Debug.Log("DiceScene 로드 완료, 컨테이너 위치: " + container.transform.position);
        
    //     // Canvas
    //     GameObject canvasObj = GameObject.Find("Canvas");
    //     if (canvasObj != null)
    //     {
    //         // 백버튼 찾기
    //         Transform backButtonTransform = canvasObj.transform.Find("BackButton");
    //         if (backButtonTransform != null)
    //         {
    //             backButton = backButtonTransform.gameObject;
    //             backButton.SetActive(true);
                
    //             // 백버튼에 클릭 이벤트 추가
    //             Button backButtonComponent = backButton.GetComponent<Button>();
    //             if (backButtonComponent != null)
    //             {
    //                 backButtonComponent.onClick.RemoveAllListeners();
    //                 backButtonComponent.onClick.AddListener(OnBackButtonClicked);
    //                 // Debug.Log("백버튼 이벤트 등록됨");
    //             }
    //         }
            
    //         // Canvas 설정 확인
    //         Canvas canvasComponent = canvasObj.GetComponent<Canvas>();
    //         if (canvasComponent != null && mainCamera != null)
    //         {
    //             canvasComponent.worldCamera = mainCamera;
    //             Debug.Log("Canvas에 카메라 할당됨");
    //         }
    //     }
        
    //     // 주사위 버튼 비활성화
    //     if (diceButton != null)
    //         diceButton.gameObject.SetActive(false);
    // }
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
        
        // DiceScene의 모든 루트 오브젝트를 컨테이너로 묶기
        GameObject container = new GameObject("DiceSceneContainer");
        
        foreach (var rootObject in diceScene.GetRootGameObjects())
        {
            rootObject.transform.SetParent(container.transform, true);
        }
        
        // Plane_bottom 찾기 - Plane의 하위에서 찾아야 함
        Transform planeBottomTransform = null;
        
        // DiceScene 내에서 Plane을 먼저 찾고
        foreach (var rootObject in container.GetComponentsInChildren<Transform>())
        {
            if (rootObject.name == "Plane")
            {
                // Plane의 자식 중에서 Plane_bottom 찾기
                planeBottomTransform = rootObject.Find("Plane_bottom");
                if (planeBottomTransform != null)
                {
                    Debug.Log("Plane_bottom을 찾았습니다: " + planeBottomTransform.name);
                    break;
                }
            }
        }
        
        // Plane_bottom을 못 찾으면 다른 방법으로 시도
        if (planeBottomTransform == null)
        {
            planeBottomTransform = container.GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t.name == "Plane_bottom");
        }
        
        if (planeBottomTransform != null)
        {
            // Plane_bottom의 월드 위치 계산 (컨테이너 기준)
            Vector3 planeBottomWorldPos = planeBottomTransform.position;
            
            // Plane_bottom의 중앙이 플레이어 위치에 오도록 컨테이너 위치 조정
            // 컨테이너를 이동시켜서 Plane_bottom이 목표 위치에 오도록 함
            Vector3 offsetToApply = targetPosition - planeBottomWorldPos;
            container.transform.position += offsetToApply;
            
            Debug.Log($"Plane_bottom 위치 조정됨. 목표위치: {targetPosition}, Plane_bottom 위치: {planeBottomWorldPos}");
        }
        else
        {
            Debug.LogWarning("⚠ Plane_bottom을 찾을 수 없습니다. 기본 위치로 설정합니다.");
            // Plane_bottom을 찾지 못한 경우 기존 방식대로 처리
            container.transform.position = targetPosition;
        }
        
        // 거리 조정 - 플레이어 앞쪽으로 씬 배치
        float distanceAdjustment = 3.5f; 
        container.transform.position += cameraForward * distanceAdjustment;
        
        // 높이 조정 (필요한 경우)
        float heightAdjustment = 0.0f;
        container.transform.position += Vector3.up * heightAdjustment;
        
        Debug.Log("DiceScene 로드 완료, 최종 컨테이너 위치: " + container.transform.position);
        
        // Canvas 설정 (기존 코드 유지)
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            // 백버튼 찾기
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
                    Debug.Log("백버튼 이벤트 등록됨");
                }
            }
            
            // Canvas 설정 확인
            Canvas canvasComponent = canvasObj.GetComponent<Canvas>();
            if (canvasComponent != null && mainCamera != null)
            {
                canvasComponent.worldCamera = mainCamera;
                Debug.Log("Canvas에 카메라 할당됨");
            }
        }
        
        // 주사위 버튼 비활성화
        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        StartCoroutine(UnloadDiceScene());
    }

    // private IEnumerator UnloadDiceScene()
    // {
    //     Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        
    //     if (!diceScene.IsValid() || !diceScene.isLoaded)
    //     {
    //         Debug.LogWarning("DiceScene이 유효하지 않거나 이미 unload됨");
    //         yield break;
    //     }

    //     var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
    //     yield return new WaitUntil(() => asyncUnload.isDone);

    //     diceButton?.gameObject.SetActive(true);  
    //     backButton = null; 
    // }
    
    private IEnumerator UnloadDiceScene()
    {
        // 컨테이너 찾기 및 삭제
        GameObject container = GameObject.Find("DiceSceneContainer");
        if (container != null)
        {
            Destroy(container);
            Debug.Log("DiceSceneContainer 삭제됨");
        }

        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        
        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            Debug.LogWarning("DiceScene이 유효하지 않거나 이미 unload됨");
            // 버튼 상태는 어쨌든 복원
            diceButton?.gameObject.SetActive(true);  
            backButton = null;
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);
        Debug.Log("DiceScene 언로드 완료");

        diceButton?.gameObject.SetActive(true);  
        backButton = null; 
    }
} 