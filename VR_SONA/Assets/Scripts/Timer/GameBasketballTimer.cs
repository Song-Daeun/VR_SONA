using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameBasketballTimer : MonoBehaviour
{
    public float totalTime = 15f;
    private float currentTime;
    public Slider timerSlider;
    private bool isRunning = true;

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
            GameManager.Instance.EndGame(false); // 실패로 끝남
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
