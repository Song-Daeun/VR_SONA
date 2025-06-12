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
        else if (sceneName == "MainGameScene" && isInMissionScene)
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
    private void ResetSpellBookState()
    {
        isSpellBookActive = false;
        lastActivatedScene = "";
        
        Debug.Log("SpellBook 상태 완전 리셋 완료");
    }

    // ================================ //
    // 스펠북 활성화 (수정된 중복 호출 방지)
    // ================================ //
    public void ActivateSpellBook()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // 미션 씬에서는 절대 활성화하지 않음
        if (isInMissionScene || currentScene == "MissionBasketballScene" || currentScene == "MissionWaterRushScene")
        {
            Debug.Log($"미션 씬에서 SpellBook 활성화 시도 차단: {currentScene}");
            return;
        }
        
        // 같은 씬에서 이미 활성화되었다면 무시
        if (isSpellBookActive && lastActivatedScene == currentScene)
        {
            Debug.Log($"같은 씬에서 SpellBook 중복 활성화 차단: {currentScene}");
            return;
        }
        
        // GameManager 상태 확인 추가
        if (GameManager.Instance != null)
        {
            string currentTileName = GameManager.Instance.GetCurrentTileName();
            if (currentTileName != "SpellBook")
            {
                Debug.Log($"현재 타일이 SpellBook이 아님: {currentTileName} - 활성화 차단");
                return;
            }
        }
        
        isSpellBookActive = true;
        lastActivatedScene = currentScene;
        
        Debug.Log($"스펠북 활성화! (씬: {currentScene})");
        
        // UIManager를 통해 스펠북 UI 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookUI(true);
        }
        
        // 랜덤으로 효과 선택 (50% 확률)
        bool isAirplane = Random.Range(0, 2) == 0;
        
        if (isAirplane)
        {
            ShowAirplaneEffect();
        }
        else
        {
            ShowTimeBonus();
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
    void Update()
    {
#if UNITY_EDITOR
        // 디버그용: S 키로 스펠북 테스트 (메인 씬에서만)
        if (Input.GetKeyDown(KeyCode.S) && !isInMissionScene)
        {
            ActivateSpellBook();
        }
        
        // 디버그용: 현재 상태 출력
        if (Input.GetKeyDown(KeyCode.P))
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"🔍 SpellBook 상태 - Active: {isSpellBookActive}, InMission: {isInMissionScene}, Scene: {currentScene}, LastScene: {lastActivatedScene}");
        }
#endif
    }
}