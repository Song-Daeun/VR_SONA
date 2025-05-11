using UnityEngine;
using TMPro;

public class CoinUIManager : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    public GameObject coinBackground; // 타원형 배경 이미지

    private int coinCount = 800;

    void Start()
    {
        UpdateCoinUI();
    }

    public bool HasEnoughCoins()
    {
        return coinCount >= 100;
    }

    public void SubtractCoinsForMission()
    {
        if (HasEnoughCoins())
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
