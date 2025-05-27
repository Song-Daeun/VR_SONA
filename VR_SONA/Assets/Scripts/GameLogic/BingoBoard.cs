using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BingoBoard : MonoBehaviour
{
    [Header("Board Size")]
    public int rows = 3;
    public int cols = 3;

    [Header("Hierarchy Reference")]
    public GameObject tileParent; // "Tile" GameObject를 Inspector에 드래그

    private TileData[,] tiles;
    private Transform[,] tilePositions;

    private Dictionary<GameObject, Vector2Int> tileToCoords = new Dictionary<GameObject, Vector2Int>();
    private Dictionary<Vector2Int, GameObject> coordToTile = new Dictionary<Vector2Int, GameObject>();

    private string[] validTileNames = {
        "NetherlandTile", "GermanyTile", "USATile", "SpellBookTile",
        "JapanTile", "SeoulTile", "SuncheonTile", "TaiwanTile"
    };

    void Start()
    {
        InitializeTilePositions();
    }

    // 자동으로 tilePositions과 타일 정보 매핑
    private void InitializeTilePositions()
    {
        tiles = new TileData[rows, cols];
        tilePositions = new Transform[rows, cols];

        Transform[] allTiles = tileParent.GetComponentsInChildren<Transform>(true);

        List<GameObject> foundTiles = new List<GameObject>();
        foreach (Transform t in allTiles)
        {
            if (validTileNames.Contains(t.name))
                foundTiles.Add(t.gameObject);
        }

        // 위치 기준 정렬: z가 위→아래, x가 좌→우
        foundTiles = foundTiles
            .OrderBy(t => t.transform.position.z)
            .ThenBy(t => t.transform.position.x)
            .ToList();

        for (int i = 0; i < foundTiles.Count && i < rows * cols; i++)
        {
            int x = i / cols;
            int y = i % cols;

            tiles[x, y] = new TileData();
            tilePositions[x, y] = foundTiles[i].transform;

            tileToCoords[foundTiles[i]] = new Vector2Int(x, y);
            coordToTile[new Vector2Int(x, y)] = foundTiles[i];
        }

        Debug.Log("✅ 타일 위치 자동 초기화 완료");
    }

    // 플레이어가 서 있는 타일 좌표 계산
    public Vector2Int GetPlayerTileCoords()
    {
        GameObject player = GameObject.Find("XR Origin (XR Rig)");

        if (player == null)
        {
            Debug.LogWarning("🚫 플레이어를 찾을 수 없습니다.");
            return new Vector2Int(-1, -1);
        }

        Vector3 playerPos = player.transform.position;

        float minDist = float.MaxValue;
        Vector2Int closestCoord = new Vector2Int(-1, -1);

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                float dist = Vector3.Distance(playerPos, tilePositions[x, y].position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestCoord = new Vector2Int(x, y);
                }
            }
        }

        return closestCoord;
    }

    // 미션 성공 시 건물 생성
    public void OnMissionSuccess(int x, int y)
    {
        tiles[x, y].isMissionCleared = true;
        tiles[x, y].isOccupied = true;

        if (tiles[x, y].buildingPrefab == null)
        {
            Debug.LogWarning($"🚫 buildingPrefab이 설정되지 않았습니다. ({x}, {y})");
            return;
        }

        Vector3 spawnPos = tilePositions[x, y].position + Vector3.up * 10f;

        GameObject building = Instantiate(
            tiles[x, y].buildingPrefab,
            spawnPos,
            Quaternion.identity,
            tilePositions[x, y]
        );

        StartCoroutine(DropBuilding(building, tilePositions[x, y].position));
    }

    // 건물이 위에서 "쿵" 하고 떨어지는 연출
    private IEnumerator DropBuilding(GameObject obj, Vector3 targetPos)
    {
        float time = 0f;
        float duration = 0.5f;
        Vector3 startPos = obj.transform.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        obj.transform.position = targetPos;
    }

    // 특정 타일 오브젝트에서 해당 국가의 건물 오브젝트를 찾아 buildingPrefab으로 설정
    public void SetBuildingPrefabFromTile(GameObject tileGO, int x, int y)
    {
        string countryName = tileGO.name.Replace("Tile", "");
        Transform building = FindChildByNameIncludingInactive(tileGO.transform.parent, countryName + "Building");

        if (building != null)
        {
            tiles[x, y].buildingPrefab = building.gameObject;
            Debug.Log($"✅ {countryName} 건물 프리팹 설정 완료 ({x}, {y})");
        }
        else
        {
            Debug.LogWarning($"❗ '{countryName}Building' 오브젝트를 찾지 못함.");
        }
    }

    private Transform FindChildByNameIncludingInactive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }

    // 테스트용 키 입력 처리
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2Int coords = GetPlayerTileCoords();
            if (coords.x == -1) return;

            GameObject tileGO = coordToTile[coords];
            SetBuildingPrefabFromTile(tileGO, coords.x, coords.y);
            OnMissionSuccess(coords.x, coords.y);
        }
    }
}
