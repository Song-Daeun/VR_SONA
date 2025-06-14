using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Dice UI")]
    public GameObject diceGroup;
    public Button diceButton;
    public float diceUIDistance = 2f;
    public float diceUIHeightOffset = 0.5f;

    [Header("Mission UI")]
    public GameObject missionPromptGroup;
    public TextMeshProUGUI missionPromptText;
    public Button yesButton;
    public Button noButton;
    public float missionUIDistance = 2f;
    public float missionUIHeightOffset = 0.5f;

    [Header("Coin UI")]
    public TextMeshProUGUI coinText;
    public GameObject coinBackground;
    public GameObject insufficientCoinsMessage;

    [Header("SpellBook UI")]
    public GameObject spellBookCanvas;
    public GameObject spellBookResultPanel;
    public TextMeshProUGUI spellBookResultText;
    public GameObject spellBookAirplanePanel;
    public Button[] spellBookTileButtons = new Button[9];
    public float spellBookUIDistance = 2f;
    public float spellBookUIHeightOffset = 0f;

    [Header("Game State Tracking")] 
    private bool isInMission = false;
    private bool diceUIWasActiveBeforeMission = false; 

    private bool isDiceSceneActive = false;
    private bool shouldShowDiceUI = true; // DiceUI 표시 여부

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InitializeUISystem();
    }

    private void InitializeUISystem()
    {
        Debug.Log("UIManager 초기화: 주사위 버튼 이벤트 연결");
        
        ConnectDiceButtonToDiceManager();
        ConnectMissionButtons();
        
        // 플레이어 이동 완료 후 UI 활성화
        StartCoroutine(WaitForPlayerAndInitializeUI());
        
        // 카메라 자동 찾기
        FindCameraTransform();
        
        Debug.Log("UIManager 초기화 완료");
    }

    // 플레이어가 준비된 후 UI 초기화
    private IEnumerator WaitForPlayerAndInitializeUI()
    {
        Debug.Log("플레이어 준비 상태 확인 시작...");
        
        // PlayerManager가 존재할 때까지 대기
        while (PlayerManager.Instance == null)
        {
            Debug.Log("PlayerManager 인스턴스 대기 중...");
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("PlayerManager 인스턴스 발견됨");
        
        // 플레이어가 이동 중이 아닐 때까지 대기
        while (PlayerManager.Instance.IsMoving())
        {
            Debug.Log("플레이어 이동 완료 대기 중...");
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("플레이어 이동 완료 확인됨");
        
        // 추가 안전 대기 시간
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("UI 초기화 시작 - 플레이어가 완전히 준비됨");
        SetupUIAfterPlayerReady();
    }

    // 플레이어 준비 완료 후 UI 설정
    private void SetupUIAfterPlayerReady()
    {
        Debug.Log("플레이어 준비 완료 후 UI 설정 시작");
        
        if (cameraTransform == null)
        {
            FindCameraTransform();
        }
        
        // 플레이어 위치가 안정된 상태에서 UI 활성화
        SetInitialUIStates();
        
        Debug.Log("UI 설정 완료 - 플레이어 위치 기준으로 정확히 배치됨");
    }

    // 주사위 버튼을 DiceManager에 직접 연결
    private void ConnectDiceButtonToDiceManager()
    {
        if (diceButton != null)
        {
            diceButton.onClick.AddListener(OnDiceButtonClicked);
            Debug.Log("주사위 버튼이 DiceManager에 연결됨");
        }
        else
        {
            Debug.LogError("주사위 버튼을 찾을 수 없습니다!");
        }
    }

    private void OnDiceButtonClicked()
    {
        Debug.Log("주사위 버튼 클릭 감지");
        
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.LoadDiceScene();
        }
        else
        {
            Debug.LogError("DiceManager.Instance를 찾을 수 없습니다!");
        }
    }

    private void ConnectMissionButtons()
    {
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
        }
    }

    private void OnYesClicked()
    {
        diceUIWasActiveBeforeMission = (diceGroup != null && diceGroup.activeSelf);
        isInMission = true;
        
        Debug.Log($"미션 시작: 이전 주사위 UI 상태 = {diceUIWasActiveBeforeMission}");
        
        ShowMissionPrompt(false);
        GameManager.Instance?.OnMissionDecisionMade(true);
    }

    private void OnNoClicked()
    {
        Debug.Log("미션 No 버튼 클릭 - 미션 거부");

        isInMission = false; // ★ 이 줄 추가

        ShowMissionPrompt(false);
        ShowDiceUI(true);

        Debug.Log("미션 거부 후 주사위 UI 복구 완료");

        GameManager.Instance?.OnMissionDecisionMade(false);

        if (diceButton != null)
        {
            diceButton.onClick.RemoveAllListeners();
            diceButton.onClick.AddListener(OnDiceButtonClicked);
        }
    }

    // 초기 UI 상태 설정
    private void SetInitialUIStates()
    {
        Debug.Log("UI 초기 상태 설정 시작");
        
        ShowDiceUI(true);                    // 주사위 버튼 활성화
        ShowMissionPrompt(false);            // 미션 프롬프트 숨김
        ShowInsufficientCoinsMessage(false); // 코인 부족 메시지 숨김
        ShowSpellBookUI(false);              // 스펠북 UI 숨김
        
        Debug.Log("UI 초기 상태 설정 완료");
    }

    // 간소화된 주사위 UI 표시
    // public void ShowDiceUI(bool show)
    // {
    //     Debug.Log($"ShowDiceUI 호출: show = {show}");

    //     if (diceGroup != null)
    //     {
    //         if (show)
    //         {
    //             Debug.Log("주사위 UI 활성화 시작");

    //             // 카메라 참조 확보
    //             if (cameraTransform == null)
    //             {
    //                 FindCameraTransform();
    //             }

    //             if (cameraTransform != null)
    //             {
    //                 Debug.Log($"카메라 위치: {cameraTransform.position}");

    //                 // PlayerManager가 이미 준비되어 있으므로 안전하게 위치 계산 가능
    //                 if (PlayerManager.Instance != null)
    //                 {
    //                     Vector3 playerPos = PlayerManager.Instance.GetPlayerPosition();
    //                     Debug.Log($"플레이어 위치: {playerPos}");
    //                 }

    //                 // UI 위치 설정 - 이제 정확한 플레이어 위치 기준으로 계산됨
    //                 PositionUIInFrontOfCamera(diceGroup.transform, diceUIDistance, diceUIHeightOffset);

    //                 // UI 활성화
    //                 diceGroup.SetActive(true);

    //                 Debug.Log($"주사위 UI 최종 위치: {diceGroup.transform.position}");
    //                 Debug.Log("주사위 UI 활성화 완료");
    //             }
    //             else
    //             {
    //                 Debug.LogError("카메라를 찾을 수 없어서 UI 배치 불가능!");
    //                 // 그래도 UI는 활성화 (기본 위치에서라도)
    //                 diceGroup.SetActive(true);
    //             }
    //         }
    //         else
    //         {
    //             diceGroup.SetActive(false);
    //             Debug.Log("주사위 UI 비활성화");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("diceGroup이 null입니다!");
    //     }
    // }
    // public void ShowDiceUI(bool show)
    // {
    //     Debug.Log($"ShowDiceUI 호출: show = {show}");

    //     // DiceScene이 로드되어 있으면 UI 표시를 차단
    //     if (show && DiceManager.Instance != null && DiceManager.Instance.IsDiceSceneLoaded())
    //     {
    //         Debug.Log("DiceScene이 로드되어 있어서 DiceUI 표시를 차단합니다.");
    //         return; // 여기서 바로 리턴하여 UI 활성화 차단
    //     }

    //     if (diceGroup != null)
    //     {
    //         if (show)
    //         {
    //             Debug.Log("주사위 UI 활성화 시작");

    //             // 카메라 참조 확보
    //             if (cameraTransform == null)
    //             {
    //                 FindCameraTransform();
    //             }

    //             if (cameraTransform != null)
    //             {
    //                 Debug.Log($"카메라 위치: {cameraTransform.position}");

    //                 if (PlayerManager.Instance != null)
    //                 {
    //                     Vector3 playerPos = PlayerManager.Instance.GetPlayerPosition();
    //                     Debug.Log($"플레이어 위치: {playerPos}");
    //                 }

    //                 PositionUIInFrontOfCamera(diceGroup.transform, diceUIDistance, diceUIHeightOffset);
    //                 diceGroup.SetActive(true);

    //                 Debug.Log($"주사위 UI 최종 위치: {diceGroup.transform.position}");
    //                 Debug.Log("주사위 UI 활성화 완료");
    //             }
    //             else
    //             {
    //                 Debug.LogError("카메라를 찾을 수 없어서 UI 배치 불가능!");
    //                 diceGroup.SetActive(true);
    //             }
    //         }
    //         else
    //         {
    //             diceGroup.SetActive(false);
    //             Debug.Log("주사위 UI 비활성화");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("diceGroup이 null입니다!");
    //     }
    // }

    // // 미션 UI 처리
    // public void ShowMissionPrompt(bool show)
    // {
    //     if (missionPromptGroup != null)
    //     {
    //         missionPromptGroup.SetActive(show);

    //         if (show && cameraTransform != null)
    //         {
    //             PositionUIInFrontOfCamera(missionPromptGroup.transform, missionUIDistance, missionUIHeightOffset);
    //         }
    //     }
    // }
    public void ShowDiceUI(bool show)
    {
        if (!show)
        {
            diceGroup.SetActive(false);
            return;
        }

        // 간단한 차단 로직
        if (DiceManager.Instance?.IsDiceSceneLoaded() == true) return;
        if (isInMission) return;

        // 카메라 찾기 (한 번만)
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform ?? FindObjectOfType<Camera>()?.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("카메라를 찾을 수 없습니다!");
            return;
        }

        // 플레이어 앞 2미터, 위로 0.5미터 위치에 배치
        Vector3 targetPos = cameraTransform.position 
                        + cameraTransform.forward * 7f 
                        + Vector3.up * 0.5f;
        
        diceGroup.transform.position = targetPos;
        Vector3 lookDirection = targetPos - cameraTransform.position;
        diceGroup.transform.rotation = Quaternion.LookRotation(lookDirection);
        diceGroup.SetActive(true);
    }

    // ShowMissionPrompt 메소드도 수정해서 확실하게 차단
    public void ShowMissionPrompt(bool show)
    {
        if (missionPromptGroup != null)
        {
            missionPromptGroup.SetActive(show);

            if (show)
            {
                // 미션 프롬프트를 표시할 때 주사위 UI 강제로 숨김
                if (diceGroup != null)
                {
                    diceGroup.SetActive(false);
                    Debug.Log("미션 프롬프트 표시로 인해 주사위 UI 숨김");
                }
                
                if (cameraTransform != null)
                {
                    PositionUIInFrontOfCamera(missionPromptGroup.transform, missionUIDistance, missionUIHeightOffset);
                }
            }
        }
    }

    // 코인 UI 처리
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
        StartCoroutine(HideInsufficientCoinsMessageAfterDelay(3f));
    }

    private void ShowInsufficientCoinsMessage(bool show)
    {
        if (insufficientCoinsMessage != null)
        {
            insufficientCoinsMessage.SetActive(show);
            
            if (show && cameraTransform != null)
            {
                Vector3 targetPos = cameraTransform.position
                    + cameraTransform.forward * missionUIDistance
                    + Vector3.up * (missionUIHeightOffset + 0.3f);
                    
                insufficientCoinsMessage.transform.position = targetPos;
                insufficientCoinsMessage.transform.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
            }
        }
    }

    private System.Collections.IEnumerator HideInsufficientCoinsMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowInsufficientCoinsMessage(false);
    }

    // SpellBook UI 처리
    public void ShowSpellBookUI(bool show)
    {
        if (spellBookCanvas != null)
        {
            spellBookCanvas.SetActive(show);
            
            if (show)
            {
                Canvas canvas = spellBookCanvas.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = FindCameraComponent();
                    canvas.sortingOrder = 10;
                    
                    RectTransform canvasRect = spellBookCanvas.GetComponent<RectTransform>();
                    if (canvasRect != null)
                    {
                        canvasRect.localScale = Vector3.one * 0.01f;
                    }
                }
                
                PositionUIInFrontOfCamera(spellBookCanvas.transform, spellBookUIDistance, spellBookUIHeightOffset);
            }
        }
    }

    public void ShowSpellBookResult(string resultText)
    {
        if (spellBookResultPanel != null)
        {
            spellBookResultPanel.SetActive(true);
        }
        
        if (spellBookResultText != null)
        {
            spellBookResultText.gameObject.SetActive(true);
            spellBookResultText.text = resultText;
            
            Canvas textCanvas = spellBookResultText.GetComponentInParent<Canvas>();
            if (textCanvas != null)
            {
                textCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                textCanvas.sortingOrder = 2000;
            }
            
            RectTransform textRect = spellBookResultText.rectTransform;
            textRect.sizeDelta = new Vector2(400, 100);
            textRect.localScale = Vector3.one;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            
            spellBookResultText.fontSize = 48;
            spellBookResultText.color = Color.yellow;
            spellBookResultText.fontStyle = FontStyles.Bold;
        }
        
        if (spellBookAirplanePanel != null)
        {
            spellBookAirplanePanel.SetActive(false);
        }
    }

    public void ShowSpellBookAirplanePanel()
    {
        if (spellBookAirplanePanel != null)
        {
            spellBookAirplanePanel.SetActive(true);

            Canvas canvas = spellBookAirplanePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main ?? FindObjectOfType<Camera>();
                canvas.sortingOrder = 1000;

                if (cameraTransform != null)
                {
                    float airplanePanelDistance = 1.2f; // 원하는 거리
                    Vector3 targetPos = cameraTransform.position + cameraTransform.forward * airplanePanelDistance;
                    // y축을 카메라 높이와 동일하게 맞춤
                    targetPos.y = cameraTransform.position.y;

                    canvas.transform.position = targetPos;
                    canvas.transform.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
                }

                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.localScale = Vector3.one * 0.01f;
                }
            }
        }
    }

    public void UpdateSpellBookTileButtons(bool[] tileStates, System.Action<int> onTileClicked)
    {
        for (int i = 0; i < spellBookTileButtons.Length && i < tileStates.Length; i++)
        {
            if (spellBookTileButtons[i] != null)
            {
                bool isOccupied = tileStates[i];
                spellBookTileButtons[i].interactable = !isOccupied;
                
                Image buttonImage = spellBookTileButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isOccupied ? Color.gray : Color.white;
                }
                
                int x = i / 3;
                int y = i % 3;
                TextMeshProUGUI buttonText = spellBookTileButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = GetTileDisplayName(BingoBoard.GetTileNameByCoords(x, y));
                }
                
                spellBookTileButtons[i].onClick.RemoveAllListeners();
                int buttonIndex = i;
                spellBookTileButtons[i].onClick.AddListener(() => onTileClicked(buttonIndex));
            }
        }
    }

    private string GetTileDisplayName(string tileName)
    {
        switch(tileName)
        {
            case "Netherlands": return "네덜란드";
            case "Germany": return "독일";
            case "USA": return "미국";
            case "SpellBook": return "마법서";
            case "Japan": return "일본";
            case "Seoul": return "서울";
            case "Suncheon": return "순천";
            case "Egypt": return "이집트";
            default: return tileName;
        }
    }

    // 카메라 위치 찾기
    private void FindCameraTransform()
    {
        if (cameraTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                Camera playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    cameraTransform = playerCamera.transform;
                    Debug.Log("Player 카메라 자동 연결됨");
                    return;
                }
            }
            
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                Debug.Log("메인 카메라 자동 연결됨");
                return;
            }
            
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                cameraTransform = cameras[0].transform;
                Debug.Log("첫 번째 카메라 자동 연결됨");
                return;
            }
            
            Debug.LogError("카메라를 찾을 수 없습니다!");
        }
    }
    
    private void PositionUIInFrontOfCamera(Transform uiTransform, float distance, float heightOffset)
    {
        if (cameraTransform == null)
        {
            FindCameraTransform();
            if (cameraTransform == null) return;
        }
        
        Vector3 targetPos = cameraTransform.position
            + cameraTransform.forward * distance
            + Vector3.up * heightOffset;
        
        uiTransform.position = targetPos;
        uiTransform.rotation = Quaternion.LookRotation(targetPos - cameraTransform.position);
    }
    
    private Camera FindCameraComponent()
    {
        if (cameraTransform != null)
        {
            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam != null) return cam;
        }
        
        return Camera.main ?? FindObjectOfType<Camera>();
    }

    // 미션 돌아가기 처리
    public static void ReturnFromMission()
    {
        Debug.Log("미션에서 돌아가기 요청됨");
        Time.timeScale = 1f;
        
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.ReturnFromMission();
        }
        else
        {
            Debug.LogError("MissionManager.Instance를 찾을 수 없습니다!");
        }
    }

    public void OnMissionReturnButtonClicked()
    {
        ReturnFromMission();
    }
}