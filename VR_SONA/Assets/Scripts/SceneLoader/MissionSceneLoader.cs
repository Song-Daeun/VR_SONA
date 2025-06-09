using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

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

            // 미션 씬 언로드 추가
            string sceneName = MissionManager.Instance.GetSceneNameFromMission(
                MissionManager.Instance.GetMissionType(tile)
            );
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneName);
                Debug.Log($"🟣 미션 씬 {sceneName} 언로드");
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

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("⚠️ 로드할 씬 이름이 없습니다.");
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

            // ✅ XR Origin을 포함한 씬은 파괴되지 않도록 유지됨
            // ✅ Additive 로드로 새 씬 추가
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            Debug.Log($"✅ 씬 {sceneName} 로드됨");

            missionTriggered = true;

            StartCoroutine(SetActiveSceneAfterLoad(sceneName));
        }
    }

    private IEnumerator SetActiveSceneAfterLoad(string sceneName)
    {
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneName).isLoaded);

        // ✅ 미션 씬이 아니라 InteractionScene을 활성 씬으로 설정
        Scene interactionScene = SceneManager.GetSceneByName("InteractionScene");
        if (interactionScene.IsValid())
        {
            SceneManager.SetActiveScene(interactionScene);
            Debug.Log("🟢 활성 씬을 InteractionScene으로 유지");
        }
        else
        {
            Debug.LogWarning("InteractionScene을 찾을 수 없습니다.");
        }

        // Additive 씬 내 TeleportationArea와 InteractionManager 연결 (기존 코드 유지)
        var teleportAreas = FindObjectsOfType<TeleportationArea>();
        var interactionManager = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();

        foreach (var area in teleportAreas)
        {
            if (area.interactionManager == null && interactionManager != null)
            {
                area.interactionManager = interactionManager;
                Debug.Log($"TeleportationArea {area.name} 에 InteractionManager 연결 완료");
            }
        }
    }

}
