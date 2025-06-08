using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject backButton;
    [SerializeField] private Button diceButton;  
    [SerializeField] public GameObject diceButtonCanvas;

    private DiceSceneLoader sceneLoader;

    public static DiceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        sceneLoader = FindObjectOfType<DiceSceneLoader>();
    }

    private void Start()
    {
        if (diceButton == null)
        {
            GameObject found = GameObject.Find("DiceButton");
            if (found != null)
                diceButton = found.GetComponent<Button>();
        }

        if (diceButton != null)
        {
            diceButton.onClick.RemoveAllListeners();
            diceButton.onClick.AddListener(DiceButtonClicked);
        }

        if (backButton != null)
            backButton.SetActive(false);
    }

    public void DiceButtonClicked()
    {
        StartCoroutine(LoadDiceScene());
    }

    private IEnumerator LoadDiceScene()
    {
        if (sceneLoader != null)
            yield return sceneLoader.LoadDiceScene();

        // ⬇️ 여기에 추가
        DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
        if (sceneManager != null)
        {
            sceneManager.ResetDice(); // 주사위 초기화
            PlayerManager pm = FindObjectOfType<PlayerManager>();
            if (pm != null)
            {
                sceneManager.playerManager = pm;
                sceneManager.AlignSceneToPlayer();
            }
        }

        SetupBackButtonUI();

        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    private void SetupBackButtonUI()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform backButtonTransform = canvasObj.transform.Find("BackButton");
            if (backButtonTransform != null)
            {
                backButton = backButtonTransform.gameObject;
                backButton.SetActive(true);

                Button btn = backButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnBackButtonClicked(true));
                }
            }
        }
    }

    public void OnBackButtonClicked(bool showButtonAfter = true)
    {
        StartCoroutine(UnloadDiceScene(showButtonAfter));
    }

    private IEnumerator UnloadDiceScene(bool showButtonAfter)
    {
        if (sceneLoader != null)
            yield return sceneLoader.UnloadDiceScene();

        if (showButtonAfter)
            SetDiceButtonVisible(true);

        backButton = null;
    }

    public void SetDiceButtonVisible(bool visible)
    {
        if (diceButtonCanvas != null)
            diceButtonCanvas.SetActive(visible);

        if (diceButton != null)
            diceButton.gameObject.SetActive(visible);
    }
}
