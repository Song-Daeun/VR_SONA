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
    
    // 타일 그리드 구조 (공통 사용)
    public static string[,] TileGrid = new string[3, 3]
    {
        { "Netherlands", "Germany", "USA" },
        { "SpellBook", "Japan", "Seoul" },
        { "Suncheon", "Egypt", "Start" }
    };
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeTilePositions();
    }

    // 타일 위치 초기화
    private void InitializeTilePositions()
    {
        tiles = new TileData[rows, cols];
        tilePositions = new Transform[rows, cols];

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                string tileName = TileGrid[x, y];
                GameObject go = GameObject.Find(tileName);

                if (go != null)
                {
                    tiles[x, y] = new TileData();
                    tilePositions[x, y] = go.transform;

                    tileToCoords[go] = new Vector2Int(x, y);
                    coordToTile[new Vector2Int(x, y)] = go;

                    // Start 타일은 게임 시작부터 점령된 상태로 설정
                    if (tileName == "Start")
                    {
                        tiles[x, y].isMissionCleared = true;
                        tiles[x, y].isOccupied = true;
                    }
                    else
                    {
                        SetBuildingPrefabFromTile(go, x, y);
                    }
                }
                else
                {
                    Debug.LogError($"타일 '{tileName}'을 찾지 못했습니다.");
                }
            }
        }
    }

    // 플레이어 가까운 타일 위치 반환 
    public Vector2Int GetPlayerTileCoords()
    {
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        if (player == null)
        {
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

    // 미션 성공 시 해당 위치에 건물 생성
    public void OnMissionSuccess(int x, int y)
    {
        if (x < 0 || x >= rows || y < 0 || y >= cols)
        {
            return;
        }

        tiles[x, y].isMissionCleared = true;
        tiles[x, y].isOccupied = true;

        GameObject building = tiles[x, y].buildingPrefab;

        if (building == null)
        {
            return;
        }

        Vector3 targetPos = building.transform.position;
        building.transform.position = targetPos + Vector3.up * 10f;
        building.SetActive(true);

        // 떨어지는 연출
        StartCoroutine(DropBuilding(building, targetPos));
    }

    // 건물 생성 애니메이션
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

    // 해당 타일의 국가 이름에 맞는 건물 프리팹을 자동으로 설정
    public void SetBuildingPrefabFromTile(GameObject tileGO, int x, int y)
    {
        string countryName = tileGO.name.Replace("Tile", "");
        Transform building = FindChildByNameIncludingInactive(tileGO.transform.parent, countryName + "Building");

        if (building != null)
        {
            tiles[x, y].buildingPrefab = building.gameObject;
        }
        else
        {
            Debug.LogWarning($"'{countryName}Building' 오브젝트를 찾지 못했습니다.");
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

    // 타일 이름으로 좌표 반환
    public static Vector2Int GetTileCoordsByName(string tileName)
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (TileGrid[x, y] == tileName)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    // 좌표로 타일 이름 반환
    public static string GetTileNameByCoords(int x, int y)
    {
        if (x >= 0 && x < 3 && y >= 0 && y < 3)
        {
            return TileGrid[x, y];
        }
        return "";
    }

    // 타일 상태 관리
    public GameObject GetTileGameObject(int x, int y)
    {
        Vector2Int coord = new Vector2Int(x, y);
        if (coordToTile.ContainsKey(coord))
        {
            return coordToTile[coord];
        }
        return null;
    }

    // 미션 완료 상태 설정 (MissionManager에서 호출)
    public void SetTileMissionCleared(int x, int y, bool cleared)
    {
        if (x < 0 || x >= rows || y < 0 || y >= cols)
        {
            Debug.LogError($"잘못된 좌표: ({x}, {y})");
            return;
        }

        tiles[x, y].isMissionCleared = cleared;
        Debug.Log($"타일 ({x}, {y}) 미션 완료 상태: {cleared}");
    }

    // 미션 완료 상태 확인 (GameManager에서 호출)
    public bool IsTileMissionCleared(int x, int y)
    {
        if (x < 0 || x >= rows || y < 0 || y >= cols)
        {
            return false;
        }

        return tiles[x, y].isMissionCleared;
    }
}