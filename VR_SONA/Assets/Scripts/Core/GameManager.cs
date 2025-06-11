using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Manager References")]
    public PlayerManager playerManager; // 플레이어 이동
    public UIManager uiManager;
    public DiceManager diceManager;
    // public DiceResultUI diceResultUI;
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
    private Vector3 playerPositionBeforeMission;    // 미션 시작 전 위치 저장
    private Quaternion playerRotationBeforeMission; // 미션 시작 전 회전 저장
    private bool hasStoredPlayerPosition = false;   // 위치가 저장되었는지 확인

    // 빙고 좌표 매핑 
    private System.Collections.Generic.Dictionary<string, Vector2Int> tileToCoords = 
        new System.Collections.Generic.Dictionary<string, Vector2Int>()
    {
        // 3x3 빙고 보드의 좌표 
        { "Netherlands", new Vector2Int(0, 0) }, // 왼쪽 상단
        { "Germany", new Vector2Int(0, 1) },     // 왼쪽 중앙
        { "USA", new Vector2Int(0, 2) },         // 왼쪽 하단
        { "SpellBook", new Vector2Int(1, 0) },   // 중앙 상단
        { "Japan", new Vector2Int(1, 1) },       // 중앙 중앙
        { "Seoul", new Vector2Int(1, 2) },       // 중앙 하단
        { "Suncheon", new Vector2Int(2, 0) },    // 오른쪽 상단
        { "Taiwan", new Vector2Int(2, 1) }       // 오른쪽 중앙
        // Start 타일은 (2,2) 위치이지만 빙고 계산에서 자동으로 완성된 것으로 처리
    };

    private void Awake()
    {
        // 싱글톤 패턴 구현현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
            return;
        }
    }

    void Start()
    {
        // 게임 시작 시 필요한 모든 초기화 작업을 순서대로 수행
        InitializeGameSystems();
    }
    
    // public void OnDiceRollRequested()
    // {
    //     // 중복 요청 방지
    //     if (isDiceRolling)
    //     {
    //         Debug.LogWarning("이미 주사위 처리가 진행 중입니다");
    //         return;
    //     }
        
    //     // 게임 상태 검증
    //     if (!IsGameInProgress())
    //     {
    //         Debug.LogWarning("현재 주사위를 굴릴 수 없는 상태입니다");
    //         return;
    //     }
        
    //     Debug.Log("주사위 굴리기 요청 접수 - 주사위 씬 로딩을 시작합니다");
        
    //     // 주사위 상태 플래그 설정
    //     isDiceRolling = true;
        
    //     // UI 상태 변경 (버튼 비활성화 등)
    //     // uiManager.ConnectDiceButtonToDiceManager();
        
    //     // 주사위 씬 로딩 시작
    //     diceManager.LoadDiceScene();
    // }

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
            Debug.LogError("PlayerManager 참조가 설정되지 않았습니다. Inspector에서 할당해주세요");
            // 안전장치: 기존 방식으로 폴백
            // MovePlayerToStartPositionFallback();
        }
    }

    // private void MovePlayerToStartPositionFallback()
    // {
    //     // PlayerManager가 없을 때 사용하는 기존 방식
    //     GameObject startTile = GameObject.Find(startTileName);
        
    //     if (player == null)
    //         player = GameObject.Find("Player"); // 플레이어 오브젝트 자동 검색

    //     if (startTile != null && player != null)
    //     {
    //         // Start 타일의 정확한 위치 계산
    //         Vector3 targetPosition = CalculateStartTilePosition(startTile);
    //         player.transform.position = targetPosition;
            
    //         currentTileIndex = -1;
    //         Debug.Log("기존 방식으로 Start 타일 배치 완료");
    //     }
    //     else
    //     {
    //         Debug.LogError("Start 타일 또는 Player 오브젝트를 찾을 수 없습니다");
    //     }
    // }

    // private Vector3 CalculateStartTilePosition(GameObject startTile)
    // {
    //     // 타일의 Collider 또는 Renderer를 사용하여 정확한 표면 위치 계산
    //     Collider tileCollider = startTile.GetComponentInChildren<Collider>();
    //     if (tileCollider != null)
    //     {
    //         Bounds bounds = tileCollider.bounds;
    //         return new Vector3(bounds.center.x, bounds.max.y + 0.1f, bounds.center.z);
    //     }
        
    //     Renderer tileRenderer = startTile.GetComponentInChildren<Renderer>();
    //     if (tileRenderer != null)
    //     {
    //         Bounds bounds = tileRenderer.bounds;
    //         return new Vector3(bounds.center.x, bounds.max.y + 0.1f, bounds.center.z);
    //     }
        
    //     // 마지막 수단으로 타일의 Transform 위치 사용
    //     return startTile.transform.position + Vector3.up * 1.0f;
    // }

    // 턴 관리
    public void StartTurn()
    {
        Debug.Log("새로운 턴 시작 - 주사위를 굴려주세요");
        
        // 주사위 상태 초기화
        isDiceRolling = false;
        
        // UI를 통해 주사위 굴리기 버튼 활성화
        ActivateDiceUI();
    }

    private void ActivateDiceUI()
    {
        // UIManager를 통한 안전한 주사위 UI 활성화
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(true);
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다");
            AttemptUIManagerRecovery();
        }
    }

    private void AttemptUIManagerRecovery()
    {
        // UIManager가 없을 때 씬에서 다시 찾아보는 안전장치
        UIManager foundUIManager = FindObjectOfType<UIManager>();
        if (foundUIManager != null)
        {
            foundUIManager.ShowDiceUI(true);
            Debug.Log("UIManager를 성공적으로 복구하여 주사위 UI를 활성화했습니다");
        }
        else
        {
            Debug.LogError("씬에서 UIManager를 찾을 수 없습니다");
        }
    }

    // 주사위 결과 처리 시스템 (DiceResultDetector에서 호출)
    public void OnDiceRolled(int diceResult)
    {
        // 중복 처리 방지
        if (isDiceRolling)
        {
            Debug.LogWarning("이미 주사위 결과를 처리 중입니다");
            return;
        }
        
        isDiceRolling = true;
        Debug.Log("주사위 결과: " + diceResult + " - 이동 처리를 시작합니다");
        
        // 주사위 결과 UI 표시
        // DisplayDiceResultUI(diceResult);
    }

    // private void DisplayDiceResultUI(int diceResult)
    // {
    //     // DiceResultUI를 통한 결과 표시
    //     if (diceResultUI != null)
    //     {
    //         diceResultUI.ShowResult(diceResult, () => {
    //             // UI 표시가 완료된 후 플레이어 이동 시작
    //             InitiatePlayerMovement(diceResult);
    //         });
    //     }
    //     else
    //     {
    //         Debug.LogWarning("DiceResultUI가 설정되지 않았습니다. 즉시 이동을 시작합니다");
    //         InitiatePlayerMovement(diceResult);
    //     }
    // }

    private void InitiatePlayerMovement(int diceResult)
    {
        // PlayerManager를 통한 플레이어 이동 처리
        if (playerManager != null)
        {
            Debug.Log("PlayerManager를 통해 타일 " + diceResult + "로 이동 시작");
            playerManager.MovePlayer(diceResult);
            
            // 이동 완료 대기 및 후속 처리
            StartCoroutine(WaitForPlayerMovementComplete(diceResult));
        }
        else
        {
            Debug.LogError("PlayerManager가 설정되지 않아 이동할 수 없습니다");
            isDiceRolling = false;
            StartTurn(); // 실패 시 다음 턴으로
        }
    }

    // ================================ //
    // 플레이어 이동 완료 대기 및 후속 처리
    // ================================ //
    private IEnumerator WaitForPlayerMovementComplete(int diceResult)
    {
        // PlayerManager의 이동이 완료될 때까지 대기
        while (playerManager.IsMoving())
        {
            yield return null; // 매 프레임마다 확인
        }
        
        Debug.Log("플레이어 이동 완료 - 도착 처리를 시작합니다");
        
        // 도착한 타일 정보 업데이트 및 후속 처리
        ProcessPlayerArrivalAtTile(diceResult);
    }

    private void ProcessPlayerArrivalAtTile(int diceResult)
    {
        // 주사위 결과를 배열 인덱스로 변환 (1~8을 0~7로)
        int targetTileIndex = diceResult - 1;
        
        // 유효한 타일 인덱스인지 검증
        if (targetTileIndex >= 0 && targetTileIndex < tileNames.Length)
        {
            currentTileIndex = targetTileIndex;
            string arrivedTileName = tileNames[targetTileIndex];
            
            Debug.Log(arrivedTileName + " 타일에 성공적으로 도착했습니다");

            // PlayerState에 현재 위치 정보 저장 (빙고 시스템용)
            UpdatePlayerStateWithCurrentLocation(arrivedTileName);

            // 도착한 타일에 따른 게임 로직 처리
            HandleTileArrival();
        }
        else
        {
            Debug.LogError("잘못된 주사위 결과입니다: " + diceResult);
            isDiceRolling = false;
            StartTurn(); // 오류 시 다음 턴
        }
    }

    private void UpdatePlayerStateWithCurrentLocation(string tileName)
    {
        // 타일 이름을 빙고 보드 좌표로 변환하여 저장
        if (tileToCoords.ContainsKey(tileName))
        {
            PlayerState.LastEnteredTileCoords = tileToCoords[tileName];
            Debug.Log("PlayerState 위치 업데이트: " + tileName + " -> " + PlayerState.LastEnteredTileCoords);
        }
        else
        {
            Debug.LogWarning(tileName + "에 대한 빙고 좌표 매핑을 찾을 수 없습니다");
        }
    }

    // 타일별 특수 처리 시스템
    private void HandleTileArrival()
    {
        // SpellBook 타일
        if (currentTileIndex >= 0 && tileNames[currentTileIndex] == "SpellBook")
        {
            ProcessSpellBookTileArrival();
            return;
        }
        
        // 일반 타일은 미션 선택 프롬프트 표시
        if (currentTileIndex >= 0)
        {
            Invoke(nameof(ShowMissionSelectionPrompt), 0.5f);
        }
        else
        {
            // Start 타일이면 특별한 처리 없이 바로 다음 턴
            isDiceRolling = false;
            StartTurn();
        }
    }

    // Spellook Tile 로직
    private void ProcessSpellBookTileArrival()
    {
        Debug.Log("SpellBook 타일 도착 - 마법책 시스템을 활성화합니다");

        // SpellBookManager를 통한 마법책 기능 활성화
        if (SpellBookManager.Instance != null)
        {
            SpellBookManager.Instance.ActivateSpellBook();
            isDiceRolling = false; // 주사위 상태 리셋
        }
        else
        {
            Debug.LogError("SpellBookManager.Instance를 찾을 수 없습니다");
            isDiceRolling = false;
            StartTurn(); // 실패 시 다음 턴
        }
    }

    private void ShowMissionSelectionPrompt()
    {
        // UIManager를 통해 미션 선택 UI 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMissionPrompt(true);
            isDiceRolling = false; // 주사위 상태 리셋
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다 (ShowMissionPrompt)");
            isDiceRolling = false;
            StartTurn(); // 실패 시 다음 턴
        }
    }

    // 코인 관리
    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public bool HasSufficientCoinsForMission()
    {
        // PlayerState에서 설정된 미션 비용과 현재 코인 비교
        return currentCoins >= PlayerState.MissionCost;
    }

    public bool DeductCoinsForMission()
    {
        if (HasSufficientCoinsForMission())
        {
            currentCoins -= PlayerState.MissionCost;
            UpdateCoinDisplayUI();
            Debug.Log("미션 비용 차감 완료. 차감액: " + PlayerState.MissionCost + ", 잔액: " + currentCoins);
            return true;
        }
        else
        {
            Debug.Log("코인이 부족합니다. 필요: " + PlayerState.MissionCost + ", 보유: " + currentCoins);
            return false;
        }
    }

    public void AwardCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinDisplayUI();
        Debug.Log("코인 획득 완료. 획득액: " + amount + ", 현재 잔액: " + currentCoins);
    }

    private void UpdateCoinDisplayUI()
    {
        // UIManager를 통한 코인 표시 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinDisplay(currentCoins);
        }
    }

    // ================================ //
    // 미션 시스템을 위한 플레이어 위치 백업/복구
    // ================================ //
    private void BackupPlayerPositionForMission()
    {
        // 미션 시작 전 플레이어의 현재 위치와 회전을 저장
        GameObject targetObject = FindPlayerObjectForBackup();
        
        if (targetObject != null)
        {
            playerPositionBeforeMission = targetObject.transform.position;
            playerRotationBeforeMission = targetObject.transform.rotation;
            hasStoredPlayerPosition = true;
            
            Debug.Log("미션 시작 전 " + targetObject.name + " 위치 백업 완료");
            Debug.Log("저장된 위치: " + playerPositionBeforeMission);
            Debug.Log("저장된 회전: " + playerRotationBeforeMission.eulerAngles);
        }
        else
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없어 위치를 백업할 수 없습니다");
        }
    }

    public void RestorePlayerPositionAfterMission()
    {
        // 미션 완료 후 저장된 위치로 플레이어 복구
        GameObject targetObject = FindPlayerObjectForBackup();
        
        if (targetObject != null && hasStoredPlayerPosition)
        {
            targetObject.transform.position = playerPositionBeforeMission;
            targetObject.transform.rotation = playerRotationBeforeMission;
            
            Debug.Log(targetObject.name + " 위치 복구 완료");
            Debug.Log("복구된 위치: " + playerPositionBeforeMission);
            Debug.Log("복구된 회전: " + playerRotationBeforeMission.eulerAngles);
            
            // 플레이어 오브젝트들이 활성화되어 있는지 확인
            EnsurePlayerObjectsAreActive(targetObject);
            
            // 백업 플래그 초기화
            hasStoredPlayerPosition = false;
        }
        else
        {
            Debug.LogError("위치 복구 실패: 대상 오브젝트가 없거나 저장된 위치가 없습니다");
        }
    }

    private void EnsurePlayerObjectsAreActive(GameObject targetObject)
    {
        // 플레이어 오브젝트 활성화 확인
        if (!targetObject.activeInHierarchy)
        {
            targetObject.SetActive(true);
            Debug.Log(targetObject.name + " 오브젝트 활성화 완료");
        }
        
        // VR 환경에서 중요한 카메라들 활성화 확인
        Camera[] cameras = targetObject.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
        {
            if (!cam.gameObject.activeInHierarchy)
            {
                cam.gameObject.SetActive(true);
                Debug.Log("카메라 활성화 완료: " + cam.name);
            }
        }
    }
    
    private GameObject FindPlayerObjectForBackup()
    {
        GameObject xrOrigin = FindXROriginObject();
        if (xrOrigin != null)
        {
            Debug.Log("XR Origin 발견: " + xrOrigin.name);
            return xrOrigin;
        }
        
        // 일반 환경에서 Player 오브젝트 찾기
        GameObject playerObject = FindStandardPlayerObject();
        if (playerObject != null)
        {
            Debug.Log("Player 오브젝트 발견: " + playerObject.name);
            return playerObject;
        }
        
        Debug.LogError("XR Origin 또는 Player 오브젝트를 찾을 수 없습니다");
        return null;
    }

    private GameObject FindXROriginObject()
    {
        // 다양한 XR Origin 명명 패턴 검색
        string[] possibleXROriginNames = {
            "XR Origin (XR Rig)"
            // "XR Origin",
            // "XROrigin"
        };
        
        foreach (string name in possibleXROriginNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null) return found;
        }
        
        return null;
    }

    private GameObject FindStandardPlayerObject()
    {
        // Inspector에서 설정된 player 변수 우선 사용
        if (player != null) return player;
        
        // 씬에서 "Player" 이름으로 검색
        return GameObject.Find("Player");
    }


    // 미션 수락/거절 처리 시스템
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
        // 코인 부족 확인
        if (!HasSufficientCoinsForMission())
        {
            Debug.Log("코인이 부족하여 미션을 시작할 수 없습니다");
            DisplayInsufficientCoinsMessage();
            StartTurn(); // 다음 턴으로
            return;
        }

        // 코인 차감 성공 시 미션 진행
        if (DeductCoinsForMission())
        {
            // 미션 시작 전 플레이어 위치 백업 (매우 중요!)
            BackupPlayerPositionForMission();
            
            Debug.Log("미션 수락 처리 완료 - MissionManager를 호출합니다");
            
            // MissionManager를 통해 해당 타일의 미션 씬 로드
            LoadMissionSceneForCurrentTile();
        }
    }

    private void ProcessMissionRejection()
    {
        Debug.Log("미션 거절됨 - 다음 턴으로 진행합니다");
        StartTurn();
    }

    private void DisplayInsufficientCoinsMessage()
    {
        // UIManager를 통한 코인 부족 알림 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInsufficientCoinsMessage();
        }
    }

    private void LoadMissionSceneForCurrentTile()
    {
        // MissionManager를 통해 현재 타일에 해당하는 미션 씬 로드
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.LoadMissionScene(currentTileIndex);
        }
        else
        {
            Debug.LogError("MissionManager.Instance를 찾을 수 없습니다");
            StartTurn(); // 실패 시 다음 턴
        }
    }

    // 미션 결과 처리 시스템
    public void OnMissionResult(bool missionSuccessful)
    {
        // 미션 완료 후 플레이어 위치 복구
        RestorePlayerPositionAfterMission();
        
        if (missionSuccessful)
        {
            ProcessSuccessfulMission();
        }
        else
        {
            ProcessFailedMission();
        }

        // 미션 결과와 관계없이 다음 턴 시작
        StartTurn();
    }

    private void ProcessSuccessfulMission()
    {
        Debug.Log("미션 성공! 빙고 보드 업데이트 및 승리 조건을 확인합니다");
        
        // BingoBoard에 성공한 미션 결과 반영
        if (BingoBoard.Instance != null && PlayerState.LastEnteredTileCoords.x != -1)
        {
            Vector2Int coords = PlayerState.LastEnteredTileCoords;
            BingoBoard.Instance.OnMissionSuccess(coords.x, coords.y);
            
            // 빙고 완성 여부 확인
            if (CheckForBingoCompletion())
            {
                ProcessGameVictory(); // 게임 승리 처리
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
        // 유효한 타일 인덱스 검증
        if (targetTileIndex < 0 || targetTileIndex >= tileNames.Length)
        {
            Debug.LogError("잘못된 타일 인덱스입니다: " + targetTileIndex);
            StartTurn();
            return;
        }
        
        Debug.Log(tileNames[targetTileIndex] + " 타일로 텔레포트를 시작합니다");
        
        // PlayerManager를 통한 텔레포트 처리
        if (playerManager != null)
        {
            playerManager.TeleportToTile(targetTileIndex);
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
        
        // PlayerManager를 통한 Start 타일 텔레포트
        if (playerManager != null)
        {
            playerManager.MoveToStart();
        }
        
        currentTileIndex = -1; // Start 타일 인덱스
        StartTurn(); // Start 타일은 즉시 다음 턴
    }
    
    private IEnumerator WaitForTeleportationComplete(int targetIndex)
    {
        // 텔레포트 완료 대기
        while (playerManager.IsMoving())
        {
            yield return null;
        }
        
        // 텔레포트 완료 후 처리
        currentTileIndex = targetIndex;
        Debug.Log(tileNames[targetIndex] + "에 텔레포트 완료!");

        // PlayerState 위치 정보 업데이트
        UpdatePlayerStateWithCurrentLocation(tileNames[targetIndex]);

        // 잠깐 대기 후 도착 처리
        yield return new WaitForSeconds(0.5f);
        HandleTileArrival();
    }

    // 빙고 완성 체크 및 성공 조건 시스템
    private bool CheckForBingoCompletion()
    {
        if (BingoBoard.Instance == null)
        {
            Debug.LogError("BingoBoard.Instance가 null입니다");
            return false;
        }

        int totalCompletedLines = 0;
        
        // 가로 3줄 완성 체크
        totalCompletedLines += CountCompletedHorizontalLines();
        
        // 세로 3줄 완성 체크
        totalCompletedLines += CountCompletedVerticalLines();
        
        // 대각선 2줄 완성 체크
        totalCompletedLines += CountCompletedDiagonalLines();
        
        Debug.Log("총 완성된 빙고 줄 수: " + totalCompletedLines + "/8");
        
        // 2줄 이상 완성 시 게임 승리
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
                Debug.Log("가로 " + (row + 1) + "줄 완성!");
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
                Debug.Log("세로 " + (col + 1) + "줄 완성!");
            }
        }
        
        return completedCount;
    }

    private int CountCompletedDiagonalLines()
    {
        int completedCount = 0;
        
        if (IsDiagonalLineCompleted(true)) // 좌상→우하 대각선
        {
            completedCount++;
            Debug.Log("대각선 (좌상→우하) 완성!");
        }
        
        if (IsDiagonalLineCompleted(false)) // 우상→좌하 대각선
        {
            completedCount++;
            Debug.Log("대각선 (우상→좌하) 완성!");
        }
        
        return completedCount;
    }

    private bool IsHorizontalLineCompleted(int row)
    {
        // 해당 가로줄의 모든 타일이 완성되었는지 확인
        for (int col = 0; col < 3; col++)
        {
            if (!IsTilePositionCompleted(row, col))
                return false;
        }
        return true;
    }

    private bool IsVerticalLineCompleted(int col)
    {
        // 해당 세로줄의 모든 타일이 완성되었는지 확인
        for (int row = 0; row < 3; row++)
        {
            if (!IsTilePositionCompleted(row, col))
                return false;
        }
        return true;
    }

    private bool IsDiagonalLineCompleted(bool isMainDiagonal)
    {
        if (isMainDiagonal) // 좌상→우하 대각선 (0,0 → 1,1 → 2,2)
        {
            return IsTilePositionCompleted(0, 0) && 
                   IsTilePositionCompleted(1, 1) && 
                   IsTilePositionCompleted(2, 2);
        }
        else // 우상→좌하 대각선 (0,2 → 1,1 → 2,0)
        {
            return IsTilePositionCompleted(0, 2) && 
                   IsTilePositionCompleted(1, 1) && 
                   IsTilePositionCompleted(2, 0);
        }
    }

    private bool IsTilePositionCompleted(int x, int y)
    {
        // Start 타일 (2,2) 위치는 게임 시작부터 자동으로 완성된 것으로 처리
        if (x == 2 && y == 2) 
        {
            Debug.Log("Start 타일 (" + x + ", " + y + "): 시작부터 완성된 상태");
            return true;
        }
        
        // BingoBoard에서 해당 위치의 미션 완성 상태 확인
        bool isCompleted = BingoBoard.Instance != null && 
                          BingoBoard.Instance.IsTileMissionCleared(x, y);
        
        Debug.Log("타일 (" + x + ", " + y + "): " + (isCompleted ? "완성" : "미완성"));
        return isCompleted;
    }

    // 시간 제한 
    public void OnTimeUp()
    {
        Debug.Log("게임 시간이 만료되었습니다! 게임 종료 처리를 시작합니다");
        
        // 게임 일시정지
        Time.timeScale = 0f;
        
        // 시간 만료 시 빙고 달성 여부에 따른 결과 처리
        bool hasAchievedBingo = CheckForBingoCompletion();
        
        if (hasAchievedBingo)
        {
            Debug.Log("시간은 부족했지만 빙고를 달성했습니다!");
            ProcessPartialVictory();
        }
        else
        {
            Debug.Log("시간 만료로 인한 게임 종료!");
            ProcessGameDefeat();
        }
    }

    private void ProcessPartialVictory()
    {
        Debug.Log("부분 승리 달성! (시간 부족하지만 빙고 완성)");
        
        // TODO: 부분 승리 UI 표시 또는 씬 전환
        // 예: 달성한 빙고 수에 따른 점수 계산, 보상 지급 등
        
        // 임시: 3초 후 게임 재시작
        Invoke(nameof(RestartEntireGame), 3f);
    }

    private void ProcessGameDefeat()
    {
        Debug.Log("게임 패배! (시간 만료 + 빙고 미달성)");
        
        // TODO: 게임 오버 UI 표시 또는 패배 씬 전환
        
        // 임시: 3초 후 게임 재시작
        Invoke(nameof(RestartEntireGame), 3f);
    }

    // ================================ //
    // 완전 승리 처리 시스템
    // ================================ //
    private void ProcessGameVictory()
    {
        Debug.Log("완전 승리 달성! 빙고 2줄 이상 완성!");
        
        // 게임 승리 상태로 일시정지
        Time.timeScale = 0f;
        
        // 승리 UI 표시 또는 엔딩 씬 전환
        DisplayVictoryUI();
        
        // 임시: 5초 후 게임 재시작 (승리는 더 오래 표시)
        Invoke(nameof(RestartEntireGame), 5f);
    }

    private void DisplayVictoryUI()
    {
        // UIManager를 통한 승리 UI 표시
        if (UIManager.Instance != null)
        {
            // UIManager.Instance.ShowGameWinUI(); // 구현 필요
            Debug.Log("승리 UI 표시 요청 (UIManager에서 구현 필요)");
        }
    }

    private void RestartEntireGame()
    {
        // 시간 스케일 정상화
        Time.timeScale = 1f;
        
        // 현재 씬 재시작
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        
        Debug.Log("게임 재시작 완료");
    }

    // ================================ //
    // 디버그 및 개발자 도구
    // ================================ //
    void Update()
    {
#if UNITY_EDITOR
        // 개발 중 디버그용 단축키들
        HandleDevelopmentDebugInputs();
#endif
    }

#if UNITY_EDITOR
    private void HandleDevelopmentDebugInputs()
    {
        // G 키: 현재 빙고 상태 강제 체크
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("디버그: 현재 빙고 상태 강제 체크");
            CheckForBingoCompletion();
        }
        
        // C 키: 현재 코인 상태 출력
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("디버그: 현재 코인 수 = " + currentCoins);
        }
        
        // T 키: 현재 타일 위치 출력
        if (Input.GetKeyDown(KeyCode.T))
        {
            string currentLocation = currentTileIndex == -1 ? "Start" : tileNames[currentTileIndex];
            Debug.Log("디버그: 현재 위치 = " + currentLocation + " (인덱스: " + currentTileIndex + ")");
        }
        
        // R 키: 강제 턴 시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("디버그: 강제 턴 시작");
            isDiceRolling = false;
            StartTurn();
        }
    }
#endif

    // ================================ //
    // 공개 접근자 메서드들 (다른 스크립트에서 사용)
    // ================================ //
    
    /// <summary>
    /// 현재 플레이어가 위치한 타일의 이름을 반환합니다.
    /// </summary>
    public string GetCurrentTileName()
    {
        if (currentTileIndex == -1)
            return startTileName;
        else if (currentTileIndex >= 0 && currentTileIndex < tileNames.Length)
            return tileNames[currentTileIndex];
        else
            return "Unknown";
    }
    
    /// <summary>
    /// 현재 플레이어가 위치한 타일의 인덱스를 반환합니다.
    /// </summary>
    public int GetCurrentTileIndex()
    {
        return currentTileIndex;
    }
    
    /// <summary>
    /// 게임이 진행 중인지 (주사위를 굴리는 중이 아닌지) 확인합니다.
    /// </summary>
    public bool IsGameInProgress()
    {
        return !isDiceRolling;
    }
    
    /// <summary>
    /// 특정 타일 이름에 해당하는 빙고 좌표를 반환합니다.
    /// </summary>
    public Vector2Int GetBingoCoordinatesForTile(string tileName)
    {
        if (tileToCoords.ContainsKey(tileName))
            return tileToCoords[tileName];
        else
            return new Vector2Int(-1, -1); // 유효하지 않은 좌표
    }
    
    /// <summary>
    /// 모든 타일 이름 배열을 반환합니다.
    /// </summary>
    public string[] GetAllTileNames()
    {
        return (string[])tileNames.Clone(); // 복사본 반환으로 원본 보호
    }
}