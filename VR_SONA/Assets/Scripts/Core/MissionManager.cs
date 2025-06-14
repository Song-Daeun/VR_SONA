using UnityEngine;

public class MissionManager : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static MissionManager Instance;

    [Header("Mission Scene Names")]
    public string basketballSceneName = "MissionBasketballScene";
    public string waterRushSceneName = "MissionWaterRushScene";

    [Header("Objects to Deactivate During Mission")]
    public GameObject playBoard;
    public GameObject diceCanvas;
    public GameObject missionCanvas;

    // 타일별 미션 타입 매핑 (이미지 기준)
    private readonly int[] tileMissionTypes = {
        1, 2, 1,  // Netherlands(미션1), Germany(미션2), USA(미션1)
        2, 2, 2,  // SpellBook(미션2), Japan(미션2), Seoul(미션2)
        2, 1      // Suncheon(미션2), Egypt(미션1)
    };

    // private readonly int[] tileMissionTypes = {
    //     2, 2, 2,  // Netherlands(미션1), Germany(미션2), USA(미션1)
    //     2, 2, 2,  // SpellBook(미션2), Japan(미션2), Seoul(미션2)
    //     2, 2      // Suncheon(미션2), Egypt(미션1)
    // };

    // ================================ //
    // 미션 결과 저장용 변수
    // ================================ //
    private int currentMissionTileIndex = -1; // 현재 진행 중인 미션의 타일 인덱스

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 미션 씬 로드 요청
    public void LoadMissionScene(int tileIndex)
    {
        // 디버깅 로그
        Debug.Log($"=== 미션 로드 디버그 정보 ===");
        Debug.Log($"요청된 타일 인덱스: {tileIndex}");
        Debug.Log($"tileMissionTypes 배열 길이: {tileMissionTypes.Length}");
        Debug.Log($"GameManager 타일 배열 길이: {GameManager.Instance.GetAllTileNames().Length}");
        
        if (tileIndex >= 0 && tileIndex < GameManager.Instance.GetAllTileNames().Length)
        {
            string tileName = GameManager.Instance.GetAllTileNames()[tileIndex];
            Debug.Log($"GameManager에서의 타일 이름: {tileName}");
        }
    
        if (tileIndex < 0 || tileIndex >= tileMissionTypes.Length)
        {
            Debug.LogError($"잘못된 타일 인덱스: {tileIndex}");
            return;
        }

        // 현재 미션 타일 인덱스 저장
        currentMissionTileIndex = tileIndex;

        int missionType = tileMissionTypes[tileIndex];
        string sceneName = GetMissionSceneName(missionType);
        
        Debug.Log($"타일 {tileIndex}에서 미션{missionType} 시작 → {sceneName}");

        // 게임 오브젝트들 비활성화
        DeactivateGameObjects();

        // 미션 씬 로드
        SceneLoader.Instance.LoadMissionScene(sceneName);
    }

    private string GetMissionSceneName(int missionType)
    {
        switch (missionType)
        {
            case 1:
                return basketballSceneName;
            case 2:
                return waterRushSceneName;
            default:
                Debug.LogWarning($"⚠️ 알 수 없는 미션 타입: {missionType}, 기본값으로 Basketball 사용");
                return basketballSceneName;
        }
    }

    // ================================ //
    // 게임 오브젝트 관리
    // ================================ //
    private void DeactivateGameObjects()
    {
        if (playBoard != null)
        {
            playBoard.SetActive(false);
            Debug.Log("🔇 PlayBoard 비활성화");
        }

        if (diceCanvas != null)
        {
            diceCanvas.SetActive(false);
            Debug.Log("🔇 DiceCanvas 비활성화");
        }

        if (missionCanvas != null)
        {
            missionCanvas.SetActive(false);
            Debug.Log("🔇 MissionCanvas 비활성화");
        }
    }

    public void ReactivateGameObjects()
    {
        if (playBoard != null)
        {
            playBoard.SetActive(true);
            Debug.Log("🔊 PlayBoard 활성화");
        }

        if (diceCanvas != null)
        {
            diceCanvas.SetActive(true);
            Debug.Log("🔊 DiceCanvas 활성화");
        }

        if (missionCanvas != null)
        {
            missionCanvas.SetActive(true);
            Debug.Log("🔊 MissionCanvas 활성화");
        }

        // UIManager 인스턴스 재확인 및 UI 상태 초기화
        StartCoroutine(ResetUIAfterDelay());
    }

    private System.Collections.IEnumerator ResetUIAfterDelay()
    {
        // 오브젝트 활성화 후 잠시 대기
        yield return new WaitForSeconds(0.1f);
        
        // UIManager 인스턴스 재확인
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("⚠️ UIManager.Instance가 null입니다. 다시 찾는 중...");
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log("✅ UIManager를 다시 찾았습니다.");
            }
        }
    }

    // ================================ //
    // 미션 완료 처리
    // ================================ //
    public void OnMissionCompleted(bool success)
    {
        Debug.Log($"미션 완료: {(success ? "성공" : "실패")}");

        // 미션 결과를 BingoBoard에 저장 (타일 좌표 변환)
        if (currentMissionTileIndex >= 0 && success)
        {
            Vector2Int tileCoords = GetTileCoordsFromIndex(currentMissionTileIndex);
            if (tileCoords.x != -1 && BingoBoard.Instance != null)
            {
                // 미션 성공 상태를 BingoBoard에 저장 (건물 생성은 GameManager에서 처리)
                BingoBoard.Instance.SetTileMissionCleared(tileCoords.x, tileCoords.y, true);
                Debug.Log($"💾 미션 성공 상태 저장: 타일 {currentMissionTileIndex} → 좌표 ({tileCoords.x}, {tileCoords.y})");
            }
        }
        
        // 미션 씬 언로드
        SceneLoader.Instance.UnloadMissionScene();

        // 게임 오브젝트들 재활성화
        ReactivateGameObjects();

        // GameManager에 결과 전달 (여기서 Player 위치 복구 처리됨)
        GameManager.Instance.OnMissionResult(success);

        // Dice씬 로드
        if (PlayerState.CanShowUI()) 
        { 
            DiceManager.Instance.DiceButtonClicked();
        }

        // 현재 미션 타일 인덱스 초기화
        currentMissionTileIndex = -1;
    }

    // ================================ //
    // 미션 결과 수집 (각 미션씬에서 호출)
    // ================================ //
    public void ReturnFromMission()
    {
        bool missionResult = false;

        // 미션1(Basketball) 결과 확인
        if (BasGameManager.MissionResult.HasValue)
        {
            missionResult = BasGameManager.MissionResult.Value;
            Debug.Log($"🏀 Basketball 미션 결과: {(missionResult ? "성공" : "실패")}");
            
            // 결과 초기화
            BasGameManager.MissionResult = null;
        }
        // 미션2(WaterRush) 결과 확인  
        else if (WaterCollisionHandler.missionCompleted)
        {
            // WaterRush는 BasGameManager.MissionResult도 사용하므로 우선 확인
            if (BasGameManager.MissionResult.HasValue)
            {
                missionResult = BasGameManager.MissionResult.Value;
                BasGameManager.MissionResult = null;
            }
            else
            {
                // fallback: missionCompleted만 true인 경우 (구체적 성공/실패 불명)
                missionResult = true; // 일단 성공으로 처리
            }
            
            Debug.Log($"💧 WaterRush 미션 결과: {(missionResult ? "성공" : "실패")}");
            
            // WaterRush 상태 초기화
            WaterCollisionHandler.missionCompleted = false;
        }
        else
        {
            Debug.LogWarning("⚠️ 미션 결과를 찾을 수 없습니다. 실패로 처리합니다.");
            missionResult = false;
        }

        // 미션 완료 처리
        OnMissionCompleted(missionResult);
    }

    // ================================ //
    // 유틸리티 메소드: 타일 인덱스 → 빙고 좌표 변환
    // ================================ //
    // private Vector2Int GetTileCoordsFromIndex(int tileIndex)
    // {
    //     // GameManager의 tileNames 배열과 빙고 보드 좌표 매핑
    //     switch (tileIndex)
    //     {
    //         case 0: return new Vector2Int(0, 0); // Netherlands
    //         case 1: return new Vector2Int(0, 1); // Germany
    //         case 2: return new Vector2Int(0, 2); // USA
    //         case 3: return new Vector2Int(1, 0); // SpellBook
    //         case 4: return new Vector2Int(1, 1); // Japan
    //         case 5: return new Vector2Int(1, 2); // Seoul
    //         case 6: return new Vector2Int(2, 0); // Suncheon
    //         case 7: return new Vector2Int(2, 1); // Egypt
    //         default:
    //             Debug.LogError($"❌ 잘못된 타일 인덱스: {tileIndex}");
    //             return new Vector2Int(-1, -1);
    //     }
    // }
    private Vector2Int GetTileCoordsFromIndex(int tileIndex)
    {
        // GameManager의 실제 타일 이름을 기반으로 좌표 매핑
        if (GameManager.Instance == null) 
        {
            Debug.LogError("GameManager.Instance가 null입니다");
            return new Vector2Int(-1, -1);
        }
        
        string[] tileNames = GameManager.Instance.GetAllTileNames();
        if (tileIndex < 0 || tileIndex >= tileNames.Length)
        {
            Debug.LogError($"잘못된 타일 인덱스: {tileIndex}");
            return new Vector2Int(-1, -1);
        }
        
        string tileName = tileNames[tileIndex];
        Vector2Int coords = GameManager.Instance.GetBingoCoordinatesForTile(tileName);
        
        if (coords.x == -1)
        {
            Debug.LogWarning($"타일 '{tileName}'에 대한 빙고 좌표를 찾을 수 없습니다");
        }
        
        return coords;
    }

    // ================================ //
    // 디버그용 메소드
    // ================================ //
    void Update()
    {
#if UNITY_EDITOR
        // 디버그용: M 키로 현재 미션 상태 출력
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"🔍 현재 미션 타일 인덱스: {currentMissionTileIndex}");
            
            if (BingoBoard.Instance != null)
            {
                // 모든 타일의 미션 완료 상태 출력
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        bool isCleared = BingoBoard.Instance.IsTileMissionCleared(x, y);
                        Debug.Log($"📋 타일 ({x}, {y}): {(isCleared ? "완료" : "미완료")}");
                    }
                }
            }
        }
#endif
    }
}