using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class DiceResultUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject resultPanel;           
    public TextMeshProUGUI resultNumberText;
    public TextMeshProUGUI resultMessageText;
    public Button backButton;               
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;     // 나타나는 시간
    public float displayDuration = 3.0f;    // 표시 유지 시간
    
    private System.Action onResultDisplayComplete;
    private Coroutine displayCoroutine;
    
    private void Start()
    {      
        // 시작할 때 패널 숨기기
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    // 굴리기 전 UI
    public void ShowCustomMessage(string message)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultMessageText != null)
            resultMessageText.text = message;

        if (resultNumberText != null)
            resultNumberText.text = ""; // 숫자 없음
    }

    public void ShowResult(int diceNumber)
    {
        ShowResult(diceNumber, null);
    }
    
    public void ShowResult(int diceNumber, System.Action onComplete)
    {
        onResultDisplayComplete = onComplete;
        
        // 이전 표시가 진행 중이면 멈추기
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceNumber, Color.white));
    }

    private IEnumerator DisplayResultCoroutine(int diceNumber, Color numberColor)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            yield return StartCoroutine(FadeInAnimation());
        }
        else
        {
            Debug.LogError("resultPanel이 null입니다!");
            yield break; 
        }
        
        resultNumberText.text = diceNumber.ToString();
        resultNumberText.color = numberColor; 
        
        if (resultMessageText != null)
        {
            resultMessageText.text = "Dice Result:";
        }
        
        yield return new WaitForSeconds(0.5f);
        
        onResultDisplayComplete?.Invoke();

        yield return new WaitForSeconds(displayDuration - 0.5f);
    }
    
    private IEnumerator FadeInAnimation()
    {
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null; 
        }
        canvasGroup.alpha = 1f;
    }
    
    // private IEnumerator FadeOutAnimation()
    // {
    //     // 페이드 아웃 애니메이션 
    //     CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
    //     if (canvasGroup == null) yield break;
        
    //     float elapsedTime = 0f;
    //     while (elapsedTime < fadeInDuration)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
    //         yield return null;
    //     }
    //     canvasGroup.alpha = 0f;
    //     resultPanel.SetActive(false);
    // }
    
    private void OnBackClicked()
    {
        // 뒤로가기 버튼 클릭시 DiceManager의 뒤로가기 함수 호출
        FindObjectOfType<DiceManager>()?.OnBackButtonClicked();
    }
    
    // // 수동으로 패널을 숨기는 함수
    // public void HideResult()
    // {
    //     if (displayCoroutine != null)
    //     {
    //         StopCoroutine(displayCoroutine);
    //         displayCoroutine = null;
    //     }
        
    //     if (resultPanel != null)
    //     {
    //         StartCoroutine(FadeOutAnimation());
    //     }
    // }
}