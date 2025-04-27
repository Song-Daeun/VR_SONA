using UnityEngine;
using TMPro;

public class CoinUIManager : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    public GameObject coinBackground; // 타원형 배경을 위한 Image 오브젝트

    private int coinCount = 800;

    void Start()
    {
        UpdateCoinUI();
    }

    public void SubtractCoinsForMission()
    {
        if (coinCount >= 100)
        {
            coinCount -= 100;
            UpdateCoinUI();
        }
        else
        {
            Debug.Log("코인이 부족합니다");
        }
    }

    private void UpdateCoinUI()
    {
        coinText.text = "Coins: " + coinCount.ToString();
    }
}