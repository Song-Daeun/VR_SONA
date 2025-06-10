using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static GameManager Instance;

    [Header("References")]
    public GameObject player;
    public string startTileName = "Start"; // NOTE: 시작 타일 이름. 변경하지 말 것

    [Header("Game Economy")]
    public int startingCoins = 800;
    public int missionCost = 100;

    [Header("Tile Movement")]
    public string[] tileNames = { 
        "Netherlands",  // 주사위 1
        "Germany",      // 주사위 2
        "USA",          // 주사위 3
        "SpellBook",    // 주사위 4
        "Japan",        // 주사위 5
        "Seoul",        // 주사위 6
        "Suncheon",     // 주사위 7
        "Taiwan"        // 주사위 8
        // Start는 제외 (시작 위치)
    };
    public float moveSpeed = 2f; // 이동 속도
    
    private int currentTileIndex = -1; // -1 = Start 타일, 0~7 = 실제 타일들
    private int currentCoins; // 현재 보유 코인

    // ================================ //
    // 빙고 보드 매핑 (타일 이름 → 빙고 좌표)
    // ================================ //
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
        { "Taiwan", new Vector2Int(2, 1) }
        // Start는 빙고 보드에 포함되지 않음 (2,2)
    };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 코인 초기화
        currentCoins = startingCoins;
        UpdateCoinUI();
        
        MovePlayerToStart(); // 플레이어 Start 타일에서 시작
        StartTurn(); // 턴 시작
    }

    // ================================ //
    // 플레이어 시작 위치 설정
    // ================================ //
    private void MovePlayerToStart()
    {
        GameObject startTile = GameObject.Find(startTileName);

        if (player == null)
            player = GameObject.Find("Player"); // NOTE: 플레이어 오브젝트 이름 변경하지 말 것

        // 1) Tile의 Collider(또는 Renderer) Bounds 가져오기
        Collider tileCol = startTile.GetComponentInChildren<Collider>();
        Bounds bounds;
        if (tileCol != null)
        {
            bounds = tileCol.bounds;
        }
        else
        {
            // Collider가 없으면 Renderer로 대체
            Renderer tileRend = startTile.GetComponentInChildren<Renderer>();
            if (tileRend != null)
                bounds = tileRend.bounds;
            else
            {
                // 둘 다 없으면 기존 방식
                player.transform.position = startTile.transform.position + Vector3.up * 1.0f;
                Debug.LogWarning("🟡 Collider/Renderer 없음, 기본 위치로 이동");
                return;
            }
        }

        // 2) Bounds.center 와 Bounds.max.y 로 정확한 위치 계산
        Vector3 tileCenter = bounds.center;
        float topY = bounds.max.y;

        // 3) 플레이어 Pivot(발바닥)에 맞는 높이 오프셋 (Inspector에서 조절 가능)
        float playerHeightOffset = 0.1f; 

        // 4) 최종 포지션 세팅
        player.transform.position = new Vector3(
            tileCenter.x,
            topY + playerHeightOffset,
            tileCenter.z
        );

        currentTileIndex = -1; // Start 타일
        Debug.Log($"🚀 플레이어가 StartTile 중앙 위로 이동했습니다 → {player.transform.position}");
    }

    // ================================ //
    // 턴 시작 & 주사위 처리
    // ================================ //
    public void StartTurn()
    {
        Debug.Log("🎲 턴 시작 → 주사위를 굴려주세요");
        
        // UIManager 안전한 호출
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(true);
        }
        else
        {
            Debug.LogError("❌ UIManager.Instance가 null입니다!");
            // UIManager 다시 찾기 시도
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowDiceUI(true);
                Debug.Log("✅ UIManager를 다시 찾아서 호출했습니다.");
            }
        }
    }

    public void OnDiceRolled(int result)
    {
        Debug.Log($"🎲 주사위 결과 {result} → 해당 타일로 직접 이동");
        
        // 주사위 결과에 따라 직접 해당 타일로 이동
        MovePlayerToTile(result);
    }

    // ================================ //
    // 플레이어 이동 로직 (수정됨)
    // ================================ //
    private void MovePlayerToTile(int diceResult)
    {
        // 주사위 결과를 배열 인덱스로 변환 (1~8 → 0~7)
        int targetTileIndex = diceResult - 1;
        
        // 타일 범위 체크
        if (targetTileIndex < 0 || targetTileIndex >= tileNames.Length)
        {
            Debug.LogError($"❌ 잘못된 주사위 결과: {diceResult} (유효 범위: 1~{tileNames.Length})");
            return;
        }

        Debug.Log($"🎯 주사위 {diceResult} → {tileNames[targetTileIndex]} 타일로 이동");
        
        // 목표 타일로 이동
        StartCoroutine(MoveToTile(targetTileIndex));
    }

    private System.Collections.IEnumerator MoveToTile(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= tileNames.Length)
        {
            Debug.LogError($"❌ 잘못된 타일 인덱스: {targetIndex}");
            yield break;
        }

        // 목표 타일 찾기
        GameObject targetTile = GameObject.Find(tileNames[targetIndex]);
        if (targetTile == null)
        {
            Debug.LogError($"❌ {tileNames[targetIndex]} 타일을 찾을 수 없습니다!");
            yield break;
        }

        // 목표 위치 계산 (StartTile과 동일한 방식)
        Vector3 targetPosition = CalculateTilePosition(targetTile);

        // 이동 애니메이션
        Vector3 startPosition = player.transform.position;
        float journey = 0f;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * moveSpeed;
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, journey);
            yield return null;
        }

        // 이동 완료
        currentTileIndex = targetIndex;
        Debug.Log($"✅ 플레이어가 {tileNames[targetIndex]}에 도착했습니다!");

        // PlayerState에 현재 위치 저장
        string tileName = tileNames[targetIndex];
        if (tileToCoords.ContainsKey(tileName))
        {
            PlayerState.LastEnteredTileCoords = tileToCoords[tileName];
            Debug.Log($"📍 PlayerState 업데이트: {tileName} → {PlayerState.LastEnteredTileCoords}");
        }

        // 도착 후 처리
        OnPlayerArrived();
    }

    private Vector3 CalculateTilePosition(GameObject tile)
    {
        Collider tileCol = tile.GetComponentInChildren<Collider>();
        Bounds bounds;
        
        if (tileCol != null)
        {
            bounds = tileCol.bounds;
        }
        else
        {
            Renderer tileRend = tile.GetComponentInChildren<Renderer>();
            if (tileRend != null)
                bounds = tileRend.bounds;
            else
                return tile.transform.position + Vector3.up * 1.0f;
        }

        return new Vector3(bounds.center.x, bounds.max.y + 0.1f, bounds.center.z);
    }

    private void OnPlayerArrived()
    {
        // 미션 프롬프트 표시 (Start 타일이 아닌 경우)
        if (currentTileIndex >= 0)
        {
            Invoke(nameof(ShowMissionPrompt), 0.5f); // 0.5초 후 미션 UI 표시
        }
        else
        {
            StartTurn(); // Start 타일이면 바로 다음 턴
        }
    }

    private void ShowMissionPrompt()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMissionPrompt(true);
        }
        else
        {
            Debug.LogError("❌ UIManager.Instance가 null입니다! (ShowMissionPrompt)");
        }
    }

    // ================================ //
    // 코인 관리
    // ================================ //
    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public bool HasEnoughCoins()
    {
        return currentCoins >= missionCost;
    }

    public bool SubtractCoinsForMission()
    {
        if (HasEnoughCoins())
        {
            currentCoins -= missionCost;
            UpdateCoinUI();
            Debug.Log($"💰 코인 차감: -{missionCost}, 잔액: {currentCoins}");
            return true;
        }
        else
        {
            Debug.Log("❌ 코인이 부족합니다!");
            return false;
        }
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
        Debug.Log($"💰 코인 획득: +{amount}, 잔액: {currentCoins}");
    }

    private void UpdateCoinUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinDisplay(currentCoins);
        }
    }

    // ================================ //
    // 미션 수락/거절 처리 (코인 체크 추가)
    // ================================ //
    public void OnMissionConfirmed(bool accepted)
    {
        if (accepted)
        {
            // 코인 체크
            if (!HasEnoughCoins())
            {
                Debug.Log("❌ 코인이 부족하여 미션을 시작할 수 없습니다!");
                
                // 코인 부족 UI 표시 (UIManager에서 처리)
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowInsufficientCoinsMessage();
                }
                
                // 다음 턴으로
                StartTurn();
                return;
            }

            // 코인 차감
            if (SubtractCoinsForMission())
            {
                Debug.Log("✅ 미션 수락됨 → MissionManager 호출");
                MissionManager.Instance.LoadMissionScene(currentTileIndex);
            }
        }
        else
        {
            Debug.Log("❌ 미션 거절됨 → 다음 턴으로");
            StartTurn();
        }
    }

    // ================================ //
    // 미션 결과 처리 (MissionManager에서 호출)
    // ================================ //
    public void OnMissionResult(bool success)
    {
        if (success)
        {
            Debug.Log("🎉 미션 성공! 건물 생성 및 빙고 체크");
            
            // 1. BingoBoard에 건물 생성 요청
            if (BingoBoard.Instance != null && PlayerState.LastEnteredTileCoords.x != -1)
            {
                Vector2Int coords = PlayerState.LastEnteredTileCoords;
                BingoBoard.Instance.OnMissionSuccess(coords.x, coords.y);
                
                // 2. 빙고 완성 체크
                if (CheckBingoCompletion())
                {
                    OnGameWin();
                    return; // 게임 승리 시 다음 턴 시작하지 않음
                }
            }
            else
            {
                Debug.LogError("❌ BingoBoard.Instance가 null이거나 플레이어 위치가 유효하지 않습니다!");
            }
        }
        else
        {
            Debug.Log("💥 미션 실패! 다음 턴으로");
        }

        // 다음 턴 시작
        StartTurn();
    }

    // ================================ //
    // 빙고 완성 체크 로직
    // ================================ //
    private bool CheckBingoCompletion()
    {
        if (BingoBoard.Instance == null)
        {
            Debug.LogError("❌ BingoBoard.Instance가 null입니다!");
            return false;
        }

        int completedLines = 0;
        
        // 가로 3줄 체크
        for (int row = 0; row < 3; row++)
        {
            if (IsRowCompleted(row))
            {
                completedLines++;
                Debug.Log($"✅ 가로 {row + 1}줄 완성!");
            }
        }
        
        // 세로 3줄 체크
        for (int col = 0; col < 3; col++)
        {
            if (IsColumnCompleted(col))
            {
                completedLines++;
                Debug.Log($"✅ 세로 {col + 1}줄 완성!");
            }
        }
        
        // 대각선 2줄 체크
        if (IsDiagonalCompleted(true)) // 좌상→우하
        {
            completedLines++;
            Debug.Log("✅ 대각선 (좌상→우하) 완성!");
        }
        
        if (IsDiagonalCompleted(false)) // 우상→좌하
        {
            completedLines++;
            Debug.Log("✅ 대각선 (우상→좌하) 완성!");
        }
        
        Debug.Log($"🏁 총 완성된 줄: {completedLines}/8");
        
        // 2줄 이상 완성 시 게임 승리
        return completedLines >= 2;
    }

    private bool IsRowCompleted(int row)
    {
        for (int col = 0; col < 3; col++)
        {
            if (!IsTileCompleted(row, col))
                return false;
        }
        return true;
    }

    private bool IsColumnCompleted(int col)
    {
        for (int row = 0; row < 3; row++)
        {
            if (!IsTileCompleted(row, col))
                return false;
        }
        return true;
    }

    private bool IsDiagonalCompleted(bool mainDiagonal)
    {
        if (mainDiagonal) // 좌상→우하 (0,0 → 1,1 → 2,2)
        {
            return IsTileCompleted(0, 0) && IsTileCompleted(1, 1) && IsTileCompleted(2, 2);
        }
        else // 우상→좌하 (0,2 → 1,1 → 2,0)
        {
            return IsTileCompleted(0, 2) && IsTileCompleted(1, 1) && IsTileCompleted(2, 0);
        }
    }

    private bool IsTileCompleted(int x, int y)
    {
        // Start 타일 (2,2)은 게임 시작부터 점령된 상태로 처리
        if (x == 2 && y == 2) 
        {
            Debug.Log($"🏠 Start 타일 ({x}, {y}): 시작부터 점령됨");
            return true;
        }
        
        // BingoBoard에서 해당 타일의 완성 상태 확인
        bool isCleared = BingoBoard.Instance != null && BingoBoard.Instance.IsTileMissionCleared(x, y);
        Debug.Log($"🎯 타일 ({x}, {y}): {(isCleared ? "완성" : "미완성")}");
        return isCleared;
    }

    // ================================ //
    // 게임 승리 처리
    // ================================ //
    private void OnGameWin()
    {
        Debug.Log("🎊🎉 게임 승리! 빙고 2줄 완성! 🎉🎊");
        
        // 게임 승리 UI 표시 또는 승리 씬 로드
        // TODO: 승리 UI 구현 또는 엔딩 씬 전환
        
        // 임시로 게임 일시정지
        Time.timeScale = 0f;
        
        // 승리 메시지 UI 표시 (UIManager에 추가 필요)
        if (UIManager.Instance != null)
        {
            // UIManager.Instance.ShowGameWinUI(); // 구현 필요
        }
    }

    // ================================ //
    // 디버그용 메소드
    // ================================ //
    void Update()
    {
#if UNITY_EDITOR
        // 디버그용: G 키로 빙고 상태 출력
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("🔍 현재 빙고 상태 체크");
            CheckBingoCompletion();
        }
#endif
    }
}