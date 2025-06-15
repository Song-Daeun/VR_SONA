using UnityEngine;
using System.Collections;

public class SpellBookManager : MonoBehaviour
{
    public static SpellBookManager Instance;

    [Header("Settings")]
    public float resultDisplayTime = 5f;
    
    // 중복 호출 방지용 변수들 강화
    private bool isSpellBookActive = false;
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
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 호출되는 메서드
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        string sceneName = scene.name;
        
        // 미션 씬 진입 감지
        if (sceneName == "MissionBasketballScene" || sceneName == "MissionWaterRushScene")
        {
            isInMissionScene = true;
            Debug.Log($"미션 씬 진입 감지: {sceneName} - SpellBook 비활성화");
            
            // 미션 씬에서는 SpellBook 강제 비활성화
            ForceDeactivateSpellBook();
        }
        // 메인 씬 복귀 감지
        else if (sceneName == "MainGameScene 1" && isInMissionScene)
        {
            isInMissionScene = false;
            Debug.Log($"메인 씬 복귀 감지: {sceneName} - SpellBook 상태 리셋");
            
            // 메인 씬 복귀 시 상태 완전 리셋
            ResetSpellBookState();
        }
    }

    // 🆕 SpellBook 강제 비활성화 메서드
    private void ForceDeactivateSpellBook()
    {
        isSpellBookActive = false;
        
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
        isSpellBookActive = false;
        lastActivatedScene = "";
        
        Debug.Log("SpellBook 상태 완전 리셋 완료");
    }

    // ================================ //
    // 스펠북 활성화 (수정된 중복 호출 방지)
    // ================================ //
    public bool hasSpellBookActivatedOnce = false;

    public void ActivateSpellBook()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (isInMissionScene || currentScene == "MissionBasketballScene" || currentScene == "MissionWaterRushScene")
        {
            Debug.Log($"미션 씬에서 SpellBook 활성화 시도 차단: {currentScene}");
            return;
        }

        if (isSpellBookActive && lastActivatedScene == currentScene)
        {
            Debug.Log($"같은 씬에서 SpellBook 중복 활성화 차단: {currentScene}");
            return;
        }

        if (GameManager.Instance != null)
        {
            string currentTileName = GameManager.Instance.GetCurrentTileName();
            if (currentTileName != "SpellBook")
            {
                Debug.Log($"현재 타일이 SpellBook이 아님: {currentTileName} - 활성화 차단");
                return;
            }
        }

        if (!hasSpellBookActivatedOnce)
        {     
            // 처음 방문 시 - 기존 코드 그대로 유지
            StopAllCoroutines();
            isSpellBookActive = true;
            lastActivatedScene = currentScene;

            Debug.Log($"스펠북 최초 활성화! (씬: {currentScene})");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSpellBookUI(true);
            }

            bool isAirplane = Random.Range(0, 2) == 0;

            if (isAirplane)
            {
                ShowAirplaneEffect();
            }
            else
            {
                ShowTimeBonus();
            }
            hasSpellBookActivatedOnce = true;
        }
        else
        {
            Debug.Log($"이미 스펠북에 한 번 이상 접근 - 바로 주사위씬으로 이동");
            
            // UI가 떠있다면 닫기
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSpellBookUI(false);
            }

            // 주사위 씬으로 직접 이동하지 않고 OnSpellBookSuccess 호출
            OnSpellBookSuccess();
        }
    }

    public void OnSpellBookSuccess()
    {
        Debug.Log("마법서 미션 성공 처리!");

        if (GameManager.Instance != null)
        {
            // 빙고 보드 업데이트 (건물 건설)
            TriggerSpellBookBuildingConstruction();
            
            // 승리 조건 확인
            bool hasWon = GameManager.Instance.CheckForBingoCompletion();
            if (hasWon)
            {
                // 게임 승리 처리
                if (GameEndManager.Instance != null)
                {
                    GameEndManager.Instance.EndGameDueToSuccess();
                    return; // 게임 종료이므로 StartTurn 호출 안 함
                }
            }
            
            // 다음 턴 시작 (딜레이 추가)
            StartCoroutine(StartTurnWithDelay());
        }
    }

    // 새로 추가: 딜레이 후 턴 시작
    private IEnumerator StartTurnWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartTurn();
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
    // 시간 보너스 효과
    // ================================ //
    private void ShowTimeBonus()
    {
        Debug.Log("시간 보너스 효과 발동!");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookResult("+30초");
            Debug.Log("UIManager.ShowSpellBookResult() 호출됨");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
        
        AddGameTime(30f);
        StartCoroutine(CloseSpellBookAfterDelay());
    }

    // ================================ //
    // 비행기 효과 (텔레포트)
    // ================================ //
    private void ShowAirplaneEffect()
    {
        Debug.Log("비행기 효과 발동!");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookResult("비행기!");
            Debug.Log("UIManager.ShowSpellBookResult() 호출됨 (비행기)");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
        
        StartCoroutine(ShowAirplanePanelAfterDelay());
    }

    private IEnumerator ShowAirplanePanelAfterDelay()
    {           
        yield return new WaitForSeconds(2f);
        
        if (UIManager.Instance != null)
        {

            bool[] tileStates = GetTileStates();
            UIManager.Instance.ShowSpellBookAirplanePanel();
            UIManager.Instance.UpdateSpellBookTileButtons(tileStates, OnTileButtonClicked);
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

    private void OnTileButtonClicked(int buttonIndex)
    {
        int x = buttonIndex / 3;
        int y = buttonIndex % 3;
        string targetTileName = BingoBoard.GetTileNameByCoords(x, y);
        
        Debug.Log($"✈️ {targetTileName} 타일로 텔레포트!");
        
        CloseSpellBook();
        TeleportPlayerToTile(targetTileName);
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
    // UI 닫기 (수정됨)
    // ================================ //
    private IEnumerator CloseSpellBookAfterDelay()
    {
        yield return new WaitForSeconds(resultDisplayTime);
        CloseSpellBook();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartTurn();
        }
    }

    private void CloseSpellBook()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookUI(false);
        }
        
        // 🆕 상태 리셋 시 씬 정보도 함께 업데이트
        isSpellBookActive = false;
        
        Debug.Log("스펠북 UI 닫힘");
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
//             Debug.Log($"🔍 SpellBook 상태 - Active: {isSpellBookActive}, InMission: {isInMissionScene}, Scene: {currentScene}, LastScene: {lastActivatedScene}");
//         }
// #endif
//     }
}