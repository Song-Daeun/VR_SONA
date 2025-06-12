using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasGameManager : MonoBehaviour
{
    public static BasGameManager Instance;

    // UIScene에 결과 전달할 정적 변수
    public static bool? MissionResult = null;

    public TMP_Text gameStateText;
    public int goalCount = 0;
    public int requiredGoals = 0;

    private bool isGameEnded = false;

    public GameObject returnButton;

    private void Awake()
    {
        Instance = this;

        if (returnButton != null) returnButton.SetActive(false);
    }

    public void AddGoal()
    {
        goalCount++;

        if (!isGameEnded && goalCount >= requiredGoals)
        {
            EndGame(true); // 성공 처리
        }
    }

    public void EndGame(bool success)
    {
        if (isGameEnded)
        {
            if (success) return;
            else return;
        }

        isGameEnded = true;

        // 결과 저장 (UIScene에서 읽을 수 있도록)
        MissionResult = success;

        FindObjectOfType<GameBasketballTimer>()?.StopTimer();

        if (gameStateText != null)
        {
            gameStateText.text = success ? "Success!" : "Failed!";
        }

        Time.timeScale = 0f;

        if (returnButton != null) returnButton.SetActive(true);
    }
}