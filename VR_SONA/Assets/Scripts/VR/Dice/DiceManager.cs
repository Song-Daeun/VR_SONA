// DiceManager.cs
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
    public static DiceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        PlayerManager pm = FindObjectOfType<PlayerManager>();
        DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
        if (sceneManager != null && pm != null)
        {
            sceneManager.playerManager = pm;
            sceneManager.AlignSceneToPlayer();
        }

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform backButtonTransform = canvasObj.transform.Find("BackButton");
            if (backButtonTransform != null)
            {
                backButton = backButtonTransform.gameObject;
                backButton.SetActive(true);

                Button backButtonComponent = backButton.GetComponent<Button>();
                if (backButtonComponent != null)
                {
                    backButtonComponent.onClick.RemoveAllListeners();
                    // backButtonComponent.onClick.AddListener(OnBackButtonClicked);
                    backButtonComponent.onClick.AddListener(() => OnBackButtonClicked(true));
                }
            }
        }

        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    // public void OnBackButtonClicked()
    // {
    //     StartCoroutine(UnloadDiceScene());
    // }

    // private IEnumerator UnloadDiceScene()
    // {
    //     Scene diceScene = SceneManager.GetSceneByName("DiceScene");

    //     if (!diceScene.IsValid() || !diceScene.isLoaded)
    //     {
    //         if (diceButton != null)
    //             diceButton.gameObject.SetActive(true);
    //         backButton = null;
    //         yield break;
    //     }

    //     var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
    //     yield return new WaitUntil(() => asyncUnload.isDone);

    //     if (diceButton != null)
    //         diceButton.gameObject.SetActive(true);
    //     backButton = null;
    // }
    // 수정
    public void OnBackButtonClicked(bool showButtonAfter = true)
    {
        StartCoroutine(UnloadDiceScene(showButtonAfter));
    }

    private IEnumerator UnloadDiceScene(bool showButtonAfter)
    {
        Scene diceScene = SceneManager.GetSceneByName("DiceScene");

        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            if (showButtonAfter)
                SetDiceButtonVisible(true);
            backButton = null;
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);

        if (showButtonAfter)
            SetDiceButtonVisible(true); 
        backButton = null;
    }

    public void SetDiceButtonVisible(bool visible)
    {
        if (diceButtonCanvas != null)
        {
            Debug.Log($"[DiceManager] Canvas {(visible ? "활성화" : "비활성화")}");
            diceButtonCanvas.SetActive(visible);
        }

        if (diceButton != null)
        {
            diceButton.gameObject.SetActive(visible);
        }
        else
        {
            Debug.LogWarning("diceButtonCanvas가 null입니다!");
        }
    }
}

