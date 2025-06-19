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
        ConnectDiceButtonToDiceManager();
        ConnectMissionButtons();
        StartCoroutine(WaitForPlayerAndInitializeUI());
        FindCameraTransform();
    }

    // 플레이어가 준비된 후 UI 초기화
    private IEnumerator WaitForPlayerAndInitializeUI()
    {
        while (PlayerManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (PlayerManager.Instance.IsMoving())
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.2f);

        SetupUIAfterPlayerReady();
    }

    // 플레이어 준비 완료 후 UI 설정
    private void SetupUIAfterPlayerReady()
    {
        if (cameraTransform == null)
        {
            FindCameraTransform();
        }
        SetInitialUIStates();
    }

    // 주사위 버튼을 DiceManager에 연결
    private void ConnectDiceButtonToDiceManager()
    {
        if (diceButton != null)
        {
            diceButton.onClick.AddListener(OnDiceButtonClicked);
        }
        else
        {
            Debug.LogError("주사위 버튼을 찾을 수 없습니다!");
        }
    }

    private void OnDiceButtonClicked()
    {
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

        ShowMissionPrompt(false);
        GameManager.Instance?.OnMissionDecisionMade(true);
    }

    private void OnNoClicked()
    {
        isInMission = false; 

        ShowMissionPrompt(false);
        ShowDiceUI(true);

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

    public void ResetMissionState()
    {
        isInMission = false;
        diceUIWasActiveBeforeMission = false;
    }

    public void ShowDiceUI(bool show)
    {
        if (!show)
        {
            if (diceGroup != null)
            {
                diceGroup.SetActive(false);
            }
            return;
        }

        if (DiceManager.Instance?.IsDiceSceneLoaded() == true)
        {
            return;
        }
        
        if (isInMission)
        {
            return;
        }

        // diceGroup 존재 확인
        if (diceGroup == null)
        {
            return;
        }

        // 카메라 찾기
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform ?? FindObjectOfType<Camera>()?.transform;

        if (cameraTransform == null)
        {
            return;
        }

        // UI 위치 설정 및 활성화
        Vector3 targetPos = cameraTransform.position 
                        + cameraTransform.forward * 7f 
                        + Vector3.up * 0.5f;
        
        diceGroup.transform.position = targetPos;
        Vector3 lookDirection = targetPos - cameraTransform.position;
        diceGroup.transform.rotation = Quaternion.LookRotation(lookDirection);
        diceGroup.SetActive(true);
    }

    public void ShowMissionPrompt(bool show)
    {
        if (missionPromptGroup != null)
        {
            missionPromptGroup.SetActive(show);

            if (show)
            {
                if (diceGroup != null)
                {
                    diceGroup.SetActive(false);
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
                    float airplanePanelDistance = 0.5f; 
                    Vector3 targetPos = cameraTransform.position + cameraTransform.forward * airplanePanelDistance;
                    targetPos.y = cameraTransform.position.y;

                    canvas.transform.position = targetPos;
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
                    return;
                }
            }
            
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                return;
            }
            
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                cameraTransform = cameras[0].transform;
                return;
            }
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