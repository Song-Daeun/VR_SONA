using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BingoBoard : MonoBehaviour
{
    [Header("Board Size")]
    public int rows = 3;
    public int cols = 3;

    private TileData[,] tiles;
    private Transform[,] tilePositions;

    private Dictionary<GameObject, Vector2Int> tileToCoords = new Dictionary<GameObject, Vector2Int>();
    private Dictionary<Vector2Int, GameObject> coordToTile = new Dictionary<Vector2Int, GameObject>();

    public static BingoBoard Instance { get; private set; }
    
    void Start()
    {
        InitializeTilePositions();
    }

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 이름 기반으로 타일들을 정확한 (x,y) 좌표에 매핑
    /// </summary>
    private void InitializeTilePositions()
    {
        tiles = new TileData[rows, cols];
        tilePositions = new Transform[rows, cols];

        string[,] expectedOrder = new string[3, 3]
        {
            { "NetherlandTile", "GermanyTile", "USATile" },
            { "SpellBookTile", "JapanTile", "SeoulTile" },
            { "SuncheonTile", "EgyptTile", "StartTile" }
        };

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                string tileName = expectedOrder[x, y];
                GameObject go = GameObject.Find(tileName);

                if (go != null)
                {
                    tiles[x, y] = new TileData();
                    tilePositions[x, y] = go.transform;

                    tileToCoords[go] = new Vector2Int(x, y);
                    coordToTile[new Vector2Int(x, y)] = go;

                    Debug.Log($"✅ tilePositions[{x},{y}] = {tileName}");
                }
                else
                {
                    Debug.LogError($"❌ 타일 '{tileName}'을 찾지 못했습니다.");
                }
            }
        }

        Debug.Log("✅ 이름 기반 타일 초기화 완료");
    }

    /// <summary>
    /// 플레이어가 가장 가까이 있는 타일의 (x,y) 좌표를 반환
    /// </summary>
    public Vector2Int GetPlayerTileCoords()
    {
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        if (player == null)
        {
            Debug.LogWarning("🚫 PlayerXR 태그가 지정된 오브젝트를 찾을 수 없습니다.");
            return new Vector2Int(-1, -1);
        }

        Vector3 playerPos = player.transform.position;

        float minDist = float.MaxValue;
        Vector2Int closestCoord = new Vector2Int(-1, -1);

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (tilePositions[x, y] == null)
                {
                    Debug.LogError($"tilePositions[{x},{y}] is null!");
                    continue;
                }

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

    /// <summary>
    /// 미션 성공 시 해당 위치에 건물 생성
    /// </summary>
    public void OnMissionSuccess(int x, int y)
    {
        tiles[x, y].isMissionCleared = true;
        tiles[x, y].isOccupied = true;

        GameObject building = tiles[x, y].buildingPrefab;

        if (building == null)
        {
            Debug.LogWarning($"🚫 buildingPrefab이 설정되지 않았습니다. ({x}, {y})");
            return;
        }

        // 건물 원래 위치 저장
        Vector3 targetPos = building.transform.position;

        // 위에서 떨어지게 초기 위치 세팅
        building.transform.position = targetPos + Vector3.up * 10f;

        // 비활성화 되어 있는 오브젝트 활성화
        building.SetActive(true);

        // 디버그 출력
        Debug.Log($"🏗 기존 건물 오브젝트 활성화됨 → 위치: {targetPos}, 이름: {building.name}");

        // 떨어지는 연출
        StartCoroutine(DropBuilding(building, targetPos));
    }

    /// <summary>
    /// 건물이 위에서 떨어지는 애니메이션 연출
    /// </summary>
    private IEnumerator DropBuilding(GameObject obj, Vector3 targetPos)
    {
        float time = 0f;
        float duration = 0.5f;
        Vector3 startPos = obj.transform.position;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        obj.transform.position = targetPos;
    }

    public GameObject GetTileGameObject(int x, int y)
    {
        Vector2Int coord = new Vector2Int(x, y);
        if (coordToTile.ContainsKey(coord))
        {
            return coordToTile[coord];
        }

        Debug.LogWarning($"🚫 coordToTile에 ({x}, {y}) 좌표가 없음");
        return null;
    }


    /// <summary>
    /// 해당 타일의 국가 이름에 맞는 건물 프리팹을 자동으로 설정
    /// </summary>
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
            Debug.LogWarning($"❗ '{countryName}Building' 오브젝트를 찾지 못했습니다.");
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

    /// <summary>
    /// 테스트용: Space 키 누르면 현재 플레이어 위치에 건물 생성
    /// </summary>
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("🔍 B 키 눌림 감지됨!");

            Vector2Int coords = GetPlayerTileCoords();
            if (coords.x == -1)
            {
                Debug.LogWarning("🚫 플레이어가 타일 위에 없음");
                return;
            }

            GameObject tileGO = coordToTile[coords];
            Debug.Log("🎯 타일 찾음: " + tileGO.name);
            SetBuildingPrefabFromTile(tileGO, coords.x, coords.y);
            OnMissionSuccess(coords.x, coords.y);
        }
#endif
    }
}
