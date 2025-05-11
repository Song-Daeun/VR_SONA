using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSceneLoader : MonoBehaviour
{
    public CoinUIManager coinUIManager; // Inspector에서 연결할 예정
    public GameObject loadButton;
    public GameObject unloadButton;

    void Start()
    {
        if (coinUIManager == null)
        {
            coinUIManager = FindObjectOfType<CoinUIManager>();
            if (coinUIManager == null)
            {
                Debug.LogError("CoinUIManager가 씬에 존재하지 않습니다!");
            }
        }

        // 씬 상태 확인해서 버튼 조정
        if (SceneManager.GetSceneByName("MissionBasketballScene").isLoaded)
        {
            // 미션 씬 올라와 있는 상태면 load/unload 숨기고 되돌아가기 버튼 표시
            if (loadButton != null) loadButton.SetActive(false);
            if (unloadButton != null) unloadButton.SetActive(false);
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

            // 로드할 씬이 이미 로드되어 있지 않다면 추가
            if (!SceneManager.GetSceneByName("MissionBasketballScene").isLoaded)
            {
                SceneManager.LoadScene("MissionBasketballScene", LoadSceneMode.Additive);
                Debug.Log("미션 씬 로드 및 코인 차감 완료");

                // Load/Unload 버튼 숨기고 Return 버튼 표시
                if (loadButton != null) loadButton.SetActive(false);
                if (unloadButton != null) unloadButton.SetActive(false);
            }
            else
            {
                Debug.Log("이미 미션 씬이 로드되어 있습니다.");
            }
        }
        else
        {
            Debug.Log("코인이 부족하여 미션 씬을 로드할 수 없습니다");
        }
    }

    public void UnloadMissionScene()
    {
        // 버튼만 숨김
        if (loadButton != null) loadButton.SetActive(false);
        if (unloadButton != null) unloadButton.SetActive(false);
    }

    public void ReturnToUI()
    {
        // 되돌아가기: 미션 씬만 언로드
        if (SceneManager.GetSceneByName("MissionBasketballScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("MissionBasketballScene");

            // 다시 Load/Unload 버튼 표시
            if (loadButton != null) loadButton.SetActive(true);
            if (unloadButton != null) unloadButton.SetActive(true);

            Debug.Log("미션 씬에서 UI 씬으로 돌아옴");
        }
    }
}
