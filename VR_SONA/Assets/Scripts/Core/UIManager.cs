using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("Dice State Monitoring")]
    public float diceStateCheckInterval = 0.2f; // 주사위 상태 체크 주기
    
    private bool lastKnownDiceSceneState = false; // 마지막으로 알고 있던 주사위 씬 상태
    private Coroutine diceStateWatcher;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Dice UI")]
    public Button diceButton;
    public float diceUIDistance = 2f;
    public float diceUIHeightOffset = -0.5f;

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
        
        // 주사위 버튼을 DiceManager에 직접 연결
        ConnectDiceButtonToDiceManager();
        
        // 미션 버튼들은 GameManager에 연결
        ConnectMissionButtons();
        
        // 초기 UI 상태 설정
        SetInitialUIStates();
        
        // 카메라 자동 찾기
        FindCameraTransform();
        
        Debug.Log("UIManager 초기화 완료");
    }

    // 주사위 버튼을 DiceManager에 직접 연결
    private void ConnectDiceButtonToDiceManager()
    {
        if (diceButton != null)
        {
            // 주사위 버튼 클릭 시 DiceManager로 직접 이동
            diceButton.onClick.AddListener(OnDiceButtonClicked);
            Debug.Log("주사위 버튼이 DiceManager에 연결됨");
        }
        else
        {
            Debug.LogError("주사위 버튼을 찾을 수 없습니다!");
        }
    }

    // 주사위 버튼 연결 
    private void OnDiceButtonClicked()
    {
        Debug.Log("주사위 버튼 클릭 감지");
        
        if (DiceManager.Instance != null)
        {
            // DiceManager가 모든 주사위 처리를 담당하도록 위임
            DiceManager.Instance.LoadDiceScene();
        }
        else
        {
            Debug.LogError("DiceManager.Instance를 찾을 수 없습니다!");
        }
    }
    
    private void OnDestroy()
    {
        if (diceStateWatcher != null)
        {
            StopCoroutine(diceStateWatcher);
            diceStateWatcher = null;
        }
    }

    // 미션 버튼 연결 
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
        ShowMissionPrompt(false);
        GameManager.Instance?.OnMissionDecisionMade(true);
    }

    private void OnNoClicked()
    {
        ShowMissionPrompt(false);
        GameManager.Instance?.OnMissionDecisionMade(false);
    }

    // 초기 UI 상태 설정
    private void SetInitialUIStates()
    {
        ShowDiceUI(true);                    // 주사위 버튼 활성화
        ShowMissionPrompt(false);            // 미션 프롬프트 숨김
        ShowInsufficientCoinsMessage(false); // 코인 부족 메시지 숨김
        ShowSpellBookUI(false);              // 스펠북 UI 숨김
    }

    // 주사위 UI 관리 - 활성화/비활성화만 담당
    public void ShowDiceUI(bool show)
    {
        if (diceButton != null)
        {
            diceButton.gameObject.SetActive(show);
            Debug.Log("주사위 UI " + (show ? "활성화" : "비활성화"));
        }
    }

    // 미션 UI 처리
    public void ShowMissionPrompt(bool show)
    {
        if (missionPromptGroup != null)
        {
            missionPromptGroup.SetActive(show);

            if (show && cameraTransform != null)
            {
                PositionUIInFrontOfCamera(missionPromptGroup.transform, missionUIDistance, missionUIHeightOffset);
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
            if (spellBookResultText != null)
            {
                spellBookResultText.gameObject.SetActive(false);
            }
                
            spellBookAirplanePanel.SetActive(true);
            
            Canvas canvas = spellBookAirplanePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main ?? FindObjectOfType<Camera>();
                canvas.sortingOrder = 1000;
                
                if (cameraTransform != null)
                {
                    Vector3 targetPos = cameraTransform.position
                        + cameraTransform.forward * spellBookUIDistance
                        + Vector3.up * spellBookUIHeightOffset;
                    
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
                    buttonText.text = BingoBoard.GetTileNameByCoords(x, y);
                }
                
                spellBookTileButtons[i].onClick.RemoveAllListeners();
                int buttonIndex = i;
                spellBookTileButtons[i].onClick.AddListener(() => onTileClicked(buttonIndex));
            }
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