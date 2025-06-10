using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // ================================ //
    // Singleton & References
    // ================================ //
    public static UIManager Instance;

    [Header("References")]
    public Transform cameraTransform;

    [Header("Dice UI")]
    public Button diceButton;
    public TextMeshProUGUI diceResultText;
    public float diceUIDistance = 2f;        // 카메라 앞 거리
    public float diceUIHeightOffset = -0.5f; // 카메라 높이 오프셋
    public float diceResultDisplayTime = 2f; // 주사위 결과 표시 시간

    [Header("Mission UI")]
    public GameObject missionPromptGroup; // Text + Buttons 같이 묶은 Panel
    public TextMeshProUGUI missionPromptText;
    public Button yesButton;
    public Button noButton;
    public float missionUIDistance = 2f;         // 카메라 앞 거리
    public float missionUIHeightOffset = 0.5f;   // 카메라 높이 오프셋

    [Header("Coin UI")]
    public TextMeshProUGUI coinText;
    public GameObject coinBackground; // 타원형 배경 이미지
    public GameObject insufficientCoinsMessage; // 코인 부족 메시지 UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ================================ //
        // UI 초기 설정
        // ================================ //
        diceButton.onClick.AddListener(OnDiceButtonClicked);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        
        ShowDiceUI(true);
        ShowMissionPrompt(false);
        ShowInsufficientCoinsMessage(false); // 코인 부족 메시지 초기 숨김
    }

    // ================================ //
    // 주사위 UI 처리
    // ================================ //
    public void ShowDiceUI(bool show)
    {
        diceButton.gameObject.SetActive(show);
        
        // DiceResult 텍스트는 초기에는 숨김 (결과가 나올 때만 표시)
        if (show)
        {
            diceResultText.gameObject.SetActive(false); // 초기에는 숨김
        }

        // 위치 및 회전 재배치
        if (show && cameraTransform != null)
        {
            Transform uiRoot = diceButton.transform.parent; // DiceCanvas 루트
            Vector3 targetPos = cameraTransform.position
                + cameraTransform.forward * diceUIDistance
                + Vector3.up * diceUIHeightOffset;
            uiRoot.position = targetPos;
            uiRoot.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
        }
    }

    public void ShowDiceResult(int number)
    {
        // 버튼만 숨기고 결과 텍스트는 유지
        diceButton.gameObject.SetActive(false);
        diceResultText.gameObject.SetActive(true);
        diceResultText.text = $"주사위 결과: {number}";
        
        Debug.Log($"🎲 주사위 결과 표시: {number}");
    }

    public void HideDiceResult()
    {
        diceResultText.gameObject.SetActive(false);
        Debug.Log("🎲 주사위 결과 숨김");
    }

    private void OnDiceButtonClicked()
    {
        int result = Random.Range(1, 7);
        
        // 1단계: 주사위 결과 표시
        ShowDiceResult(result);
        
        // 2단계: 지정된 시간 후 플레이어 이동 시작
        StartCoroutine(DelayedPlayerMove(result));
    }

    private System.Collections.IEnumerator DelayedPlayerMove(int diceResult)
    {
        // 주사위 결과를 지정된 시간만큼 표시
        yield return new WaitForSeconds(diceResultDisplayTime);
        
        // 주사위 결과 숨기기
        HideDiceResult();
        
        // 플레이어 이동 시작
        GameManager.Instance.OnDiceRolled(diceResult);
    }

    // ================================ //
    // 미션 수락 여부 UI 처리
    // ================================ //
    public void ShowMissionPrompt(bool show)
    {
        if (missionPromptGroup != null)
        {
            missionPromptGroup.SetActive(show);

            if (show && cameraTransform != null)
            {
                Transform uiRoot = missionPromptGroup.transform;
                Vector3 targetPos = cameraTransform.position
                    + cameraTransform.forward * missionUIDistance
                    + Vector3.up * missionUIHeightOffset;
                uiRoot.position = targetPos;
                uiRoot.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
            }
        }
    }

    private void OnYesClicked()
    {
        ShowMissionPrompt(false);
        GameManager.Instance.OnMissionConfirmed(true);
    }

    private void OnNoClicked()
    {
        ShowMissionPrompt(false);
        GameManager.Instance.OnMissionConfirmed(false);
    }

    // ================================ //
    // 코인 UI 처리
    // ================================ //
    public void UpdateCoinDisplay(int coinCount)
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + coinCount.ToString();
        }
    }

    public void ShowInsufficientCoinsMessage()
    {
        ShowInsufficientCoinsMessage(true);
        
        // 3초 후 자동으로 숨김
        StartCoroutine(HideInsufficientCoinsMessageAfterDelay(3f));
    }

    private void ShowInsufficientCoinsMessage(bool show)
    {
        if (insufficientCoinsMessage != null)
        {
            insufficientCoinsMessage.SetActive(show);
            
            if (show && cameraTransform != null)
            {
                // 코인 부족 메시지를 카메라 앞에 표시
                Transform messageRoot = insufficientCoinsMessage.transform;
                Vector3 targetPos = cameraTransform.position
                    + cameraTransform.forward * missionUIDistance
                    + Vector3.up * (missionUIHeightOffset + 0.3f); // 미션 UI보다 약간 위에
                messageRoot.position = targetPos;
                messageRoot.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
            }
        }
    }

    private System.Collections.IEnumerator HideInsufficientCoinsMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowInsufficientCoinsMessage(false);
    }

    // ================================ //
    // 미션 돌아가기 처리 (미션씬에서 호출)
    // ================================ //
    public static void ReturnFromMission()
    {
        Debug.Log("🔙 미션에서 돌아가기 요청됨");
        
        // Time.timeScale 정상화 (Basketball 미션에서 0으로 설정되었을 수 있음)
        Time.timeScale = 1f;
        
        // MissionManager를 통해 결과 수집 및 메인씬 복귀
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.ReturnFromMission();
        }
        else
        {
            Debug.LogError("❌ MissionManager.Instance를 찾을 수 없습니다!");
        }
    }

    // ================================ //
    // 미션씬 돌아가기 버튼용 헬퍼 (Inspector 연결용)
    // ================================ //
    public void OnMissionReturnButtonClicked()
    {
        ReturnFromMission();
    }
}