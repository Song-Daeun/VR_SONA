using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSceneLoader : MonoBehaviour
{
    public CoinUIManager coinUIManager; // Inspector에서 연결할 예정

    void Start()
    {
        // 자동으로 CoinUIManager 찾기 (보조용)
        if (coinUIManager == null)
        {
            coinUIManager = FindObjectOfType<CoinUIManager>();
            if (coinUIManager == null)
            {
                Debug.LogError("CoinUIManager가 씬에 존재하지 않습니다!");
            }
        }
    }

    public void LoadMissionScene()
    {
        if (coinUIManager == null)
        {
            Debug.LogError("CoinUIManager가 연결되지 않았습니다!");
            return;
        }

        if (coinUIManager.HasEnoughCoins())
        {
            coinUIManager.SubtractCoinsForMission();
            SceneManager.LoadScene("MissionBasketballScene", LoadSceneMode.Additive);
            Debug.Log("미션 씬 로드 및 코인 차감 완료");
        }
        else
        {
            Debug.Log("코인이 부족하여 미션 씬을 로드할 수 없습니다");
        }
    }

    public void UnloadMissionScene()
    {
        SceneManager.UnloadSceneAsync("MissionBasketballScene");
    }
}
