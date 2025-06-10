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

    [Header("SpellBook UI")]
    public GameObject spellBookCanvas;
    // public GameObject spellBookResultPanel; // 제거 - ResultPanel 불필요
    public TextMeshProUGUI spellBookResultText; // "+30초" 또는 "비행기!" 텍스트
    public GameObject spellBookAirplanePanel; // 비행기 모드 패널
    public Button[] spellBookTileButtons = new Button[9]; // 3x3 타일 버튼들
    public TextMeshProUGUI[] spellBookTileButtonTexts = new TextMeshProUGUI[9]; // 버튼 텍스트들
    public float spellBookUIDistance = 2f;         // 카메라 앞 거리
    public float spellBookUIHeightOffset = 0f;     // 카메라 높이 오프셋

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
        ShowSpellBookUI(false); // 스펠북 UI 초기 숨김
        
        // 카메라 자동 찾기
        FindCameraTransform();
    }

    // ================================ //
    // 카메라 자동 찾기
    // ================================ //
    private void FindCameraTransform()
    {
        if (cameraTransform == null)
        {
            // 1. Player 내부에서 카메라 찾기
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                Camera playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    cameraTransform = playerCamera.transform;
                    Debug.Log("✅ Player 카메라 자동 연결됨");
                    return;
                }
            }
            
            // 2. 메인 카메라 찾기
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                Debug.Log("✅ 메인 카메라 자동 연결됨");
                return;
            }
            
            // 3. 첫 번째 카메라 찾기
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                cameraTransform = cameras[0].transform;
                Debug.Log("✅ 첫 번째 카메라 자동 연결됨");
                return;
            }
            
            Debug.LogError("❌ 카메라를 찾을 수 없습니다!");
        }
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
    // SpellBook UI 처리 (수정됨)
    // ================================ //
    public void ShowSpellBookUI(bool show)
    {
        if (spellBookCanvas != null)
        {
            spellBookCanvas.SetActive(show);
            
            // Canvas 설정 확인 및 수정
            if (show)
            {
                Canvas canvas = spellBookCanvas.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = FindCameraComponent();
                    canvas.sortingOrder = 10; // 다른 UI보다 앞에 표시
                    
                    // Canvas 크기 및 위치 설정
                    RectTransform canvasRect = spellBookCanvas.GetComponent<RectTransform>();
                    if (canvasRect != null)
                    {
                        canvasRect.localScale = Vector3.one * 0.01f; // 적절한 크기로 조정
                    }
                }
                
                // Canvas를 카메라 앞에 배치
                PositionUIInFrontOfCamera(spellBookCanvas.transform, spellBookUIDistance, spellBookUIHeightOffset);
            }
            
            Debug.Log($"📖 SpellBook Canvas 활성화: {show}");
        }
        else
        {
            Debug.LogError("❌ spellBookCanvas가 null입니다!");
        }
    }

    public void ShowSpellBookResult(string resultText)
    {
        Debug.Log($"📖 UIManager.ShowSpellBookResult() 호출됨");
        
        // ResultPanel 단계 건너뛰고 ResultText 직접 제어
        if (spellBookResultText != null)
        {
            // 1. ResultText 직접 활성화 및 설정
            spellBookResultText.gameObject.SetActive(true);
            spellBookResultText.text = resultText;
            
            Debug.Log($"📖 SpellBook ResultText 직접 활성화: '{resultText}'");
            
            // 2. Canvas 찾기 및 설정
            Canvas canvas = spellBookResultText.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Canvas 설정 최적화
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = FindCameraComponent();
                canvas.sortingOrder = 100;
                
                Debug.Log($"📖 Canvas 설정 완료 - WorldCamera: {(canvas.worldCamera != null ? canvas.worldCamera.name : "null")}");
            }
            
            // 3. ResultText 크기 및 위치 최적화
            RectTransform textRect = spellBookResultText.rectTransform;
            textRect.sizeDelta = new Vector2(800, 200); // 큰 크기
            textRect.localScale = Vector3.one; // 정상 스케일
            textRect.anchorMin = Vector2.one * 0.5f; // 중앙 앵커
            textRect.anchorMax = Vector2.one * 0.5f; // 중앙 앵커
            textRect.anchoredPosition = Vector2.zero; // 중앙 위치
            
            // 4. 텍스트 스타일 강화
            spellBookResultText.fontSize = 48; // 큰 폰트
            spellBookResultText.color = Color.white; // 흰색
            
            Debug.Log($"📖 ResultText 크기 설정: {textRect.sizeDelta}, 스케일: {textRect.localScale}");
            
            // 5. Canvas를 카메라 바로 앞에 배치
            if (canvas != null && cameraTransform != null)
            {
                // Canvas 자체를 카메라 앞 1미터에 배치
                Vector3 targetPos = cameraTransform.position + cameraTransform.forward * 1.0f;
                canvas.transform.position = targetPos;
                canvas.transform.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
                
                // Canvas 크기 조정
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.localScale = Vector3.one * 0.003f; // 매우 작게 조정
                }
                
                Debug.Log($"📖 Canvas 위치 설정: {targetPos}");
            }
        }
        else
        {
            Debug.LogError("❌ spellBookResultText가 null입니다!");
            return;
        }
        
        // AirplanePanel 숨기기 (있다면)
        if (spellBookAirplanePanel != null)
        {
            spellBookAirplanePanel.SetActive(false);
        }
        
        Debug.Log($"📖 UIManager.ShowSpellBookResult() 완료");
    }

    public void ShowSpellBookAirplanePanel()
    {
        if (spellBookAirplanePanel != null)
        {
            // ResultText 숨기기
            if (spellBookResultText != null)
                spellBookResultText.gameObject.SetActive(false);
                
            spellBookAirplanePanel.SetActive(true);
            
            // Canvas 위치 조정
            if (spellBookCanvas != null && cameraTransform != null)
            {
                PositionUIInFrontOfCamera(spellBookCanvas.transform, spellBookUIDistance, spellBookUIHeightOffset);
            }
            
            Debug.Log($"✈️ SpellBook AirplanePanel 활성화 완료");
        }
        else
        {
            Debug.LogError("❌ spellBookAirplanePanel이 null입니다!");
        }
    }

    // ================================ //
    // UI 위치 조정 헬퍼 메소드
    // ================================ //
    private void PositionUIInFrontOfCamera(Transform uiTransform, float distance, float heightOffset)
    {
        if (cameraTransform == null)
        {
            FindCameraTransform(); // 카메라 다시 찾기 시도
            if (cameraTransform == null)
            {
                Debug.LogError("❌ 카메라를 찾을 수 없어 UI 위치 설정 실패!");
                return;
            }
        }
        
        Vector3 targetPos = cameraTransform.position
            + cameraTransform.forward * distance
            + Vector3.up * heightOffset;
        
        uiTransform.position = targetPos;
        uiTransform.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
        
        Debug.Log($"📍 UI 위치 설정: {targetPos}");
        Debug.Log($"📍 카메라 위치: {cameraTransform.position}, 카메라 방향: {cameraTransform.forward}");
    }
    
    private Camera FindCameraComponent()
    {
        if (cameraTransform != null)
        {
            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam != null) return cam;
        }
        
        // fallback
        return Camera.main ?? FindObjectOfType<Camera>();
    }

    public void UpdateSpellBookTileButtons(bool[] tileStates, System.Action<int> onTileClicked)
    {
        for (int i = 0; i < spellBookTileButtons.Length && i < tileStates.Length; i++)
        {
            if (spellBookTileButtons[i] != null)
            {
                // 버튼 활성화/비활성화
                bool isOccupied = tileStates[i];
                spellBookTileButtons[i].interactable = !isOccupied;
                
                // 버튼 색상 변경
                Image buttonImage = spellBookTileButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isOccupied ? Color.gray : Color.white;
                }
                
                // 버튼 텍스트 설정 (BingoBoard의 공통 구조 사용)
                int x = i / 3;
                int y = i % 3;
                if (spellBookTileButtonTexts[i] != null)
                {
                    spellBookTileButtonTexts[i].text = BingoBoard.GetTileNameByCoords(x, y);
                }
                
                // 클릭 이벤트 설정 (기존 리스너 제거 후 새로 추가)
                spellBookTileButtons[i].onClick.RemoveAllListeners();
                int buttonIndex = i; // 클로저 문제 해결
                spellBookTileButtons[i].onClick.AddListener(() => onTileClicked(buttonIndex));
            }
        }
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