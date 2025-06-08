// [리팩토링] 게임로직과 코인UI 분리 CoinUIManager -> CoinManager
// 코인 개수 UI 업데이트 기능 담당하는 클래스

using UnityEngine;
using TMPro;

public class CoinUIManager : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    public GameObject coinBackground;

    void Start()
    {
        UpdateCoinUI();
    }

    public void OnMissionStart() // 외부에서 호출
    {
        if (CoinManager.SubtractCoinsForMission())
        {
            UpdateCoinUI();
        }
    }

    public void UpdateCoinUI()
    {
        coinText.text = "Coins: " + PlayerState.CoinCount.ToString();
    }
}