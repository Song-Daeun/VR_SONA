using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Player Settings")]
    public Transform playerTransform;
    public List<Transform> tileList;
    public Transform startTile; 
    public float moveDuration = 0.5f;
    public float teleportDuration = 0.1f; 

    [Header("Landing Settings")]
    public float heightOffset = 1.6f;
    public LayerMask groundLayerMask = -1;
    public float raycastDistance = 10.0f;

    private bool isMoving = false;
    private int currentTileIndex = 0;
    private int currentDiceResult = -1;

    public static System.Action<string, int> OnTileArrived;
    public static System.Action OnSpellBookTileArrived;

    [Header("Position Offset Settings")]
    public Vector3 tilePositionOffset = new Vector3(0f, 0f, -50f); 

    
    // 싱글톤 초기화
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        ValidateComponents();
    }

    // 게임 시작 시 플레이어 위치 초기화
    private void Start()
    {
        StartCoroutine(InitializePlayerPosition());
    }

    // 게임 시작 시 플레이어 위치 초기화
    private IEnumerator InitializePlayerPosition()
    {
        yield return null;
        
        // 시작 타일로 이동
        if (startTile != null)
        {
            MoveToStart();
        }
        else
        {
            Vector3 currentPos = playerTransform.position;
            Vector3 safePosition = new Vector3(currentPos.x, currentPos.y + heightOffset, currentPos.z);
            playerTransform.position = safePosition;
            
            // 초기 상태 설정
            currentTileIndex = -1; 
            currentDiceResult = -1;
        }
    }

    private void ValidateComponents()
    {
        if (playerTransform == null)
        {
            playerTransform = this.transform;
            Debug.Log("playerTransform이 자동으로 설정되었습니다: " + playerTransform.name);
        }

        if (tileList == null || tileList.Count == 0)
        {
            Debug.LogWarning("tileList가 설정되지 않았습니다. Inspector에서 타일들을 할당해주세요.");
        }

        if (startTile == null)
        {
            Debug.LogWarning("startTile이 설정되지 않았습니다. Inspector에서 시작 타일을 할당해주세요.");
        }
    }

    // OnDestroy에서 싱글톤 정리
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public Vector3 GetPlayerPosition()
    {
        return playerTransform != null ? playerTransform.position : Vector3.zero;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public int GetCurrentTileIndex()
    {
        return currentTileIndex;
    }

    public int GetCurrentDiceResult()
    {
        return currentDiceResult;
    }

    // 플레이어 이동
    public void MovePlayer(int diceResult)
    {
        if (isMoving)
        {
            return;
        }

        if (tileList == null || tileList.Count == 0)
        {
            return;
        }

        int targetIndex = diceResult - 1;

        if (targetIndex < 0 || targetIndex >= tileList.Count)
        {
            return;
        }

        Transform targetTile = tileList[targetIndex];
        if (targetTile == null)
        {
            return;
        }

        // 현재 주사위 결과 저장 
        currentDiceResult = diceResult;
        currentTileIndex = targetIndex; 
        Vector3 targetPosition = CalculatePlayerPositionOnTile(targetTile);

        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false, true));
    }

    // 텔레포트
    public void TeleportToTile(int tileIndex)
    {
        if (isMoving)
        {
            return;
        }

        if (tileList == null || tileList.Count == 0)
        {
            return;
        }

        if (tileIndex < 0 || tileIndex >= tileList.Count)
        {
            return;
        }

        Transform targetTile = tileList[tileIndex];
        if (targetTile == null)
        {
            return;
        }

        currentDiceResult = tileIndex + 1;
        currentTileIndex = tileIndex; // 타일 인덱스 업데이트

        Vector3 targetPosition = CalculatePlayerPositionOnTile(targetTile);
        StartCoroutine(MoveToPosition(targetPosition, teleportDuration, false, true));
    }

    // Start 타일로 이동 
    public void MoveToStart()
    {
        if (isMoving)
        {
            return;
        }

        if (startTile == null)
        {
            return;
        }

        currentDiceResult = -1;
        currentTileIndex = -1;

        Vector3 targetPosition = CalculatePlayerPositionOnTile(startTile);
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false, false));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration = -1, bool showMission = false, bool notifyGameManager = false)
    {        
        if (duration < 0) duration = moveDuration;

        // CharacterController 비활성화
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) 
        {
            cc.enabled = false;
        }

        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;
        isMoving = true;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = targetPosition;

        if (cc != null) 
        {
            cc.enabled = true;
        }

        isMoving = false;
        Debug.Log("isMoving = false로 설정됨");

        if (notifyGameManager)
        {
            // 현재 타일 이름 확인
            string currentTileName = GetCurrentTileName();
            OnTileArrived?.Invoke(currentTileName, currentTileIndex);
            
            if (currentTileName == "SpellBook")
            {
                OnSpellBookTileArrived?.Invoke();
            }
        }
    }
    
    private Vector3 CalculateSafeLandingPosition(Transform tile)
    {
        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider == null)
        {
            return tile.position + Vector3.up * heightOffset;
        }

        Bounds bounds = tileCollider.bounds;
        Vector3 safePosition = new Vector3(bounds.center.x, bounds.max.y + heightOffset, bounds.center.z);

        return safePosition;
    }

    // 현재 타일 인덱스 설정
    public void SetCurrentTileIndex(int index)
    {
        currentTileIndex = index;
    }    

    public bool IsValidTileIndex(int index)
    {
        return tileList != null && index >= 0 && index < tileList.Count;
    }

    public Transform GetTileByIndex(int index)
    {
        if (IsValidTileIndex(index))
        {
            return tileList[index];
        }
        return null;
    }

    public string GetCurrentTileName()
    {
        if (currentTileIndex == -1)
        {
            return startTile != null ? NormalizeTileName(startTile.name) : "Start";
        }

        if (IsValidTileIndex(currentTileIndex))
        {
            string rawTileName = tileList[currentTileIndex].name;
            string normalizedName = NormalizeTileName(rawTileName);

            return normalizedName;
        }

        return "Unknown";
    }

    private string NormalizeTileName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
        {
            return "Unknown";
        }
        
        string normalizedName = rawName.Trim(); 

        if (normalizedName.EndsWith("Tile", System.StringComparison.OrdinalIgnoreCase))
        {
            normalizedName = normalizedName.Substring(0, normalizedName.Length - 4);
        }
        normalizedName = normalizedName.Trim(); 
        normalizedName = HandleSpecialCases(normalizedName);
        
        if (string.IsNullOrEmpty(normalizedName))
        {
            return rawName;
        }
        
        return normalizedName;
    }

    private string HandleSpecialCases(string tileName)
    {
        var specialCases = new System.Collections.Generic.Dictionary<string, string>
        {
            // 대소문자 변형들 처리
            {"spellbook", "SpellBook"},
            {"SPELLBOOK", "SpellBook"},
            {"netherlands", "Netherlands"},
            {"NETHERLANDS", "Netherlands"},
            {"germany", "Germany"},
            {"GERMANY", "Germany"},
            
            // 공백이나 언더스코어가 포함된 경우들
            {"Spell_Book", "SpellBook"},
            {"Spell Book", "SpellBook"},
            
            // Start 타일의 다양한 변형들
            {"StartTile", "Start"},
            {"start", "Start"},
            {"START", "Start"}
        };
        
        // 특별 케이스 매핑 확인
        if (specialCases.ContainsKey(tileName))
        {
            string mappedName = specialCases[tileName];
            Debug.Log($"특별 케이스 적용: {tileName} → {mappedName}");
            return mappedName;
        }
        return tileName;
    }


    private bool CheckIfMappingExists(string tileName)
    {
        if (GameManager.Instance != null)
        {
            Vector2Int coords = GameManager.Instance.GetBingoCoordinatesForTile(tileName);
            return coords.x != -1 && coords.y != -1;
        }
        return false;
    }

    public bool RequestPlayerMovement(PlayerMovementType targetType, int targetValue = -1)
    {
        if (isMoving)
        {
            return false;
        }

        // 이동 타입에 따라 적절한 메서드 호출
        switch (targetType)
        {
            case PlayerMovementType.DiceResult:
                if (targetValue > 0)
                {
                    MovePlayer(targetValue);
                    return true;
                }
                else
                {
                    return false;
                }

            case PlayerMovementType.TeleportToTile:
                if (IsValidTileIndex(targetValue))
                {
                    TeleportToTile(targetValue);
                    return true;
                }
                else
                {
                    return false;
                }

            case PlayerMovementType.ReturnToStart:
                MoveToStart();
                return true;

            default:
                return false;
        }
    }

    public void SetPlayerPositionImmediate(Vector3 targetPosition, bool updateGameState = false)
    {
        Debug.Log($"즉시 위치 변경: {playerTransform.position} → {targetPosition}");
        
        // 즉시 위치 변경
        playerTransform.position = targetPosition;
        
        if (updateGameState)
        {
            // 게임 상태 업데이트 
            Debug.Log("게임 상태 업데이트와 함께 위치 변경 완료");
        }
    }

    // 현재 플레이어가 특정 타일에 있는지 확인
    public bool IsPlayerOnTile(int tileIndex)
    {
        return currentTileIndex == tileIndex;
    }

    // 플레이어가 시작 위치에 있는지 확인
    public bool IsPlayerAtStart()
    {
        return currentTileIndex == -1;
    }
    
    // 현재 플레이어 상태를 문자열로 반환
    public string GetPlayerStatusString()
    {
        if (isMoving)
        {
            return "이동 중...";
        }
        
        string locationInfo = GetCurrentTileName();
        string positionInfo = $"위치: {GetPlayerPosition():F1}";
        
        return $"{locationInfo} ({positionInfo})";
    }

    public enum PlayerMovementType
    {
        DiceResult,        // 주사위 결과에 따른 일반 이동
        TeleportToTile,    // 특정 타일로 텔레포트
        ReturnToStart      // 시작 위치로 복귀
    }

    // 타일 위치에 오프셋 적용
    private Vector3 CalculatePlayerPositionOnTile(Transform tile)
    {
        // 타일의 기본 위치 계산
        Vector3 basePosition = CalculateSafeLandingPosition(tile);
        
        // 오프셋 적용 (Z축 방향으로 앞쪽에 배치)
        Vector3 offsetPosition = basePosition + tilePositionOffset;
        return offsetPosition;
    }
}