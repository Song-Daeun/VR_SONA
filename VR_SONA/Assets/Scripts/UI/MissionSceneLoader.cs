using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionSceneLoader : MonoBehaviour
{
    public CoinUIManager coinUIManager; // Inspector에서 연결할 예정
    public GameObject loadButton;
    public GameObject unloadButton;
    public GameObject returnButton; // 되돌아가기 버튼 (미션 씬에서만 표시)
    
    public GameObject uiTerrain;
    public GameObject missionMessageText;
    private bool missionTriggered = false;

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
            if (uiTerrain != null) uiTerrain.SetActive(false); // ui씬의 terrain 비활성화

            // 미션 씬 올라와 있는 상태면 load/unload 숨기고 되돌아가기 버튼 표시
            if (loadButton != null) loadButton.SetActive(false);
            if (unloadButton != null) unloadButton.SetActive(false);
            if (returnButton != null) returnButton.SetActive(true);

            if (missionMessageText != null) missionMessageText.SetActive(false);
        }
        else
        {
            if (returnButton != null) returnButton.SetActive(false);
        }
    }

    void Update()
    {
        if (missionTriggered &&
            !SceneManager.GetSceneByName("MissionBasketballScene").isLoaded &&
            GameManager.MissionResult.HasValue)
        {
            bool result = GameManager.MissionResult.Value;

            if (result == true)
            {
                Debug.Log("✅ 미션 성공 - 건물 생성");

                Vector2Int tile = PlayerState.LastEnteredTileCoords;

                GameObject tileGO = BingoBoard.Instance.GetTileGameObject(tile.x, tile.y);
                BingoBoard.Instance.SetBuildingPrefabFromTile(tileGO, tile.x, tile.y);
                BingoBoard.Instance.OnMissionSuccess(tile.x, tile.y); // ← 이거 성공일 때만 실행!
            }
            else
            {
                Debug.Log("❌ 미션 실패 - 건물 생성 안 함");
            }

            // 상태 초기화
            GameManager.MissionResult = null;
            missionTriggered = false;
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

            // 여기서 플레이어 타일 좌표를 저장
            PlayerState.LastEnteredTileCoords = BingoBoard.Instance.GetPlayerTileCoords();
            Debug.Log($"🧭 현재 타일 위치 저장됨: {PlayerState.LastEnteredTileCoords}");

            if (!SceneManager.GetSceneByName("MissionBasketballScene").isLoaded)
            {
                SceneManager.LoadScene("MissionBasketballScene", LoadSceneMode.Additive);
                Debug.Log("미션 씬 로드 및 코인 차감 완료");

                missionTriggered = true;

                if (uiTerrain != null) uiTerrain.SetActive(false);
                if (loadButton != null) loadButton.SetActive(false);
                if (unloadButton != null) unloadButton.SetActive(false);
                if (returnButton != null) returnButton.SetActive(true);
                if (missionMessageText != null) missionMessageText.SetActive(false);
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

        if (missionMessageText != null) missionMessageText.SetActive(false);
    }

    public void ReturnToUI()
    {
        // 되돌아가기: 미션 씬만 언로드
        if (SceneManager.GetSceneByName("MissionBasketballScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("MissionBasketballScene");
            if (uiTerrain != null) uiTerrain.SetActive(true); // ui씬의 terrain 활성화

            // 다시 Load/Unload 버튼 표시
            if (loadButton != null) loadButton.SetActive(true);
            if (unloadButton != null) unloadButton.SetActive(true);
            if (returnButton != null) returnButton.SetActive(false);


            Debug.Log("미션 씬에서 UI 씬으로 돌아옴");
        }
    }
}
