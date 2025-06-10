using UnityEngine;
using System.Collections;

public class SpellBookManager : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static SpellBookManager Instance;

    [Header("Settings")]
    public float resultDisplayTime = 5f; // 결과 표시 시간 (3초 → 5초로 증가)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ================================ //
    // 스펠북 활성화
    // ================================ //
    public void ActivateSpellBook()
    {
        Debug.Log("📖 스펠북 활성화!");
        
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
        Debug.Log("⏰ 시간 보너스 효과 발동!");
        
        // UIManager를 통해 결과 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookResult("+30초");
            Debug.Log("📖 UIManager.ShowSpellBookResult() 호출됨");
        }
        else
        {
            Debug.LogError("❌ UIManager.Instance가 null입니다!");
        }
        
        // 실제 게임 타이머에 30초 추가
        AddGameTime(30f);
        
        // 일정 시간 후 스펠북 UI 닫기
        StartCoroutine(CloseSpellBookAfterDelay());
    }

    // ================================ //
    // 비행기 효과 (텔레포트)
    // ================================ //
    private void ShowAirplaneEffect()
    {
        Debug.Log("✈️ 비행기 효과 발동!");
        
        // UIManager를 통해 결과 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookResult("비행기!");
            Debug.Log("📖 UIManager.ShowSpellBookResult() 호출됨 (비행기)");
        }
        else
        {
            Debug.LogError("❌ UIManager.Instance가 null입니다!");
        }
        
        // 2초 후 타일 선택 UI 표시
        StartCoroutine(ShowAirplanePanelAfterDelay());
    }

    private IEnumerator ShowAirplanePanelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        
        if (UIManager.Instance != null)
        {
            // 타일 상태 확인 후 UI 업데이트
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
            
            // BingoBoard에서 해당 타일의 점령 상태 확인
            bool isOccupied = false;
            
            if (BingoBoard.Instance != null)
            {
                isOccupied = BingoBoard.Instance.IsTileMissionCleared(x, y);
            }
            
            // SpellBook 타일은 선택 불가 (자기 자신)
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
        
        // 스펠북 UI 닫기
        CloseSpellBook();
        
        // 플레이어를 해당 타일로 텔레포트
        TeleportPlayerToTile(targetTileName);
    }

    // ================================ //
    // 플레이어 텔레포트
    // ================================ //
    private void TeleportPlayerToTile(string tileName)
    {
        // GameManager에서 해당 타일의 인덱스 찾기
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
            // GameManager의 텔레포트 메소드 호출
            GameManager.Instance.TeleportToTile(tileIndex);
        }
        else if (tileName == "Start")
        {
            // Start 타일인 경우 특별 처리
            GameManager.Instance.TeleportToStart();
        }
        else
        {
            Debug.LogError($"❌ 타일 '{tileName}'을 찾을 수 없습니다!");
            // 텔레포트 실패 시 다음 턴으로
            GameManager.Instance.StartTurn();
        }
    }

    // ================================ //
    // 게임 시간 추가
    // ================================ //
    private void AddGameTime(float seconds)
    {
        // SliderTimer를 통해 시간 추가
        if (SliderTimer.Instance != null)
        {
            SliderTimer.Instance.AddTime(seconds);
        }
        else
        {
            Debug.LogError("❌ SliderTimer.Instance를 찾을 수 없습니다!");
        }
        
        Debug.Log($"⏰ 스펠북으로 게임 시간 {seconds}초 추가 요청!");
    }

    // ================================ //
    // UI 닫기
    // ================================ //
    private IEnumerator CloseSpellBookAfterDelay()
    {
        yield return new WaitForSeconds(resultDisplayTime);
        CloseSpellBook();
        
        // 다음 턴 시작
        GameManager.Instance.StartTurn();
    }

    private void CloseSpellBook()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSpellBookUI(false);
        }
        
        Debug.Log("📖 스펠북 UI 닫힘");
    }

    // ================================ //
    // 디버그용
    // ================================ //
    void Update()
    {
#if UNITY_EDITOR
        // 디버그용: S 키로 스펠북 테스트
        if (Input.GetKeyDown(KeyCode.S))
        {
            ActivateSpellBook();
        }
#endif
    }
}