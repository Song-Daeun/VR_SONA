using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class GameEndManager : MonoBehaviour
{
    // Singleton & References
    public static GameEndManager Instance;

    [Header("Game End UI Panels")]
    public GameObject gameEndCanvas;           
    public GameObject coinLackPanel;       
    public GameObject timeUpPanel;             
    public GameObject successPanel;            

    [Header("UI Components")]
    public TextMeshProUGUI coinLackTitle;      
    public TextMeshProUGUI coinLackMessage;    
    public TextMeshProUGUI timeUpTitle;      
    public TextMeshProUGUI timeUpMessage;      
    public TextMeshProUGUI successTitle;      
    public TextMeshProUGUI successMessage;     

    [Header("Coin Lack Panel Buttons")]
    public Button coinLackRestartButton;     
    public Button coinLackExitButton;          

    [Header("Time Up Panel Buttons")]
    public Button timeUpRestartButton;     
    public Button timeUpExitButton;          

    [Header("Success Panel Buttons")]
    public Button successRestartButton;       
    public Button successExitButton;      

    [Header("Settings")]
    public float panelDisplayTime = 5f;       

    // 게임 종료 상태 추적
    private bool isGameEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeGameEndUI();
        SetupButtonEvents();
    }

    // UI 초기화
    private void InitializeGameEndUI()
    {
        // 모든 패널 비활성화
        if (gameEndCanvas != null)
            gameEndCanvas.SetActive(false);
        
        if (coinLackPanel != null)
            coinLackPanel.SetActive(false);
            
        if (timeUpPanel != null)
            timeUpPanel.SetActive(false);
            
        if (successPanel != null)
            successPanel.SetActive(false);

        Debug.Log("GameEndManager UI 초기화 완료");
    }

    private void SetupButtonEvents()
    {
        if (coinLackRestartButton != null)
        {
            coinLackRestartButton.onClick.AddListener(RestartGame);
        }
        if (coinLackExitButton != null)
        {
            coinLackExitButton.onClick.AddListener(ExitGame);
        }

        if (timeUpRestartButton != null)
        {
            timeUpRestartButton.onClick.AddListener(RestartGame);
        }
        if (timeUpExitButton != null)
        {
            timeUpExitButton.onClick.AddListener(ExitGame);
        }

        if (successRestartButton != null)
        {
            successRestartButton.onClick.AddListener(RestartGame);
        }
        if (successExitButton != null)
        {
            successExitButton.onClick.AddListener(ExitGame);
        }
    }

    // 게임 종료 시 모든 UI 숨기기
    private void HideAllGameplayUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(false);
            UIManager.Instance.ShowMissionPrompt(false);
            UIManager.Instance.ShowSpellBookUI(false);
        }
        else
        {
            Debug.LogWarning("UIManager.Instance를 찾을 수 없어 UI 숨김 처리 실패");
        }
    }

    // 코인 부족으로 인한 게임 종료
    public void EndGameDueToCoinLack()
    {
        if (isGameEnded || PlayerState.IsGameEnded()) return;

        PlayerState.SetGameFailedCoinLack();
        
        HideAllGameplayUI();
        
        isGameEnded = true;
        Time.timeScale = 0f;


        // UI 텍스트 설정
        if (coinLackTitle != null)
            coinLackTitle.text = "게임 종료";
            
        if (coinLackMessage != null)
        {
            int currentCoins = GameManager.Instance.GetCurrentCoins();
            int missionCost = PlayerState.MissionCost;
            coinLackMessage.text = $"코인이 부족합니다!\n현재 코인: {currentCoins}\n필요 코인: {missionCost}";
        }

        ShowGameEndPanel(coinLackPanel);
    }

    // 시간 만료로 인한 게임 종료
    public void EndGameDueToTimeUp()
    {
        if (isGameEnded || PlayerState.IsGameEnded()) return;

        PlayerState.SetGameFailedTimeUp();
        
        HideAllGameplayUI();
        
        isGameEnded = true;
        Time.timeScale = 0f; 

        Debug.Log("게임 종료: 시간 만료");

        // 빙고 완성 여부 확인
        bool hasAchievedBingo = CheckForBingoCompletion();

        if (timeUpTitle != null)
        {
            timeUpTitle.text = hasAchievedBingo ? "시간 만료 - 부분 성공!" : "시간 만료 - 실패";
        }
            
        if (timeUpMessage != null)
        {
            if (hasAchievedBingo)
            {
                timeUpMessage.text = "시간은 부족했지만\n빙고를 달성했습니다!";
            }
            else
            {
                timeUpMessage.text = "시간 내에 빙고를\n완성하지 못했습니다.";
            }
        }

        ShowGameEndPanel(timeUpPanel);
    }

    // 게임 성공
    public void EndGameDueToSuccess()
    {
        if (isGameEnded || PlayerState.IsGameEnded()) return;

        PlayerState.SetGameSuccess();
        
        HideAllGameplayUI();
        
        isGameEnded = true;
        Time.timeScale = 0f; 

        // UI 텍스트 설정
        if (successTitle != null)
            successTitle.text = "축하합니다!";
            
        if (successMessage != null)
            successMessage.text = "빙고 2줄 이상을 완성하여\n성공하셨습니다!";

        ShowGameEndPanel(successPanel);
    }

    // 공통 패널 표시
    private void ShowGameEndPanel(GameObject panel)
    {
        if (gameEndCanvas != null)
        {
            gameEndCanvas.SetActive(true);
            
            // VR용 Canvas 설정
            Canvas canvas = gameEndCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = FindCameraComponent();
                canvas.sortingOrder = 1000; 
                
                // 스케일 설정
                RectTransform canvasRect = gameEndCanvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.localScale = Vector3.one * 0.01f;
                }
            }
            
            // 카메라 앞에 위치 설정
            PositionUIInFrontOfCamera();
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        // 일정 시간 후 자동 재시작
        StartCoroutine(AutoRestartAfterDelay());
    }

    // 카메라 찾기 및 위치 설정 메서드들
    private Camera FindCameraComponent()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null) return mainCamera;
        
        return FindObjectOfType<Camera>();
    }

    private void PositionUIInFrontOfCamera()
    {
        Camera camera = FindCameraComponent();
        if (camera == null || gameEndCanvas == null) return;

        float distance = 2f;
        Vector3 targetPos = camera.transform.position + camera.transform.forward * distance;
        targetPos.y = camera.transform.position.y; // 카메라와 같은 높이
        
        gameEndCanvas.transform.position = targetPos;
        gameEndCanvas.transform.rotation = Quaternion.LookRotation(targetPos - camera.transform.position);
    }

    private bool CheckForBingoCompletion()
    {
        if (BingoBoard.Instance == null)
        {
            return false;
        }

        int totalCompletedLines = 0;

        for (int row = 0; row < 3; row++)
        {
            if (IsHorizontalLineCompleted(row))
            {
                totalCompletedLines++;
            }
        }

        for (int col = 0; col < 3; col++)
        {
            if (IsVerticalLineCompleted(col))
            {
                totalCompletedLines++;
            }
        }

        if (IsDiagonalLineCompleted(true))
        {
            totalCompletedLines++;
        }

        if (IsDiagonalLineCompleted(false))
        {
            totalCompletedLines++;
        }

        Debug.Log($"총 완성된 빙고 줄 수: {totalCompletedLines}/8");

        return totalCompletedLines >= 2;
    }

    private bool IsHorizontalLineCompleted(int row)
    {
        for (int col = 0; col < 3; col++)
        {
            if (!IsTilePositionCompleted(row, col))
                return false;
        }
        return true;
    }

    private bool IsVerticalLineCompleted(int col)
    {
        for (int row = 0; row < 3; row++)
        {
            if (!IsTilePositionCompleted(row, col))
                return false;
        }
        return true;
    }

    private bool IsDiagonalLineCompleted(bool isMainDiagonal)
    {
        if (isMainDiagonal)
        {
            return IsTilePositionCompleted(0, 0) && 
                   IsTilePositionCompleted(1, 1) && 
                   IsTilePositionCompleted(2, 2);
        }
        else
        {
            return IsTilePositionCompleted(0, 2) && 
                   IsTilePositionCompleted(1, 1) && 
                   IsTilePositionCompleted(2, 0);
        }
    }

    private bool IsTilePositionCompleted(int x, int y)
    {
        if (x == 2 && y == 2) 
        {
            return true; // Start 타일은 항상 완성
        }
        
        bool isCompleted = BingoBoard.Instance != null && 
                          BingoBoard.Instance.IsTileMissionCleared(x, y);
        
        return isCompleted;
    }

    private IEnumerator AutoRestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(panelDisplayTime);
        
        if (successRestartButton != null)
        {
            Debug.Log("자동 재시작 시간 도달 - 버튼을 눌러 재시작하세요");
        }
    }

    private void RestartGame()
    {
        Debug.Log("게임 재시작");
        
        Time.timeScale = 1f; // 게임 시간 복구
        
        // PlayerState 리셋
        PlayerState.ResetGameState();
        
        // 현재 씬 재로드
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void ExitGame()
    {
        Time.timeScale = 1f; // 게임 시간 복구
        
        // PlayerState 리셋
        PlayerState.ResetGameState();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool IsGameEnded()
    {
        return isGameEnded || PlayerState.IsGameEnded();
    }

    public void ResetGameEndState()
    {
        isGameEnded = false;
        Time.timeScale = 1f;
        
        PlayerState.ResetGameState();
        
        if (gameEndCanvas != null)
            gameEndCanvas.SetActive(false);
    }
}