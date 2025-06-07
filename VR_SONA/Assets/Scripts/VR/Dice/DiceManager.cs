// // DiceManager.cs
// using System.Collections;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// public class DiceManager : MonoBehaviour
// {
//     public Transform playerTransform;
//     public GameObject backButton;
//     [SerializeField] private Button diceButton;  
//     [SerializeField] public GameObject diceButtonCanvas;
//     public static DiceManager Instance { get; private set; }

//     private void Awake()
//     {
//         if (Instance == null)
//             Instance = this;
//         else
//             Destroy(gameObject);
//     }

//     private void Start()
//     {
//         if (diceButton == null)
//         {
//             GameObject found = GameObject.Find("DiceButton");
//             if (found != null)
//                 diceButton = found.GetComponent<Button>();
//         }

//         if (diceButton != null)
//         {
//             diceButton.onClick.RemoveAllListeners();
//             diceButton.onClick.AddListener(DiceButtonClicked);
//         }

//         if (backButton != null)
//             backButton.SetActive(false);
//     }

//     public void DiceButtonClicked()
//     {
//         StartCoroutine(LoadDiceScene());
//     }

//     private IEnumerator LoadDiceScene()
//     {
//         var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
//         yield return new WaitUntil(() => asyncLoad.isDone);

//         PlayerManager pm = FindObjectOfType<PlayerManager>();
//         DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
//         if (sceneManager != null && pm != null)
//         {
//             sceneManager.playerManager = pm;
//             sceneManager.AlignSceneToPlayer();
//         }

//         GameObject canvasObj = GameObject.Find("Canvas");
//         if (canvasObj != null)
//         {
//             Transform backButtonTransform = canvasObj.transform.Find("BackButton");
//             if (backButtonTransform != null)
//             {
//                 backButton = backButtonTransform.gameObject;
//                 backButton.SetActive(true);

//                 Button backButtonComponent = backButton.GetComponent<Button>();
//                 if (backButtonComponent != null)
//                 {
//                     backButtonComponent.onClick.RemoveAllListeners();
//                     backButtonComponent.onClick.AddListener(OnBackButtonClicked);
//                 }
//             }
//         }

//         if (diceButton != null)
//             diceButton.gameObject.SetActive(false);
//     }

//     public void OnBackButtonClicked()
//     {
//         StartCoroutine(UnloadDiceScene());
//     }

//     private IEnumerator UnloadDiceScene()
//     {
//         Scene diceScene = SceneManager.GetSceneByName("DiceScene");

//         if (!diceScene.IsValid() || !diceScene.isLoaded)
//         {
//             if (diceButton != null)
//                 diceButton.gameObject.SetActive(true);
//             backButton = null;
//             yield break;
//         }

//         var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
//         yield return new WaitUntil(() => asyncUnload.isDone);

//         if (diceButton != null)
//             diceButton.gameObject.SetActive(true);
//         backButton = null;
//     }

//     public void SetDiceButtonVisible(bool visible)
//     {
//         if (diceButtonCanvas != null)
//         {
//             Debug.Log($"[DiceManager] Canvas {(visible ? "활성화" : "비활성화")}");
//             diceButtonCanvas.SetActive(visible);
//         }
//         else
//         {
//             Debug.LogWarning("diceButtonCanvas가 null입니다!");
//         }
//     }
// }

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public GameObject backButton;
    
    [SerializeField] private Button diceButton;               // 버튼 클릭용
    [SerializeField] public GameObject diceButtonCanvas;     // 버튼 UI 전체 제어용

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
        // 버튼 자동 할당 시도
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

        SetDiceButtonVisible(true);  // 초기에는 버튼 보이게
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
                    backButtonComponent.onClick.AddListener(OnBackButtonClicked);
                }
            }
        }

        SetDiceButtonVisible(false); // ✅ DiceScene 진입 시 버튼 숨김
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
            SetDiceButtonVisible(true);
            backButton = null;
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);

        SetDiceButtonVisible(true);  // ✅ DiceScene 종료 시 버튼 다시 보임
        backButton = null;
    }

    /// <summary>
    /// DiceButton UI 전체를 보이거나 숨깁니다 (Canvas + Button)
    /// </summary>
    public void SetDiceButtonVisible(bool visible)
    {
        if (diceButtonCanvas != null)
            diceButtonCanvas.SetActive(visible);

        if (diceButton != null)
            diceButton.gameObject.SetActive(visible);

        Debug.Log($"[DiceManager] DiceButton 전체 {(visible ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 버튼을 클릭 불가 상태로 만들고 싶을 때 사용할 수 있음 (선택적 기능)
    /// </summary>
    public void SetDiceButtonInteractable(bool interactable)
    {
        if (diceButton != null)
            diceButton.interactable = interactable;
    }
}
