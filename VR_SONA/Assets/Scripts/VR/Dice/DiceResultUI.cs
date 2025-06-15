// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro; 

// public class DiceResultUI : MonoBehaviour
// {
//     [Header("UI Components")]
//     public GameObject resultPanel;           
//     public TextMeshProUGUI resultNumberText;
//     public TextMeshProUGUI resultMessageText;
//     public Button backButton;               
    
//     [Header("Animation Settings")]
//     public float fadeInDuration = 0.5f;     // 나타나는 시간
//     public float displayDuration = 3.0f;    // 표시 유지 시간
    
//     private System.Action onResultDisplayComplete;
//     private Coroutine displayCoroutine;
    
//     private void Start()
//     {      
//         // 시작할 때 패널 숨기기
//         if (resultPanel != null)
//         {
//             resultPanel.SetActive(false);
//         }
        
//         // if (backButton != null)
//         // {
//         //     backButton.onClick.AddListener(OnBackClicked);
//         // }
//     }

//     // 굴리기 전 UI
//     public void ShowCustomMessage(string message)
//     {
//         if (resultPanel != null)
//             resultPanel.SetActive(true);

//         if (resultMessageText != null)
//             resultMessageText.text = message;

//         if (resultNumberText != null)
//             resultNumberText.text = "";
//     }

//     public void ShowResult(int diceNumber)
//     {
//         ShowResult(diceNumber, null);
//     }
    
//     public void ShowResult(int diceNumber, System.Action onComplete)
//     {
//         onResultDisplayComplete = onComplete;
        
//         // 이전 표시가 진행 중이면 멈추기
//         if (displayCoroutine != null)
//         {
//             StopCoroutine(displayCoroutine);
//         }
//         displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceNumber, Color.white));
//     }

//     private IEnumerator DisplayResultCoroutine(int diceNumber, Color numberColor)
//     {
//         if (resultPanel != null)
//         {
//             resultPanel.SetActive(true);
//             yield return StartCoroutine(FadeInAnimation());
//         }
//         else
//         {
//             Debug.LogError("resultPanel이 null입니다!");
//             yield break; 
//         }
        
//         resultNumberText.text = diceNumber.ToString();
//         resultNumberText.color = numberColor; 
        
//         if (resultMessageText != null)
//         {
//             resultMessageText.text = "결과 :";
//         }
        
//         yield return new WaitForSeconds(0.5f);
        
//         onResultDisplayComplete?.Invoke();

//         yield return new WaitForSeconds(displayDuration - 0.5f);
//     }
    
//     private IEnumerator FadeInAnimation()
//     {
//         CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
//         if (canvasGroup == null)
//         {
//             canvasGroup = resultPanel.AddComponent<CanvasGroup>();
//         }
        
//         canvasGroup.alpha = 0f;
//         float elapsedTime = 0f;
        
//         while (elapsedTime < fadeInDuration)
//         {
//             elapsedTime += Time.deltaTime;
//             canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
//             yield return null; 
//         }
//         canvasGroup.alpha = 1f;
//     }
    
//     // private IEnumerator FadeOutAnimation()
//     // {
//     //     // 페이드 아웃 애니메이션 
//     //     CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
//     //     if (canvasGroup == null) yield break;
        
//     //     float elapsedTime = 0f;
//     //     while (elapsedTime < fadeInDuration)
//     //     {
//     //         elapsedTime += Time.deltaTime;
//     //         canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
//     //         yield return null;
//     //     }
//     //     canvasGroup.alpha = 0f;
//     //     resultPanel.SetActive(false);
//     // }
    
//     // private void OnBackClicked()
//     // {
//     //     // 뒤로가기 버튼 클릭시 DiceManager의 뒤로가기 함수 호출
//     //     FindObjectOfType<DiceManager>()?.OnBackButtonClicked();
//     // }
    
//     // // 수동으로 패널을 숨기는 함수
//     // public void HideResult()
//     // {
//     //     if (displayCoroutine != null)
//     //     {
//     //         StopCoroutine(displayCoroutine);
//     //         displayCoroutine = null;
//     //     }
        
//     //     if (resultPanel != null)
//     //     {
//     //         StartCoroutine(FadeOutAnimation());
//     //     }
//     // }
// }

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
        // 시작할 때 패널 숨기기
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 텍스트 색상 초기화
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
            resultMessageText.color = messageTextColor; // 색상 설정 추가
        }

        if (resultNumberText != null)
        {
            resultNumberText.text = "";
            resultNumberText.color = numberTextColor; // 색상 설정 추가
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
            Debug.LogError("resultPanel이 null입니다!");
            yield break; 
        }
        
        // 주사위 번호 텍스트 설정
        if (resultNumberText != null)
        {
            resultNumberText.text = diceNumber.ToString();
            resultNumberText.color = numberTextColor;
        }
        
        // 메시지 텍스트 설정
        if (resultMessageText != null)
        {
            resultMessageText.text = "결과 :";
            resultMessageText.color = messageTextColor; // 색상 설정 추가
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
    
    // 색상을 런타임에 변경할 수 있는 메서드들
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
    
    // 두 텍스트 모두 같은 색상으로 설정
    public void SetAllTextColor(Color color)
    {
        SetNumberTextColor(color);
        SetMessageTextColor(color);
    }
}