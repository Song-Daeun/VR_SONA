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
        // 메인 씬 복귀 감지 (더 포괄적으로)
        else if (sceneName == "MainGameScene 1")
        {
            if (isInMissionScene)
            {
                Debug.Log($"메인 씬 복귀 감지: {sceneName} - SpellBook 상태 리셋");
                isInMissionScene = false;
                // 메인 씬 복귀 시 상태 완전 리셋
                ResetSpellBookState();
            }
            else
            {
                Debug.Log($"메인 씬 로드 감지 (미션 복귀 아님): {sceneName}");
            }
        }
        // DiceScene 언로드 후에도 미션 상태 리셋
        else if (sceneName != "DiceScene" && isInMissionScene)
        {
            Debug.Log($"기타 씬 로드 감지 - 미션 상태 리셋: {sceneName}");
            isInMissionScene = false;
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

    // 🆕 SpellBook 강제 비활성화 메서드
    private void ForceDeactivateSpellBook()
    {
        currentState = SpellBookState.Inactive;
        
        // UI 강제 닫기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookUI(false);
        }
        
        // 진행 중인 코루틴 모두 중단
        StopAllCoroutines();
        
        Debug.Log("SpellBook 강제 비활성화 완료");
    }

    // SpellBook 상태 완전 리셋
    public void ResetSpellBookState()
    {
        currentState = SpellBookState.Inactive;
        lastActivatedScene = "";
        
        Debug.Log("SpellBook 상태 완전 리셋 완료");
    }

    // 🔥 강제 미션 상태 리셋 (외부에서 호출 가능)
    public void ForceMissionStateReset()
    {
        Debug.Log("=== ForceMissionStateReset 호출 ===");
        isInMissionScene = false;
        Debug.Log("미션 상태 강제 리셋 완료");
    }

    // ================================ //
    // 메인 활성화 로직 (단순화)
    // ================================ //
    public void ActivateSpellBook()
    {
        Debug.Log($"=== ActivateSpellBook 호출됨 ===");
        Debug.Log($"현재 상태: {currentState}");
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
        
        // 상태에 따른 처리
        Debug.Log($"상태별 분기 처리 시작: {currentState}");
        switch (currentState)
        {
            case SpellBookState.Inactive:
                Debug.Log("첫 방문 처리 시작");
                StartFirstVisit();
                break;
                
            case SpellBookState.Completed:
                Debug.Log("재방문 처리 시작");
                // 이미 완료된 경우 - "이미 사용함" 메시지 표시 후 바로 다음 턴
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
        
        // 🔥 1단계: 먼저 건물 건설 처리
        Debug.Log("건물 건설 처리 시작");
        TriggerSpellBookBuildingConstruction();
        
        // UI 표시
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
    // 스펠북 완료 처리 (통합)
    // ================================ //
    private void CompleteSpellBook()
    {
        currentState = SpellBookState.Completed;
        
        Debug.Log("스펠북 완료 - 게임 진행");
        
        // 🔥 건물 건설은 StartFirstVisit에서 이미 처리했으므로 여기서는 제거
        
        // 승리 조건 확인
        if (GameManager.Instance != null)
        {
            bool hasWon = GameManager.Instance.CheckForBingoCompletion();
            if (hasWon)
            {
                GameEndManager.Instance?.EndGameDueToSuccess();
                return;
            }
        }
        
        // 다음 턴 시작
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

    private bool isSpellBookBuildingConstructed = false; // 건물 건설 여부 추적을 위한 변수 추가

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
    // 디버그용
    // ================================ //
//     void Update()
//     {
// #if UNITY_EDITOR
//         // 디버그용: S 키로 스펠북 테스트 (메인 씬에서만)
//         if (Input.GetKeyDown(KeyCode.S) && !isInMissionScene)
//         {
//             ActivateSpellBook();
//         }
        
//         // 디버그용: 현재 상태 출력
//         if (Input.GetKeyDown(KeyCode.P))
//         {
//             string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//             Debug.Log($"🔍 SpellBook 상태 - State: {currentState}, InMission: {isInMissionScene}, Scene: {currentScene}, LastScene: {lastActivatedScene}");
//         }
// #endif
//     }
}