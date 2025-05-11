using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMP_Text gameStateText;
    public int goalCount = 0;
    public int requiredGoals = 10;

    private bool isGameEnded = false;

    public GameObject returnButton;

    private void Awake()
    {
        Instance = this;

        if (returnButton != null) returnButton.SetActive(false);
    }

    public void AddGoal()
    {
        // 득점은 언제든 받을 수 있음 → 성공 조건 먼저 검사
        goalCount++;

        if (!isGameEnded && goalCount >= requiredGoals)
        {
            EndGame(true); // 성공 처리
        }
    }

    public void EndGame(bool success)
    {
        // 이미 성공했으면 실패로 덮지 않도록
        if (isGameEnded)
        {
            // 성공 후 실패가 들어오는 걸 막기 위해 성공이면 무시
            if (success) return;
            else return; // 실패도 이미 처리했으면 무시
        }

        isGameEnded = true;

        FindObjectOfType<GameBasketballTimer>()?.StopTimer();

        if (gameStateText != null)
        {
            gameStateText.text = success ? "Success!" : "Failed!";
        }

        Time.timeScale = 0f;

        if (returnButton != null) returnButton.SetActive(true);
    }
}
