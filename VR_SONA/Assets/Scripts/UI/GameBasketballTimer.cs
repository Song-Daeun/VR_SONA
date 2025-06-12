using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameBasketballTimer : MonoBehaviour
{
    public float totalTime = 15f;
    private float currentTime;
    public Slider timerSlider;
    private bool isRunning = true;
    public static System.Action OnTimerExpired; // 시간 종료 이벤트

    void Start()
    {
        currentTime = totalTime;
        if (timerSlider != null)
        {
            timerSlider.maxValue = totalTime;
            timerSlider.value = totalTime;
        }
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0);
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }

        if (currentTime <= 0f)
        {
            isRunning = false;
            // BasGameManager.Instance.EndGame(false); // 실패로 끝남
            Debug.Log("시간 종료 - 게임 종료 처리 필요");
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void HandleTimeExpiration()
    {
        isRunning = false;
        
        // 직접 BasGameManager를 호출하는 대신 이벤트 발생
        OnTimerExpired?.Invoke();
        
        Debug.Log("타이머 종료 - 이벤트 발생!");
    }
}
