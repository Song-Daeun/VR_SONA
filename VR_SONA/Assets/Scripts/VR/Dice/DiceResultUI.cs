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
    
    [Header("Text Colors")]
    public Color numberTextColor = Color.white;
    public Color messageTextColor = Color.white;
    
    private System.Action onResultDisplayComplete;
    private Coroutine displayCoroutine;
    
    private void Start()
    {      
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        InitializeTextColors();
    }

    private void InitializeTextColors()
    {
        if (resultNumberText != null)
        {
            resultNumberText.color = numberTextColor;
        }
        
        if (resultMessageText != null)
        {
            resultMessageText.color = messageTextColor;
        }
    }

    // 굴리기 전 UI
    public void ShowCustomMessage(string message)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultMessageText != null)
        {
            resultMessageText.text = message;
            resultMessageText.color = messageTextColor; 
        }

        if (resultNumberText != null)
        {
            resultNumberText.text = "";
            resultNumberText.color = numberTextColor; 
        }
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
        displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceNumber));
    }

    private IEnumerator DisplayResultCoroutine(int diceNumber)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            yield return StartCoroutine(FadeInAnimation());
        }
        else
        {
            yield break; 
        }

        if (resultNumberText != null)
        {
            resultNumberText.text = diceNumber.ToString();
            resultNumberText.color = numberTextColor;
        }

        if (resultMessageText != null)
        {
            resultMessageText.text = "결과 :";
            resultMessageText.color = messageTextColor;
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

    public void SetNumberTextColor(Color color)
    {
        numberTextColor = color;
        if (resultNumberText != null)
        {
            resultNumberText.color = color;
        }
    }
    
    public void SetMessageTextColor(Color color)
    {
        messageTextColor = color;
        if (resultMessageText != null)
        {
            resultMessageText.color = color;
        }
    }
    
    public void SetAllTextColor(Color color)
    {
        SetNumberTextColor(color);
        SetMessageTextColor(color);
    }
}