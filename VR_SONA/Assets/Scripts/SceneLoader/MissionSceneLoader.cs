using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MissionSceneLoader : MonoBehaviour
{
    public CoinUIManager coinUIManager;
    public GameObject uiTerrain;

    private bool missionTriggered = false;

    void Start()
    {
        if (coinUIManager == null)
        {
            coinUIManager = FindObjectOfType<CoinUIManager>();
        }
    }

    void Update()
    {
        if (missionTriggered && GameManager.MissionResult.HasValue)
        {
            bool result = GameManager.MissionResult.Value;
            Vector2Int tile = PlayerState.LastEnteredTileCoords;

            if (result)
            {
                Debug.Log("미션 성공 - 건물 생성");
                GameObject tileGO = BingoBoard.Instance.GetTileGameObject(tile.x, tile.y);
                BingoBoard.Instance.SetBuildingPrefabFromTile(tileGO, tile.x, tile.y);
                BingoBoard.Instance.OnMissionSuccess(tile.x, tile.y);
            }
            else
            {
                Debug.Log("미션 실패 - 건물 생성 안 함");
            }

            GameManager.MissionResult = null;
            missionTriggered = false;
        }
    }

    public void LoadMissionScene()
    {
        if (coinUIManager == null)
        {
            Debug.LogError("🚫 coinUIManager가 연결되지 않았습니다.");
            return;
        }

        if (!CoinManager.HasEnoughCoins())
        {
            Debug.Log("코인이 부족합니다.");
            return;
        }

        Vector2Int coords = BingoBoard.Instance?.GetPlayerTileCoords() ?? new Vector2Int(-1, -1);
        Debug.Log("🎯 플레이어 위치: " + coords);

        PlayerState.LastEnteredTileCoords = coords;

        if (MissionManager.Instance == null)
        {
            Debug.LogError("🚫 MissionManager.Instance is null!");
            return;
        }

        MissionType missionType = MissionManager.Instance.GetMissionType(coords);
        string sceneName = MissionManager.Instance.GetSceneNameFromMission(missionType);

        Debug.Log($"🧩 미션타입: {missionType}, 🗺 씬 이름: {sceneName}");

        if (sceneName == null)
        {
            Debug.LogWarning("⚠️ 로드할 씬 이름이 null입니다.");
            return;
        }

        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.Log("이미 로드된 씬입니다.");
            return;
        }

        if (CoinManager.SubtractCoinsForMission())
        {
            coinUIManager.UpdateCoinUI();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            Debug.Log($"✅ 씬 {sceneName} 로드됨");

            missionTriggered = true;

            // 씬 활성화 코루틴 시작
            StartCoroutine(SetActiveSceneAfterLoad(sceneName));
        }
    }

    // 🧩 추가된 코루틴
    private IEnumerator SetActiveSceneAfterLoad(string sceneName)
    {
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneName).isLoaded);

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            Debug.Log($"🟢 활성 씬을 {sceneName} 으로 설정");
        }
        else
        {
            Debug.LogError($"🚫 {sceneName} 씬을 활성 씬으로 설정하지 못함");
        }
    }
}
