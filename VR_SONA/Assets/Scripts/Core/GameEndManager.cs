using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameEndManager : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static GameEndManager Instance;

    [Header("Game End UI Panels")]
    public GameObject gameEndCanvas;           // 게임 종료 전체 캔버스
    public GameObject coinLackPanel;           // 코인 부족 패널
    public GameObject timeUpPanel;             // 시간 만료 패널
    public GameObject successPanel;            // 성공 패널

    [Header("UI Components")]
    public TextMeshProUGUI coinLackTitle;      // 코인 부족 제목
    public TextMeshProUGUI coinLackMessage;    // 코인 부족 메시지
    public TextMeshProUGUI timeUpTitle;        // 시간 만료 제목
    public TextMeshProUGUI timeUpMessage;      // 시간 만료 메시지
    public TextMeshProUGUI successTitle;       // 성공 제목
    public TextMeshProUGUI successMessage;     // 성공 메시지

    [Header("Coin Lack Panel Buttons")]
    public Button coinLackRestartButton;       // 코인 부족 패널 재시작 버튼
    public Button coinLackExitButton;          // 코인 부족 패널 종료 버튼

    [Header("Time Up Panel Buttons")]
    public Button timeUpRestartButton;         // 시간 만료 패널 재시작 버튼
    public Button timeUpExitButton;            // 시간 만료 패널 종료 버튼

    [Header("Success Panel Buttons")]
    public Button successRestartButton;        // 성공 패널 재시작 버튼
    public Button successExitButton;           // 성공 패널 종료 버튼

    [Header("Settings")]
    public float panelDisplayTime = 5f;        // 패널 표시 시간

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

    // ================================ //
    // UI 초기화
    // ================================ //
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
        // 코인 부족 패널 버튼들
        if (coinLackRestartButton != null)
        {
            coinLackRestartButton.onClick.AddListener(RestartGame);
        }
        if (coinLackExitButton != null)
        {
            coinLackExitButton.onClick.AddListener(ExitGame);
        }

        // 시간 만료 패널 버튼들
        if (timeUpRestartButton != null)
        {
            timeUpRestartButton.onClick.AddListener(RestartGame);
        }
        if (timeUpExitButton != null)
        {
            timeUpExitButton.onClick.AddListener(ExitGame);
        }

        // 성공 패널 버튼들
        if (successRestartButton != null)
        {
            successRestartButton.onClick.AddListener(RestartGame);
        }
        if (successExitButton != null)
        {
            successExitButton.onClick.AddListener(ExitGame);
        }

        Debug.Log("GameEndManager 버튼 이벤트 설정 완료");
    }

    // ================================ //
    // 게임 종료 시 모든 게임플레이 UI 숨기기
    // ================================ //
    private void HideAllGameplayUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(false);
            UIManager.Instance.ShowMissionPrompt(false);
            UIManager.Instance.ShowSpellBookUI(false);
            
            Debug.Log("모든 게임플레이 UI 숨김 처리 완료");
        }
        else
        {
            Debug.LogWarning("UIManager.Instance를 찾을 수 없어 UI 숨김 처리 실패");
        }
    }

    // ================================ //
    // 코인 부족으로 인한 게임 종료
    // ================================ //
    public void EndGameDueToCoinLack()
    {
        if (isGameEnded || PlayerState.IsGameEnded()) return;

        // PlayerState에 게임 상태 설정
        PlayerState.SetGameFailedCoinLack();
        
        // 모든 게임플레이 UI 숨기기
        HideAllGameplayUI();
        
        isGameEnded = true;
        Time.timeScale = 0f; // 게임 일시정지

        Debug.Log("게임 종료: 코인 부족");

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

    // ================================ //
    // 시간 만료로 인한 게임 종료
    // ================================ //
    public void EndGameDueToTimeUp()
    {
        if (isGameEnded || PlayerState.IsGameEnded()) return;

        // PlayerState에 게임 상태 설정
        PlayerState.SetGameFailedTimeUp();
        
        // 모든 게임플레이 UI 숨기기
        HideAllGameplayUI();
        
        isGameEnded = true;
        Time.timeScale = 0f; // 게임 일시정지

        Debug.Log("게임 종료: 시간 만료");

        // 빙고 완성 여부 확인
        bool hasAchievedBingo = CheckForBingoCompletion();

        // UI 텍스트 설정
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

    // ================================ //
    // 성공으로 인한 게임 종료
    // ================================ //
    public void EndGameDueToSuccess()
    {
        if (isGameEnded || PlayerState.IsGameEnded()) return;

        // PlayerState에 게임 상태 설정
        PlayerState.SetGameSuccess();
        
        // 모든 게임플레이 UI 숨기기
        HideAllGameplayUI();
        
        isGameEnded = true;
        Time.timeScale = 0f; // 게임 일시정지

        Debug.Log("게임 종료: 성공");

        // UI 텍스트 설정
        if (successTitle != null)
            successTitle.text = "축하합니다!";
            
        if (successMessage != null)
            successMessage.text = "빙고 2줄 이상을 완성하여\n성공하셨습니다!";

        ShowGameEndPanel(successPanel);
    }

    // ================================ //
    // 공통 패널 표시 메서드 (VR용으로 수정)
    // ================================ //
    private void ShowGameEndPanel(GameObject panel)
    {
        if (gameEndCanvas != null)
        {
            gameEndCanvas.SetActive(true);
            
            // VR용 Canvas 설정 (UIManager 방식 참고)
            Canvas canvas = gameEndCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = FindCameraComponent();
                canvas.sortingOrder = 1000; // 최상위에 표시
                
                // 스케일 설정 (UIManager 참고)
                RectTransform canvasRect = gameEndCanvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.localScale = Vector3.one * 0.01f; // UIManager와 동일
                }
            }
            
            // 카메라 앞에 위치 설정
            PositionUIInFrontOfCamera();
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        // 일정 시간 후 자동 재시작 (선택사항)
        StartCoroutine(AutoRestartAfterDelay());
    }

    // VR용 카메라 찾기 및 위치 설정 메서드들
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
        
        // 카메라 앞 적당한 거리에 배치 (UIManager 방식)
        float distance = 2f;
        Vector3 targetPos = camera.transform.position + camera.transform.forward * distance;
        targetPos.y = camera.transform.position.y; // 카메라와 같은 높이
        
        gameEndCanvas.transform.position = targetPos;
        gameEndCanvas.transform.rotation = Quaternion.LookRotation(targetPos - camera.transform.position);
    }

    // 나머지 메서드들은 기존과 동일...
    
    private bool CheckForBingoCompletion()
    {
        if (BingoBoard.Instance == null)
        {
            Debug.LogError("BingoBoard.Instance가 null입니다");
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
        Debug.Log("게임 종료");
        
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

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            EndGameDueToCoinLack();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            EndGameDueToTimeUp();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            EndGameDueToSuccess();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            PlayerState.LogCurrentState();
        }
    }
#endif
}