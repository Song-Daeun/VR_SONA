using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int goalCount = 0;
    public int requiredGoals = 10;
    public TMP_Text resultText;

    private bool isGameEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    public void AddGoal()
    {
        if (isGameEnded) return;

        goalCount++;

        if (goalCount >= requiredGoals)
        {
            EndGame(true); // 성공
        }
    }

    public void EndGame(bool success)
    {
        if (isGameEnded) return;

        isGameEnded = true;

        GameObject.FindObjectOfType<GameTimer>().StopTimer();

        if (resultText != null)
        {
            resultText.text = success ? "Success!" : "Failed!";
        }

        Time.timeScale = 0f;
    }
}
