using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackButton: MonoBehaviour
{
    public DiceManager diceManager;

    void Start()
    {
        // 자동으로 InteractionScene 안의 DiceManager 찾기
        if (diceManager == null)
        {
            diceManager = FindObjectOfType<DiceManager>();
            if (diceManager == null)
                Debug.LogWarning("DiceManager 못 찾음");
        }
    }

    public void OnBackClicked()
    {
        Debug.Log("BackButtonHandler 호출됨!");
        if (diceManager != null)
            diceManager.OnBackButtonClicked();
        else
            Debug.LogWarning("DiceManager 연결 안 됨!");
    }
}

