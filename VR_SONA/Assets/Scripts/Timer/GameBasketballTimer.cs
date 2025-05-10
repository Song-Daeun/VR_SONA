using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float totalTime = 15f;
    private float currentTime;

    public TMP_Text timerText;

    private bool isRunning = true;

    void Start()
    {
        currentTime = totalTime;
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0); // 음수 방지

        UpdateTimerUI();

        if (currentTime <= 0f)
        {
            isRunning = false;
            GameManager.Instance.EndGame(false); // 실패 처리
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(currentTime).ToString();
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
