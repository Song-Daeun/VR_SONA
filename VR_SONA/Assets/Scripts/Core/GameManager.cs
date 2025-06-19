using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Manager References")]
    public PlayerManager playerManager; // 플레이어 이동
    public UIManager uiManager;
    public DiceManager diceManager;
    public GameObject player;         

    [Header("Game Configuration")]
    public string startTileName = "Start";

    [Header("Tile System")]
    public string[] tileNames = { 
        "Netherlands",  
        "Germany",      
        "USA",         
        "SpellBook",   
        "Japan",        
        "Seoul",        
        "Suncheon",     
        "Egypt"      
    };

    // 게임 상태 추적 변수들
    private int currentTileIndex = -1; 
    private int currentCoins;         
    private bool isDiceRolling = false; 

    // 미션 시스템을 위한 플레이어 위치 백업 
    private Vector3 playerPositionBeforeMission;    
    private Quaternion playerRotationBeforeMission; 
    private bool hasStoredPlayerPosition = false;   

    // 빙고 좌표 매핑 
    private System.Collections.Generic.Dictionary<string, Vector2Int> tileToCoords = 
        new System.Collections.Generic.Dictionary<string, Vector2Int>()
    {
        { "Netherlands", new Vector2Int(0, 0) }, 
        { "Germany", new Vector2Int(0, 1) },     
        { "USA", new Vector2Int(0, 2) },         
        { "SpellBook", new Vector2Int(1, 0) },   
        { "Japan", new Vector2Int(1, 1) },       
        { "Seoul", new Vector2Int(1, 2) },       
        { "Suncheon", new Vector2Int(2, 0) },    
        { "Egypt", new Vector2Int(2, 1) }       
    };

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

    void Start()
    {
        // 이벤트 시스템 구독
        PlayerManager.OnTileArrived += OnTileArrivedEvent;
        PlayerManager.OnSpellBookTileArrived += OnSpellBookArrivedEvent;
        InitializeGameSystems();
    }
    
    private void OnDestroy()
    {
        // 메모리 누수 방지
        PlayerManager.OnTileArrived -= OnTileArrivedEvent;
        PlayerManager.OnSpellBookTileArrived -= OnSpellBookArrivedEvent;
    }

    // 게임 시스템 초기화
    private void InitializeGameSystems()
    {
        currentCoins = PlayerState.InitialCoins;
        UpdateCoinDisplayUI();
        MovePlayerToStartPosition();

        StartTurn();
    }

    // 플레이어 시작 위치 설정 
    private void MovePlayerToStartPosition()
    {
        if (playerManager != null)
        {
            playerManager.MoveToStart();
            currentTileIndex = -1; // StartTile
        }
        else
        {
            Debug.LogError("PlayerManager 참조가 설정되지 않았습니다.");
        }
    }

    // 턴 관리 시스템 
    public void StartTurn()
    {
        isDiceRolling = false; 
        if (PlayerState.CanShowUI())
        {
            ActivateDiceUI(); 
        }     
    }

    private void ActivateDiceUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(true);
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다");
        }
    }

    // 공통 상태 초기화 메서드 
    private void ResetTurnState()
    {
        isDiceRolling = false;
        Debug.Log("턴 상태 초기화 완료");
    }

    // 주사위 결과 처리 시스템 (DiceManager에서 호출)
    public void OnDiceRolled(int diceResult)
    {
        // 중복 처리 방지
        if (isDiceRolling)
        {
            Debug.LogWarning("이미 주사위 결과를 처리 중입니다");
            return;
        }

        isDiceRolling = true;
        InitiatePlayerMovement(diceResult);
    }

    private void InitiatePlayerMovement(int diceResult)
    {
        if (playerManager != null)
        {
            playerManager.MovePlayer(diceResult);
        }
        else
        {
            ResetTurnState();
        }
    }

    // 타일 도착 이벤트 처리 시스템
    private void OnTileArrivedEvent(string tileName, int tileIndex)
    {
        // 게임 상태 업데이트
        currentTileIndex = tileIndex;
        UpdatePlayerStateWithCurrentLocation(tileName);

        if (tileName == "SpellBook")
        {
            // SpellBook 타일이면 전용 이벤트 호출
            OnSpellBookArrivedEvent();
        }
        else
        {
            // 일반 타일 - 미션 선택 프롬프트 표시
            StartCoroutine(ShowMissionPromptAfterDelay(0.5f));
        }

        // 턴 상태 리셋
        ResetTurnState();
    }

    // SpellBookTile 이벤트 처리
    private void OnSpellBookArrivedEvent()
    {
        // 현재 씬이 메인 게임 씬인지 확인
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "MainGameScene")
        {
            ResetTurnState();
            return;
        }
        
        if (GetCurrentTileName() != "SpellBook")
        {
            ResetTurnState();
            return;
        }

        if (SpellBookManager.Instance != null)
        {
            SpellBookManager.Instance.ActivateSpellBook();
        }
        else
        {
            Debug.LogError("SpellBookManager.Instance를 찾을 수 없습니다");
        }

        ResetTurnState();
    }

    // Player 위치 업데이트 
    private void UpdatePlayerStateWithCurrentLocation(string tileName)
    {
        if (tileToCoords.ContainsKey(tileName))
        {
            PlayerState.LastEnteredTileCoords = tileToCoords[tileName];
        }
        else
        {
            Debug.LogWarning($"{tileName}에 대한 빙고 좌표 매핑을 찾을 수 없습니다");
        }
    }

    private IEnumerator ShowMissionPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowMissionSelectionPrompt();
    }

    private void ShowMissionSelectionPrompt()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMissionPrompt(true);
            ResetTurnState(); 
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다 (ShowMissionPrompt)");
            ResetTurnState();
        }
    }

    // 미션 수락/거절
    public void OnMissionDecisionMade(bool missionAccepted)
    {
        if (missionAccepted)
        {
            ProcessMissionAcceptance();
        }
        else
        {
            ProcessMissionRejection();
        }
    }

    private void ProcessMissionAcceptance()
    {
        if (!HasSufficientCoinsForMission())
        {
            Debug.Log("코인이 부족하여 미션을 시작할 수 없습니다");
            
            // 코인 부족 시 게임 종료
            if (GameEndManager.Instance != null)
            {
                GameEndManager.Instance.EndGameDueToCoinLack();
            }
            else
            {
                DisplayInsufficientCoinsMessage();
                StartTurn();
            }
            return;
        }

        // 코인 차감 성공 시 미션 진행
        if (DeductCoinsForMission())
        {
            BackupPlayerPositionForMission();
            LoadMissionSceneForCurrentTile();
        }
    }

    private void ProcessMissionRejection()
    {
        ActivateDiceUI();
    }

    // 코인 관리 시스템 
    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public bool HasSufficientCoinsForMission()
    {
        return currentCoins >= PlayerState.MissionCost;
    }

    public bool DeductCoinsForMission()
    {
        if (HasSufficientCoinsForMission())
        {
            currentCoins -= PlayerState.MissionCost;
            UpdateCoinDisplayUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AwardCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinDisplayUI();
    }

    private void UpdateCoinDisplayUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinDisplay(currentCoins);
        }
    }

    private void DisplayInsufficientCoinsMessage()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInsufficientCoinsMessage();
        }
    }

    private void LoadMissionSceneForCurrentTile()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.LoadMissionScene(currentTileIndex);
        }
        else
        {
            StartTurn();
        }
    }

    // 플레이어 위치 백업
    private void BackupPlayerPositionForMission()
    {
        GameObject targetObject = FindPlayerObjectForBackup();
        
        if (targetObject != null)
        {
            playerPositionBeforeMission = targetObject.transform.position;
            playerRotationBeforeMission = targetObject.transform.rotation;
            hasStoredPlayerPosition = true;
        }
        else
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없어 위치를 백업할 수 없습니다");
        }
    }

    public void RestorePlayerPositionAfterMission()
    {
        GameObject targetObject = FindPlayerObjectForBackup();
        
        if (targetObject != null && hasStoredPlayerPosition)
        {
            targetObject.transform.position = playerPositionBeforeMission;
            targetObject.transform.rotation = playerRotationBeforeMission;
            
            hasStoredPlayerPosition = false;
        }
        else
        {
            Debug.LogError("위치 복구 실패: 대상 오브젝트가 없거나 저장된 위치가 없습니다");
        }
    }

    private GameObject FindPlayerObjectForBackup()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin != null) return xrOrigin;
        
        if (player != null) return player;
        return GameObject.Find("Player");
    }

    // 미션 결과 처리 시스템
    public void OnMissionResult(bool missionSuccessful)
    {
        RestorePlayerPositionAfterMission();
        
        // 미션 완료 후 SpellBook 미션 상태 강제 리셋
        if (SpellBookManager.Instance != null)
        {
            SpellBookManager.Instance.ForceMissionStateReset();
        }
        
        bool gameEnded = false; // 게임 종료 여부 추적
        
        if (missionSuccessful)
        {
            ProcessSuccessfulMission();
            
            // 승리로 게임이 종료되었는지 확인
            if (PlayerState.IsGameEnded())
            {
                gameEnded = true;
            }
        }
        else
        {
            ProcessFailedMission();
        }

        // 게임이 종료되지 않았다면 코인 부족 확인
        if (!gameEnded)
        {
            // 미션 완료 후 코인 부족 확인
            if (!HasSufficientCoinsForMission())
            {
                if (GameEndManager.Instance != null)
                {
                    GameEndManager.Instance.EndGameDueToCoinLack();
                    return;
                }
            }
            
            // 다음 턴 시작
            StartTurn();
        }
    }

    private void ProcessSuccessfulMission()
    {
        if (BingoBoard.Instance != null && PlayerState.LastEnteredTileCoords.x != -1)
        {
            Vector2Int coords = PlayerState.LastEnteredTileCoords;
            BingoBoard.Instance.OnMissionSuccess(coords.x, coords.y);
            
            if (CheckForBingoCompletion())
            {
                ProcessGameSuccess();
                return; 
            }
        }
        else
        {
            Debug.LogError("BingoBoard.Instance가 null이거나 플레이어 위치가 유효하지 않습니다");
        }
    }

    private void ProcessFailedMission()
    {
        Debug.Log("미션 실패! 다음 턴으로 진행합니다");
    }

    // 텔레포트 시스템 (SpellBook에서 사용)
    public void TeleportToTile(int targetTileIndex)
    {
        if (targetTileIndex < 0 || targetTileIndex >= tileNames.Length)
        {
            StartTurn();
            return;
        }

        if (playerManager != null)
        {
            playerManager.TeleportToTile(targetTileIndex);
            // 텔레포트 완료 대기 
            StartCoroutine(WaitForTeleportationComplete(targetTileIndex));
        }
        else
        {
            StartTurn();
        }
    }
    
    public void TeleportToStart()
    {
        if (playerManager != null)
        {
            playerManager.MoveToStart();
        }
        
        currentTileIndex = -1;
        StartTurn();
    }
    
    private IEnumerator WaitForTeleportationComplete(int targetIndex)
    {
        // 플레이어 이동이 완료될 때까지 대기
        while (playerManager.IsMoving())
        {
            yield return null;
        }
    }

    // 빙고 완성 체크 
    public bool CheckForBingoCompletion()
    {
        if (BingoBoard.Instance == null)
        {
            Debug.LogError("BingoBoard.Instance가 null입니다");
            return false;
        }

        int totalCompletedLines = 0;
        
        totalCompletedLines += CountCompletedHorizontalLines();
        totalCompletedLines += CountCompletedVerticalLines();
        totalCompletedLines += CountCompletedDiagonalLines();

        return totalCompletedLines >= 2;
    }

    private int CountCompletedHorizontalLines()
    {
        int completedCount = 0;
        
        for (int row = 0; row < 3; row++)
        {
            if (IsHorizontalLineCompleted(row))
            {
                completedCount++;
                Debug.Log($"가로 {row + 1}줄 완성!");
            }
        }
        
        return completedCount;
    }

    private int CountCompletedVerticalLines()
    {
        int completedCount = 0;
        
        for (int col = 0; col < 3; col++)
        {
            if (IsVerticalLineCompleted(col))
            {
                completedCount++;
                Debug.Log($"세로 {col + 1}줄 완성!");
            }
        }
        
        return completedCount;
    }

    private int CountCompletedDiagonalLines()
    {
        int completedCount = 0;
        
        if (IsDiagonalLineCompleted(true))
        {
            completedCount++;
            Debug.Log("대각선 (좌상→우하) 완성!");
        }
        
        if (IsDiagonalLineCompleted(false))
        {
            completedCount++;
            Debug.Log("대각선 (우상→좌하) 완성!");
        }
        
        return completedCount;
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

    // 시간 제한 시스템
    public void OnTimeUp()
    {
        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.EndGameDueToTimeUp();
        }
        else
        {
            Time.timeScale = 0f;
            
            bool hasAchievedBingo = CheckForBingoCompletion();
            
            if (hasAchievedBingo)
            {
                ProcessPartialSuccess();
            }
            else
            {
                ProcessGameDefeat();
            }
        }
    }

    private void ProcessPartialSuccess()
    {
        Invoke(nameof(RestartEntireGame), 3f);
    }

    private void ProcessGameDefeat()
    {
        Invoke(nameof(RestartEntireGame), 3f);
    }

    private void ProcessGameSuccess()
    {
        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.EndGameDueToSuccess();
        }
        else
        {
            Time.timeScale = 0f;
            DisplaySuccessUI();
            Invoke(nameof(RestartEntireGame), 5f);
        }
    }

    private void DisplaySuccessUI()
    {
        if (UIManager.Instance != null)
        {
            Debug.Log("승리 UI 표시 요청");
        }
    }

    private void RestartEntireGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // 공개 접근자 메서드
    public string GetCurrentTileName()
    {
        if (currentTileIndex == -1)
            return startTileName;
        else if (currentTileIndex >= 0 && currentTileIndex < tileNames.Length)
            return tileNames[currentTileIndex];
        else
            return "Unknown";
    }
    
    public int GetCurrentTileIndex()
    {
        return currentTileIndex;
    }
    
    public bool IsGameInProgress()
    {
        return !isDiceRolling;
    }
    
    public Vector2Int GetBingoCoordinatesForTile(string tileName)
    {
        if (tileToCoords.ContainsKey(tileName))
            return tileToCoords[tileName];
        else
            return new Vector2Int(-1, -1);
    }
    
    public string[] GetAllTileNames()
    {
        return (string[])tileNames.Clone();
    }
}