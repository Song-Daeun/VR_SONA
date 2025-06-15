using UnityEngine;
using System.Collections;

public class SpellBookManager : MonoBehaviour
{
    public static SpellBookManager Instance;

    [Header("Settings")]
    public float resultDisplayTime = 5f;
    
    // 상태를 더 명확하게 관리
    public enum SpellBookState
    {
        Inactive,           // 비활성
        FirstVisit,         // 첫 방문 (효과 발동)
        EffectInProgress,   // 효과 진행 중
        Completed           // 완료됨
    }
    
    private SpellBookState currentState = SpellBookState.Inactive;
    private bool isInMissionScene = false; // 미션 씬 상태 추적
    private string lastActivatedScene = ""; // 마지막 활성화된 씬 추적
    
    // ★ 핵심 추가: 영구적인 완료 상태 추적
    private bool hasEverBeenUsed = false; // 게임 세션 중 한 번이라도 사용했는지
    private bool isSpellBookBuildingConstructed = false; // 건물 건설 여부 추적

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 씬 변경 감지를 위한 이벤트 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // 씬이 로드될 때마다 호출되는 메서드
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        string sceneName = scene.name;
        Debug.Log($"=== OnSceneLoaded 호출: {sceneName} ===");
        
        // 미션 씬 진입 감지
        if (sceneName == "MissionBasketballScene" || sceneName == "MissionWaterRushScene")
        {
            isInMissionScene = true;
            Debug.Log($"미션 씬 진입 감지: {sceneName} - SpellBook 비활성화");
            
            // 미션 씬에서는 SpellBook 강제 비활성화
            ForceDeactivateSpellBook();
        }
        // 메인 씬 복귀 감지
        else if (sceneName == "MainGameScene 1")
        {
            if (isInMissionScene)
            {
                Debug.Log($"메인 씬 복귀 감지: {sceneName}");
                isInMissionScene = false;
                // ★ 중요: 메인 씬 복귀 시에는 완료 상태를 유지하되, 진행 상태만 리셋
                if (hasEverBeenUsed)
                {
                    currentState = SpellBookState.Completed; // 이미 사용된 상태로 유지
                    Debug.Log("SpellBook 이미 사용됨 - Completed 상태 유지");
                }
                else
                {
                    currentState = SpellBookState.Inactive; // 아직 사용 안했으면 Inactive
                }
            }
        }
    }

    // 씬이 언로드될 때마다 호출되는 메서드
    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        string sceneName = scene.name;
        Debug.Log($"=== OnSceneUnloaded 호출: {sceneName} ===");
        
        // 미션 씬이 언로드되면 미션 상태 리셋
        if (sceneName == "MissionBasketballScene" || sceneName == "MissionWaterRushScene")
        {
            Debug.Log($"미션 씬 언로드 감지: {sceneName} - 미션 상태 리셋");
            isInMissionScene = false;
        }
    }

    // SpellBook 강제 비활성화 메서드
    private void ForceDeactivateSpellBook()
    {
        // ★ 상태는 건드리지 않고 UI만 닫기
        if (currentState == SpellBookState.EffectInProgress)
        {
            currentState = SpellBookState.Inactive; // 진행 중이었다면 중단
        }
        
        // UI 강제 닫기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookUI(false);
        }
        
        // 진행 중인 코루틴 모두 중단
        StopAllCoroutines();
        
        Debug.Log("SpellBook 강제 비활성화 완료");
    }

    // ★ 수정: SpellBook 상태 리셋 메서드 (완료 상태는 유지)
    public void ResetSpellBookState()
    {
        // ★ hasEverBeenUsed는 리셋하지 않음 (영구 기록)
        if (hasEverBeenUsed)
        {
            currentState = SpellBookState.Completed; // 이미 사용했다면 완료 상태 유지
        }
        else
        {
            currentState = SpellBookState.Inactive; // 아직 사용 안했다면 비활성
        }
        
        lastActivatedScene = "";
        
        Debug.Log($"SpellBook 상태 리셋 완료 - hasEverBeenUsed: {hasEverBeenUsed}, currentState: {currentState}");
    }

    // 강제 미션 상태 리셋 (외부에서 호출 가능)
    public void ForceMissionStateReset()
    {
        Debug.Log("=== ForceMissionStateReset 호출 ===");
        isInMissionScene = false;
        Debug.Log("미션 상태 강제 리셋 완료");
    }

    // ================================ //
    // 메인 활성화 로직 (수정됨)
    // ================================ //
    public void ActivateSpellBook()
    {
        Debug.Log($"=== ActivateSpellBook 호출됨 ===");
        Debug.Log($"현재 상태: {currentState}");
        Debug.Log($"hasEverBeenUsed: {hasEverBeenUsed}");
        Debug.Log($"isInMissionScene: {isInMissionScene}");
        
        // 미션 씬에서는 차단
        if (isInMissionScene) 
        {
            Debug.Log("미션 씬에서 차단됨");
            return;
        }
        
        // 현재 타일이 SpellBook이 아니면 차단
        string currentTileName = GameManager.Instance?.GetCurrentTileName();
        Debug.Log($"현재 타일: {currentTileName}");
        
        if (currentTileName != "SpellBook") 
        {
            Debug.Log($"SpellBook 타일이 아니어서 차단됨: {currentTileName}");
            return;
        }
        
        // ★ 수정된 상태별 분기 처리
        Debug.Log($"상태별 분기 처리 시작: {currentState}");
        
        // ★ 핵심 수정: hasEverBeenUsed를 우선 체크
        if (hasEverBeenUsed)
        {
            Debug.Log("이미 사용된 SpellBook - 재방문 메시지 표시");
            ShowAlreadyUsedMessage();
            return;
        }
        
        // ★ 아직 사용되지 않은 경우에만 상태에 따른 처리
        switch (currentState)
        {
            case SpellBookState.Inactive:
                Debug.Log("첫 방문 처리 시작");
                StartFirstVisit();
                break;
                
            case SpellBookState.Completed:
                Debug.Log("재방문 처리 시작 (상태는 Completed이지만 hasEverBeenUsed는 false - 이상한 상황)");
                ShowAlreadyUsedMessage();
                break;
                
            default:
                // 효과 진행 중이면 무시
                Debug.Log($"스펠북 효과 진행 중 - 중복 호출 무시: {currentState}");
                break;
        }
    }
    
    // ================================ //
    // 첫 방문 시 효과 발동
    // ================================ //
    private void StartFirstVisit()
    {
        Debug.Log("=== StartFirstVisit 시작 ===");
        currentState = SpellBookState.EffectInProgress;
        
        Debug.Log("스펠북 첫 방문 - 효과 발동");
        
        // 1단계: 먼저 건물 건설 처리
        Debug.Log("건물 건설 처리 시작");
        TriggerSpellBookBuildingConstruction();
        
        // 2단계: 건물 건설 후 즉시 승리 조건 확인
        if (GameManager.Instance != null)
        {
            bool hasWon = GameManager.Instance.CheckForBingoCompletion();
            if (hasWon)
            {
                Debug.Log("🎉 빙고 완성! 스펠북 효과 건너뛰고 즉시 게임 종료");
                
                // ★ 사용됨 표시
                hasEverBeenUsed = true;
                currentState = SpellBookState.Completed;
                
                // 게임 승리 처리
                if (GameEndManager.Instance != null)
                {
                    GameEndManager.Instance.EndGameDueToSuccess();
                }
                return; // 여기서 바로 종료, 효과 진행하지 않음
            }
            else
            {
                Debug.Log("아직 빙고 미완성 - 스펠북 효과 계속 진행");
            }
        }
        
        // 3단계: 승리하지 않았다면 UI 표시 및 효과 진행
        if (UIManager.Instance != null)
        {
            Debug.Log("SpellBook UI 표시");
            UIManager.Instance.ShowSpellBookUI(true);
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null!");
        }
        
        // 랜덤 효과 선택
        bool isAirplane = Random.Range(0, 2) == 0;
        Debug.Log($"랜덤 효과 선택: {(isAirplane ? "비행기" : "시간 보너스")}");
        
        if (isAirplane)
        {
            StartAirplaneEffect();
        }
        else
        {
            StartTimeBonusEffect();
        }
    }
    
    // ================================ //
    // 비행기 효과 (사용자 선택 필요)
    // ================================ //
    private void StartAirplaneEffect()
    {
        Debug.Log("비행기 효과 시작");
        
        StartCoroutine(AirplaneEffectFlow());
    }
    
    private IEnumerator AirplaneEffectFlow()
    {
        // 1단계: "비행기!" 메시지 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookResult("비행기!");
        }
        
        yield return new WaitForSeconds(2f);
        
        // 2단계: 타일 선택 패널 표시
        if (UIManager.Instance != null)
        {
            bool[] tileStates = GetTileStates();
            UIManager.Instance.ShowSpellBookAirplanePanel();
            UIManager.Instance.UpdateSpellBookTileButtons(tileStates, OnTileSelected);
        }
        
        // 여기서는 턴을 끝내지 않음 - 사용자 선택을 기다림
    }
    
    private void OnTileSelected(int buttonIndex)
    {
        // 타일 선택 완료
        int x = buttonIndex / 3;
        int y = buttonIndex % 3;
        string targetTileName = BingoBoard.GetTileNameByCoords(x, y);
        
        Debug.Log($"타일 선택됨: {targetTileName}");
        
        // UI 닫기
        CloseSpellBookUI();
        
        // 텔레포트 실행
        TeleportPlayerToTile(targetTileName);
        
        // 스펠북 완료 처리
        CompleteSpellBook();
    }
    
    // ================================ //
    // 시간 보너스 효과 (자동 완료)
    // ================================ //
    private void StartTimeBonusEffect()
    {
        Debug.Log("시간 보너스 효과 시작");
        
        StartCoroutine(TimeBonusEffectFlow());
    }
    
    private IEnumerator TimeBonusEffectFlow()
    {
        // 1단계: "+30초" 메시지 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookResult("+30초");
        }
        
        // 2단계: 실제 시간 추가
        AddGameTime(30f);
        
        // 3단계: 표시 시간 대기
        yield return new WaitForSeconds(resultDisplayTime);
        
        // 4단계: UI 닫기 및 완료 처리
        CloseSpellBookUI();
        CompleteSpellBook();
    }
    
    // ================================ //
    // 스펠북 완료 처리 (통합) - 수정됨
    // ================================ //
    private void CompleteSpellBook()
    {
        // ★ 핵심 수정: 영구 사용 표시
        hasEverBeenUsed = true;
        currentState = SpellBookState.Completed;
        
        Debug.Log($"스펠북 완료 - 영구 사용 표시: hasEverBeenUsed = {hasEverBeenUsed}");

        // 다음 턴 시작 (승리하지 않은 경우에만 여기까지 옴)
        StartNextTurn();
    }
    
    // 이미 완료된 스펠북에 재방문 시 - "이미 사용함" 메시지 표시
    private void ShowAlreadyUsedMessage()
    {
        Debug.Log("=== ShowAlreadyUsedMessage 시작 ===");
        Debug.Log("이미 완료된 스펠북 - '이미 사용함' 메시지 표시");
        
        StartCoroutine(ShowAlreadyUsedFlow());
    }
    
    private IEnumerator ShowAlreadyUsedFlow()
    {
        Debug.Log("=== ShowAlreadyUsedFlow 코루틴 시작 ===");
        
        // UI 표시
        if (UIManager.Instance != null)
        {
            Debug.Log("이미 사용함 UI 표시");
            UIManager.Instance.ShowSpellBookUI(true);
            UIManager.Instance.ShowSpellBookResult("이미 사용함");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null! (ShowAlreadyUsedFlow)");
        }
        
        // 3초간 메시지 표시
        Debug.Log("3초 대기 시작");
        yield return new WaitForSeconds(3f);
        Debug.Log("3초 대기 완료");
        
        // UI 닫기
        Debug.Log("SpellBook UI 닫기");
        CloseSpellBookUI();
        
        // 다음 턴 시작
        Debug.Log("다음 턴 시작 요청");
        StartNextTurn();
    }
    
    // ================================ //
    // UI 정리 및 턴 시작
    // ================================ //
    private void CloseSpellBookUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookUI(false);
        }
    }
    
    private void StartNextTurn()
    {
        Debug.Log("=== StartNextTurn 호출 ===");
        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager.StartTurn() 호출");
            GameManager.Instance.StartTurn();
        }
        else
        {
            Debug.LogError("GameManager.Instance가 null!");
        }
    }

    private void TriggerSpellBookBuildingConstruction()
    {
        // 이미 건물이 지어졌다면 건설 건너뛰기
        if (isSpellBookBuildingConstructed)
        {
            Debug.Log("🔮 SpellBook 건물이 이미 건설되어 있습니다.");
            return;
        }

        if (BingoBoard.Instance != null && PlayerState.LastEnteredTileCoords.x != -1)
        {
            Vector2Int coords = PlayerState.LastEnteredTileCoords;
            
            Debug.Log($"SpellBook 건물 최초 건설: 좌표 ({coords.x}, {coords.y})");
            
            // 빙고 보드에 성공 표시 및 건물 건설
            BingoBoard.Instance.OnMissionSuccess(coords.x, coords.y);
            
            // 건물 건설 완료 표시
            isSpellBookBuildingConstructed = true;
        }
        else
        {
            Debug.LogError("BingoBoard 또는 플레이어 위치 정보가 없어 건물 건설 실패");
        }
    }

    // ================================ //
    // 타일 상태 확인
    // ================================ //
    private bool[] GetTileStates()
    {
        bool[] tileStates = new bool[9];
        
        for (int i = 0; i < 9; i++)
        {
            int x = i / 3;
            int y = i % 3;
            
            bool isOccupied = false;
            
            if (BingoBoard.Instance != null)
            {
                isOccupied = BingoBoard.Instance.IsTileMissionCleared(x, y);
            }
            
            if (BingoBoard.GetTileNameByCoords(x, y) == "SpellBook")
            {
                isOccupied = true;
            }
            
            tileStates[i] = isOccupied;
            Debug.Log($"🔘 타일 버튼 {BingoBoard.GetTileNameByCoords(x, y)}: {(isOccupied ? "비활성화" : "활성화")}");
        }
        
        return tileStates;
    }

    // ================================ //
    // 플레이어 텔레포트
    // ================================ //
    private void TeleportPlayerToTile(string tileName)
    {
        int tileIndex = -1;
        for (int i = 0; i < GameManager.Instance.tileNames.Length; i++)
        {
            if (GameManager.Instance.tileNames[i] == tileName)
            {
                tileIndex = i;
                break;
            }
        }
        Debug.Log($"[SpellBook] 텔레포트 시도: tileName={tileName}, tileIndex={tileIndex}");

        if (tileIndex != -1)
        {
            GameManager.Instance.TeleportToTile(tileIndex);
        }
        else if (tileName == "Start")
        {
            GameManager.Instance.TeleportToStart();
        }
        else
        {
            Debug.LogError($"타일 '{tileName}'을 찾을 수 없습니다!");
            GameManager.Instance.StartTurn();
        }
    }

    // ================================ //
    // 게임 시간 추가
    // ================================ //
    private void AddGameTime(float seconds)
    {
        if (SliderTimer.Instance != null)
        {
            SliderTimer.Instance.AddTime(seconds);
        }
        else
        {
            Debug.LogError("SliderTimer.Instance를 찾을 수 없습니다!");
        }
        
        Debug.Log($"스펠북으로 게임 시간 {seconds}초 추가 요청!");
    }

    // ================================ //
    // 디버그용 상태 확인 메서드 추가
    // ================================ //
    public void DebugCurrentState()
    {
        Debug.Log($"=== SpellBook 디버그 상태 ===");
        Debug.Log($"currentState: {currentState}");
        Debug.Log($"hasEverBeenUsed: {hasEverBeenUsed}");
        Debug.Log($"isInMissionScene: {isInMissionScene}");
        Debug.Log($"isSpellBookBuildingConstructed: {isSpellBookBuildingConstructed}");
        Debug.Log($"lastActivatedScene: {lastActivatedScene}");
    }
}