using UnityEngine;
using System.Collections;

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
    private int currentTileIndex = -1; // 현재 위치: -1=Start타일, 0~7=일반타일들
    private int currentCoins;          // 현재 보유 코인 수
    private bool isDiceRolling = false; // 주사위 굴리는 중인지 확인

    // 미션 시스템을 위한 플레이어 위치 백업 시스템
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
        
        Debug.Log("PlayerManager 이벤트 구독 완료");
        
        InitializeGameSystems();
    }
    
    private void InitializeGameSystems()
    {
        Debug.Log("게임 시스템 초기화 시작");

        // 초기 코인 수 가져오기
        currentCoins = PlayerState.InitialCoins;
        UpdateCoinDisplayUI();

        // 플레이어를 게임 시작 위치로 이동
        MovePlayerToStartPosition();

        // 첫 번째 턴 시작
        StartTurn();

        Debug.Log("게임 초기화 완료 - 플레이어가 Start 타일에서 게임을 시작합니다");
    }

    // 플레이어 시작 위치 설정 
    private void MovePlayerToStartPosition()
    {
        if (playerManager != null)
        {
            Debug.Log("PlayerManager를 통해 Start 타일로 이동 요청");
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
        Debug.Log("새로운 턴 시작 - 주사위를 굴려주세요");
        
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
            Debug.Log("주사위 UI 활성화 완료");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다");
        }
    }

    // 주사위 결과 처리 시스템 (DiceManager에서 호출)
    public void OnDiceRolled(int diceResult)
    {
        Debug.Log($"OnDiceRolled 호출됨 - 주사위 결과: {diceResult} ===");
        
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
        Debug.Log($"플레이어 이동 시작 - 주사위 결과: {diceResult} ===");
        
        if (playerManager != null)
        {
            playerManager.MovePlayer(diceResult);
            Debug.Log("PlayerManager에게 이동 요청 완료. 이벤트 알림을 기다립니다.");
        }
        else
        {
            Debug.LogError("PlayerManager가 설정되지 않아 이동할 수 없습니다");
            ResetTurnState();
        }
    }

    // 모든 타일 도착 처리를 통합
    private void OnTileArrivedEvent(string tileName, int tileIndex)
    {
        Debug.Log($"=== 이벤트: 타일 도착 알림 - {tileName} (인덱스: {tileIndex}) ===");
        
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
            // 일반 타일 - 미션 선택 프롬프트 표시 예정
            Debug.Log("일반 타일 - 미션 선택 프롬프트 표시 예정");
            StartCoroutine(ShowMissionPromptAfterDelay(0.5f));
        }

        // 턴 상태 리셋
        ResetTurnState();
    }

    // SpellBookTile 이벤트 처리
    private void OnSpellBookArrivedEvent()
    {
        Debug.Log("이벤트: SpellBook 타일 도착 알림 ===");
        
        // 현재 씬이 메인 게임 씬인지 확인
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "MainGameScene") // 여기서 "MainGameScene"을 실제 메인 씬 이름으로 변경
        {
            Debug.Log($"메인 씬이 아닌 곳에서 SpellBook 이벤트 차단: {currentScene}");
            ResetTurnState();
            return;
        }
        
        // 🆕 현재 타일이 정말 SpellBook인지 재확인
        if (GetCurrentTileName() != "SpellBook")
        {
            Debug.Log($"현재 타일이 SpellBook이 아님: {GetCurrentTileName()} - 이벤트 차단");
            ResetTurnState();
            return;
        }
        
        if (SpellBookManager.Instance != null)
        {
            SpellBookManager.Instance.ResetSpellBookState();
            SpellBookManager.Instance.ActivateSpellBook();

            SpellBookManager.Instance.OnSpellBookSuccess(); // SpellBook 성공 처리
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
            Debug.Log($"PlayerState 위치 업데이트: {tileName} -> {PlayerState.LastEnteredTileCoords}");
        }
        else
        {
            Debug.LogWarning($"{tileName}에 대한 빙고 좌표 매핑을 찾을 수 없습니다");
        }
    }

    private IEnumerator ShowMissionPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log("미션 선택 프롬프트 표시 시작");
        ShowMissionSelectionPrompt();
    }
    
    private void OnDestroy()
    {
        // 메모리 누수 방지
        PlayerManager.OnTileArrived -= OnTileArrivedEvent;
        PlayerManager.OnSpellBookTileArrived -= OnSpellBookArrivedEvent;
        
        Debug.Log("GameManager 이벤트 구독 해제 완료");
    }

    private void ShowMissionSelectionPrompt()
    {
        Debug.Log("=== ShowMissionSelectionPrompt 호출됨 ===");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMissionPrompt(true);
            ResetTurnState(); // 주사위 상태 리셋
            Debug.Log("미션 프롬프트 표시 완료");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다 (ShowMissionPrompt)");
            ResetTurnState();
        }
    }

    // 공통 상태 초기화 메서드 
    private void ResetTurnState()
    {
        isDiceRolling = false;
        Debug.Log("턴 상태 초기화 완료");
    }

    // 미션 수락/거절 처리 시스템 
    public void OnMissionDecisionMade(bool missionAccepted)
    {
        Debug.Log($"미션 결정 - 수락: {missionAccepted}");
        
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
        Debug.Log("미션 수락 처리 시작");
        
        // 코인 부족 확인
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
                // 기존 코드 (fallback)
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
        Debug.Log("미션 거절됨 - 다음 턴으로 진행합니다");
        StartTurn();
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
            Debug.Log($"미션 비용 차감 완료. 차감액: {PlayerState.MissionCost}, 잔액: {currentCoins}");
            return true;
        }
        else
        {
            Debug.Log($"코인이 부족합니다. 필요: {PlayerState.MissionCost}, 보유: {currentCoins}");
            return false;
        }
    }

    public void AwardCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinDisplayUI();
        Debug.Log($"코인 획득 완료. 획득액: {amount}, 현재 잔액: {currentCoins}");
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
            Debug.LogError("MissionManager.Instance를 찾을 수 없습니다");
            StartTurn();
        }
    }

    // 플레이어 위치 백업/복구 시스템 
    private void BackupPlayerPositionForMission()
    {
        GameObject targetObject = FindPlayerObjectForBackup();
        
        if (targetObject != null)
        {
            playerPositionBeforeMission = targetObject.transform.position;
            playerRotationBeforeMission = targetObject.transform.rotation;
            hasStoredPlayerPosition = true;
            
            Debug.Log($"미션 시작 전 {targetObject.name} 위치 백업 완료");
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
            
            Debug.Log($"{targetObject.name} 위치 복구 완료");
            hasStoredPlayerPosition = false;
        }
        else
        {
            Debug.LogError("위치 복구 실패: 대상 오브젝트가 없거나 저장된 위치가 없습니다");
        }
    }

    private GameObject FindPlayerObjectForBackup()
    {
        // XR Origin 검색
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin != null) return xrOrigin;
        
        // Player 오브젝트 검색
        if (player != null) return player;
        return GameObject.Find("Player");
    }

    // 미션 결과 처리 시스템
    public void OnMissionResult(bool missionSuccessful)
    {
        RestorePlayerPositionAfterMission();
        
        if (missionSuccessful)
        {
            ProcessSuccessfulMission();
        }
        else
        {
            ProcessFailedMission();
        }

        StartTurn(); // 미션 결과와 관계없이 다음 턴 시작
    }

    private void ProcessSuccessfulMission()
    {
        Debug.Log("미션 성공! 빙고 보드 업데이트 및 승리 조건을 확인합니다");
        
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
            Debug.LogError($"잘못된 타일 인덱스입니다: {targetTileIndex}");
            StartTurn();
            return;
        }
        
        Debug.Log($"{tileNames[targetTileIndex]} 타일로 텔레포트를 시작합니다");
        
        if (playerManager != null)
        {
            playerManager.TeleportToTile(targetTileIndex);
            // 텔레포트 완료 대기 
            StartCoroutine(WaitForTeleportationComplete(targetTileIndex));
        }
        else
        {
            Debug.LogError("PlayerManager가 없어 텔레포트할 수 없습니다");
            StartTurn();
        }
    }
    
    public void TeleportToStart()
    {
        Debug.Log("Start 타일로 텔레포트를 시작합니다");
        
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
        
        Debug.Log($"{tileNames[targetIndex]}에 텔레포트 완료!");
    }

    // 빙고 완성 체크 시스템 
    public bool CheckForBingoCompletion()
    {
        // 기존 코드 그대로 사용하되, public으로 변경하여 GameEndManager에서도 사용 가능하게 함
        if (BingoBoard.Instance == null)
        {
            Debug.LogError("BingoBoard.Instance가 null입니다");
            return false;
        }

        int totalCompletedLines = 0;
        
        totalCompletedLines += CountCompletedHorizontalLines();
        totalCompletedLines += CountCompletedVerticalLines();
        totalCompletedLines += CountCompletedDiagonalLines();
        
        Debug.Log($"총 완성된 빙고 줄 수: {totalCompletedLines}/8");
        
        return totalCompletedLines >= 1;
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
        Debug.Log("게임 시간이 만료되었습니다!");
        
        // GameEndManager를 통해 시간 만료 처리
        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.EndGameDueToTimeUp();
        }
        else
        {
            // 기존 코드 (fallback)
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
        Debug.Log("부분 승리 달성! (시간 부족하지만 빙고 완성)");
        Invoke(nameof(RestartEntireGame), 3f);
    }

    private void ProcessGameDefeat()
    {
        Debug.Log("게임 패배! (시간 만료 + 빙고 미달성)");
        Invoke(nameof(RestartEntireGame), 3f);
    }

    private void ProcessGameSuccess()
    {
        Debug.Log("빙고 2줄 이상 완성");
        
        // GameEndManager를 통해 성공 처리
        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.EndGameDueToSuccess();
        }
        else
        {
            // 기존 코드 (fallback)
            Time.timeScale = 0f;
            DisplaySuccessUI();
            Invoke(nameof(RestartEntireGame), 5f);
        }
    }

    private void DisplaySuccessUI()
    {
        if (UIManager.Instance != null)
        {
            Debug.Log("승리 UI 표시 요청 (UIManager에서 구현 필요)");
        }
    }

    private void RestartEntireGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // 디버그 시스템 
    void Update()
    {
#if UNITY_EDITOR
        HandleDevelopmentDebugInputs();
#endif
    }

#if UNITY_EDITOR
    private void HandleDevelopmentDebugInputs()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("디버그: 현재 빙고 상태 강제 체크");
            CheckForBingoCompletion();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            string currentLocation = currentTileIndex == -1 ? "Start" : tileNames[currentTileIndex];
            Debug.Log($"디버그: 현재 위치 = {currentLocation} (인덱스: {currentTileIndex})");
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("디버그: 강제 턴 시작");
            ResetTurnState();
            StartTurn();
        }
    }
#endif

    // 공개 접근자 메서드들 
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