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
    // public Button replayButton;             // 다시 굴리기 버튼
    public Button backButton;               // 뒤로가기 버튼
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;     // 나타나는 시간
    public float displayDuration = 5.0f;    // 표시 유지 시간
    
    private Coroutine displayCoroutine;
    
    private void Start()
    {
        // 시작할 때 패널 숨기기
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 다시 굴리기 버튼 이벤트 연결
        // if (replayButton != null)
        // {
        //     replayButton.onClick.AddListener(OnReplayClicked);
        // }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    // 주사위 결과 표시
    public void ShowResult(int diceNumber)
    {        
        // 이전 표시가 진행 중이면 멈추기
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        // 결과 표시
        displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceNumber, Color.white));
    }

    private IEnumerator DisplayResultCoroutine(int diceNumber, Color numberColor)
    {
        // 패널 활성화
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        else
        {
            // resultpanel이 null값이면면 코루틴 종료
            yield break; 
        }
        
        // 결과 텍스트 설정
        if (resultNumberText != null)
        {
            resultNumberText.text = diceNumber.ToString();
            resultNumberText.color = numberColor; 
        }
        else
        {
            Debug.LogError("resultNumberText is null!");
        }
        
        if (resultMessageText != null)
        {
            resultMessageText.text = "Dice Result :";
        }
        yield return null;
    }
    
    private IEnumerator FadeInAnimation()
    {
        // 간단한 페이드 인 애니메이션
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOutAnimation()
    {
        // 페이드 아웃 애니메이션
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
            yield return null;
        }
        
        resultPanel.SetActive(false);
    }
    
    // private void OnReplayClicked()
    // {
    //     // 다시 굴리기 버튼이 클릭되었을 때
    //     // 결과 패널 숨기기
    //     if (displayCoroutine != null)
    //     {
    //         StopCoroutine(displayCoroutine);
    //     }
        
    //     resultPanel.SetActive(false);
        
    //     // 주사위를 초기 위치로 리셋하는 로직 호출
    //     FindObjectOfType<DiceSceneManager>()?.ResetDice();
    // }
    
    private void OnBackClicked()
    {
        FindObjectOfType<DiceManager>()?.OnBackButtonClicked();
    }
}
