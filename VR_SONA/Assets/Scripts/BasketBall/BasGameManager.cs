using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class BasGameManager : MonoBehaviour
{
    public static BasGameManager Instance;

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

    void Update()
    {
        CheckOvertime();
    }

    public void CheckOvertime()
    {
        if (isGameEnded) return;

        var timer = FindObjectOfType<GameBasketballTimer>();
        if (timer != null && !timer.IsRunning)
        {
            EndGame(false);
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

        MissionResult = success;

        FindObjectOfType<GameBasketballTimer>()?.StopTimer();

        if (gameStateText != null)
        {
            gameStateText.text = success ? "Success!" : "Failed!";
        }

        Time.timeScale = 0f;

        if (returnButton != null) 
        {
            returnButton.SetActive(true);
        }
        else
        {
            Debug.LogError("[BasketBall] returnButton가 null입니다");
        }   
    }
}