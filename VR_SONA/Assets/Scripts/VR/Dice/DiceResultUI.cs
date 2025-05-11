using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

// DiceResultUI.cs - DiceScene 내부의 결과 표시 UI를 관리하는 스크립트
public class DiceResultUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public GameObject resultPanel;           // 결과를 표시할 패널
    public TextMeshProUGUI resultNumberText;
    public TextMeshProUGUI resultMessageText;
    public Button replayButton;             // 다시 굴리기 버튼
    public Button backButton;               // 뒤로가기 버튼
    
    [Header("애니메이션 설정")]
    public float fadeInDuration = 0.5f;     // 나타나는 시간
    public float displayDuration = 5.0f;    // 표시 유지 시간 (0이면 무한)
    
    [Header("색상 설정")]
    public Color defaultColor = Color.white;
    public Color highRollColor = Color.green;   // 높은 숫자일 때 색상
    public Color lowRollColor = Color.red;      // 낮은 숫자일 때 색상
    
    private Coroutine displayCoroutine;
    
    private void Start()
    {
        // 시작할 때 패널 숨기기
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 버튼 이벤트 연결
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    // 주사위 결과를 화면에 표시하는 메서드
    public void ShowResult(int diceNumber)
    {
        Debug.Log($"ShowResult 호출됨 - 숫자: {diceNumber}");
        
        // 이전 표시가 진행 중이면 멈추기
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            Debug.Log("이전 디스플레이 코루틴 중지");
        }
        
        // 결과 표시 시작
        displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceNumber));
        Debug.Log("새로운 결과 표시 코루틴 시작");
    }

    private IEnumerator DisplayResultCoroutine(int diceNumber)
    {
        Debug.Log($"DisplayResultCoroutine 시작 - 숫자: {diceNumber}");
        
        // 패널 활성화
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            Debug.Log("결과 패널 활성화됨");
        }
        else
        {
            Debug.LogError("resultPanel이 null입니다!");
            yield break;
        }
        
        // 결과 텍스트 설정
        if (resultNumberText != null)
        {
            resultNumberText.text = diceNumber.ToString();
            resultNumberText.color = GetNumberColor(diceNumber);
            Debug.Log($"결과 텍스트 설정: {diceNumber}");
        }
        else
        {
            Debug.LogError("resultNumberText가 null입니다!");
        }
        
        if (resultMessageText != null)
        {
            resultMessageText.text = "주사위 결과";
            Debug.Log("결과 메시지 텍스트 설정");
        }
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
    
    private Color GetNumberColor(int number)
    {
        // 팔면체 주사위의 경우 8이 최대값
        if (number >= 7)
        {
            return highRollColor;  // 높은 숫자 (7, 8)
        }
        else if (number <= 2)
        {
            return lowRollColor;   // 낮은 숫자 (1, 2)
        }
        else
        {
            return defaultColor;   // 중간 숫자 (3~6)
        }
    }
    
    private void OnReplayClicked()
    {
        // 다시 굴리기 버튼이 클릭되었을 때
        // 결과 패널 숨기기
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        resultPanel.SetActive(false);
        
        // 주사위를 초기 위치로 리셋하는 로직 호출
        FindObjectOfType<DiceSceneManager>()?.ResetDice();
    }
    
    private void OnBackClicked()
    {
        // 뒤로가기 버튼이 클릭되었을 때
        // DiceManager의 뒤로가기 기능 호출
        FindObjectOfType<DiceManager>()?.OnBackButtonClicked();
    }
}
