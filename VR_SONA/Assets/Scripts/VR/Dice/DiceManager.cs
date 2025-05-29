using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject backButton;
    public Button diceButton;

    private void Start()
    {
        // 버튼 연결
        if (diceButton == null)
        {
            GameObject found = GameObject.Find("DiceButton");
            if (found != null)
                diceButton = found.GetComponent<Button>();
        }

        if (diceButton != null)
        {
            diceButton.onClick.RemoveAllListeners();
            diceButton.onClick.AddListener(DiceButton_clicked);
        }
        else
        {
            Debug.LogWarning("⚠ DiceButton 못 찾음");
        }

        if (backButton != null)
            backButton.SetActive(false);
    }

    public void DiceButton_clicked()
    {
        StartCoroutine(LoadDiceScene());
    }

    private IEnumerator LoadDiceScene()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        // DiceSceneManager에 PlayerManager 할당
        PlayerManager pm = FindObjectOfType<PlayerManager>();
        Debug.Log("PlayerManager: " + (pm != null ? "찾음" : "못 찾음"));

        DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
        if (sceneManager != null)
        {
            sceneManager.playerManager = pm;
            sceneManager.AlignSceneToPlayer(); 
        }

        // UI 연결
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
                    backButtonComponent.onClick.AddListener(OnBackButtonClicked);
                }
            }
        }

        // Dice 버튼 비활성화
        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        StartCoroutine(UnloadDiceScene());
    }

    private IEnumerator UnloadDiceScene()
    {
        Scene diceScene = SceneManager.GetSceneByName("DiceScene");

        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            diceButton?.gameObject.SetActive(true);
            backButton = null;
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);

        diceButton?.gameObject.SetActive(true);
        backButton = null;
    }
}
