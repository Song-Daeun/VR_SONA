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
    public float displayDuration = 3.0f;    // 표시 유지 시간 (줄였습니다)
    
    // 새로추가: 결과 표시 완료 후 실행될 콜백 함수
    private System.Action onResultDisplayComplete;
    private Coroutine displayCoroutine;
    
    private void Start()
    {
        Debug.Log("DiceResultUI Start called");
        
        // 시작할 때 패널 숨기기
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 뒤로가기 버튼 이벤트 연결
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    // 기존 메서드: 콜백 없이 결과만 표시
    public void ShowResult(int diceNumber)
    {
        ShowResult(diceNumber, null);
    }
    
    // 새로운 메서드: 콜백 함수와 함께 결과 표시
    public void ShowResult(int diceNumber, System.Action onComplete)
    {
        // 콜백 함수 저장 - 이 함수는 UI 표시가 완료된 후 호출됩니다
        onResultDisplayComplete = onComplete;
        
        Debug.Log($"주사위 결과 UI 표시 시작: {diceNumber}");
        
        // 이전 표시가 진행 중이면 멈추기
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        // 결과 표시 코루틴 시작
        displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceNumber, Color.white));
    }

    private IEnumerator DisplayResultCoroutine(int diceNumber, Color numberColor)
    {
        // 1단계: 패널 활성화 및 페이드인
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            
            // 페이드인 애니메이션 실행
            yield return StartCoroutine(FadeInAnimation());
        }
        else
        {
            Debug.LogError("resultPanel이 null입니다!");
            yield break; 
        }
        
        // 2단계: 결과 텍스트 설정
        if (resultNumberText != null)
        {
            resultNumberText.text = diceNumber.ToString();
            resultNumberText.color = numberColor; 
            Debug.Log($"결과 숫자 텍스트 설정 완료: {diceNumber}");
        }
        else
        {
            Debug.LogError("resultNumberText가 null입니다!");
        }
        
        if (resultMessageText != null)
        {
            resultMessageText.text = "Dice Result :";
        }
        
        // 3단계: UI가 완전히 표시된 후 약간의 대기
        yield return new WaitForSeconds(0.5f);
        
        // 4단계: 콜백 함수 실행 - 여기서 플레이어 이동이 시작됩니다!
        Debug.Log("UI 표시 완료, 콜백 함수 실행");
        onResultDisplayComplete?.Invoke();
        
        // 5단계: 나머지 표시 시간 대기
        yield return new WaitForSeconds(displayDuration - 0.5f);
        
        Debug.Log("결과 표시 시간 완료");
    }
    
    private IEnumerator FadeInAnimation()
    {
        // CanvasGroup 컴포넌트로 투명도 제어
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // CanvasGroup이 없다면 추가
            canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        }
        
        // 페이드인 시작: 완전히 투명한 상태에서 시작
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        // fadeInDuration 시간 동안 서서히 불투명해지도록 애니메이션
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp 대신 간단한 계산으로 투명도 조절
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null; // 다음 프레임까지 대기
        }
        
        // 애니메이션 완료: 완전히 불투명하게 설정
        canvasGroup.alpha = 1f;
        Debug.Log("페이드인 애니메이션 완료");
    }
    
    private IEnumerator FadeOutAnimation()
    {
        // 페이드 아웃 애니메이션 (필요시 사용)
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // 1에서 0으로 서서히 투명해지도록
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
            yield return null;
        }
        
        // 완전히 투명해진 후 패널 비활성화
        canvasGroup.alpha = 0f;
        resultPanel.SetActive(false);
        Debug.Log("페이드아웃 완료, 패널 비활성화");
    }
    
    private void OnBackClicked()
    {
        // 뒤로가기 버튼 클릭시 DiceManager의 뒤로가기 함수 호출
        Debug.Log("뒤로가기 버튼 클릭됨");
        FindObjectOfType<DiceManager>()?.OnBackButtonClicked();
    }
    
    // 수동으로 패널을 숨기는 함수 (필요시 사용)
    public void HideResult()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
        
        if (resultPanel != null)
        {
            StartCoroutine(FadeOutAnimation());
        }
    }
}