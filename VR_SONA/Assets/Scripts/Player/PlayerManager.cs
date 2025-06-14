using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Player Settings")]
    public Transform playerTransform;
    public List<Transform> tileList;
    public Transform startTile; // Start 타일 추가
    public float moveDuration = 0.5f;
    public float teleportDuration = 0.1f; // 텔레포트용 빠른 이동

    [Header("Landing Settings")]
    public float heightOffset = 9.0f;
    public LayerMask groundLayerMask = -1;
    public float raycastDistance = 10.0f;

    private bool isMoving = false;
    private int currentTileIndex = 0;
    
    // 현재 이동 중인 주사위 결과를 저장
    private int currentDiceResult = -1;

    // 타일 도착 이벤트 
    public static System.Action<string, int> OnTileArrived;
    public static System.Action OnSpellBookTileArrived;

    // 오프셋 설정 추가
    [Header("Position Offset Settings")]
    public Vector3 tilePositionOffset = new Vector3(0f, 0f, -50f); // Z축으로 -10만큼 앞쪽에 배치

    
    // 싱글톤 초기화
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("PlayerManager 싱글톤 인스턴스가 생성되었습니다.");
        }
        else
        {
            Debug.LogWarning("PlayerManager 인스턴스가 이미 존재합니다. 중복 인스턴스를 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        // 필수 컴포넌트 검증
        ValidateComponents();
    }

    // Start 메서드에서 게임 시작 시 플레이어 위치 초기화
    private void Start()
    {
        Debug.Log("PlayerManager Start() 호출 - 초기 위치 설정 시작");
        StartCoroutine(InitializePlayerPosition());
    }

    // 게임 시작 시 플레이어 위치 초기화 코루틴
    private IEnumerator InitializePlayerPosition()
    {
        Debug.Log("=== 플레이어 초기 위치 설정 시작 ===");
        
        // 한 프레임 대기 (다른 컴포넌트들의 Awake가 완료되도록)
        yield return null;
        
        // 시작 타일이 설정되어 있다면 해당 위치로 이동
        if (startTile != null)
        {
            Debug.Log("시작 타일이 설정되어 있음. 시작 위치로 이동 시작");
            MoveToStart();
        }
        else
        {
            // 시작 타일이 없다면 현재 위치를 그대로 사용하되, 안전한 높이로 조정
            Debug.LogWarning("시작 타일이 설정되지 않음. 현재 위치에서 높이만 조정");
            
            Vector3 currentPos = playerTransform.position;
            Vector3 safePosition = new Vector3(currentPos.x, currentPos.y + heightOffset, currentPos.z);
            playerTransform.position = safePosition;
            
            // 초기 상태 설정
            currentTileIndex = -1; // 시작 상태
            currentDiceResult = -1;
            
            Debug.Log($"플레이어 초기 위치 설정 완료: {safePosition}");
        }
        
        Debug.Log("=== 플레이어 초기 위치 설정 완료 ===");
    }

    // 컴포넌트 유효성 검사
    private void ValidateComponents()
    {
        if (playerTransform == null)
        {
            // playerTransform이 설정되지 않았다면 자동으로 현재 GameObject의 Transform을 사용
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
            Debug.Log("PlayerManager 싱글톤 인스턴스가 정리되었습니다.");
        }
    }

    // 외부에서 접근 가능한 플레이어 상태 정보
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

    // 기존 일반 이동 (주사위용) - 수정됨
    public void MovePlayer(int diceResult)
    {
        Debug.Log($"=== PlayerManager.MovePlayer 호출됨 ===");
        Debug.Log($"주사위 결과: {diceResult}");
        Debug.Log($"현재 isMoving: {isMoving}");
        
        if (isMoving)
        {
            Debug.Log("이미 이동 중이므로 요청 무시");
            return;
        }

        if (tileList == null || tileList.Count == 0)
        {
            Debug.LogError("tileList가 null이거나 비어있음");
            return;
        }

        int targetIndex = diceResult - 1;
        Debug.Log($"계산된 targetIndex: {targetIndex}");
        
        if (targetIndex < 0 || targetIndex >= tileList.Count)
        {
            Debug.LogError($"targetIndex가 범위를 벗어남: {targetIndex}");
            return;
        }

        Transform targetTile = tileList[targetIndex];
        if (targetTile == null)
        {
            Debug.LogError($"targetTile이 null임 (인덱스: {targetIndex})");
            return;
        }

        // 현재 주사위 결과 저장 (이동 완료 후 GameManager에게 전달하기 위함)
        currentDiceResult = diceResult;
        currentTileIndex = targetIndex; // 타일 인덱스도 업데이트

        Debug.Log($"목표 타일: {targetTile.name}");
        Vector3 targetPosition = CalculatePlayerPositionOnTile(targetTile);
        Debug.Log($"목표 위치: {targetPosition}");
        
        // 일반 이동에서는 미션 메시지를 PlayerManager에서 표시하지 않음
        // GameManager가 이동 완료 신호를 받은 후 직접 처리함
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false, true));
        Debug.Log("MoveToPosition 코루틴 시작됨 (GameManager 알림 포함)");
    }

    // 텔레포트 (즉시 이동) 
    public void TeleportToTile(int tileIndex)
    {
        Debug.Log($"=== PlayerManager.TeleportToTile 호출됨 ===");
        Debug.Log($"타일 인덱스: {tileIndex}");
        
        if (isMoving)
        {
            Debug.Log("이미 이동 중이므로 텔레포트 요청 무시");
            return;
        }

        if (tileList == null || tileList.Count == 0)
        {
            Debug.LogError("tileList가 null이거나 비어있음");
            return;
        }

        if (tileIndex < 0 || tileIndex >= tileList.Count)
        {
            Debug.LogError($"tileIndex가 범위를 벗어남: {tileIndex}");
            return;
        }

        Transform targetTile = tileList[tileIndex];
        if (targetTile == null)
        {
            Debug.LogError($"targetTile이 null임 (인덱스: {tileIndex})");
            return;
        }

        // 텔레포트의 경우 주사위 결과는 타일 인덱스 + 1로 설정
        currentDiceResult = tileIndex + 1;
        currentTileIndex = tileIndex; // 타일 인덱스 업데이트

        Vector3 targetPosition = CalculatePlayerPositionOnTile(targetTile);
        Debug.Log($"텔레포트 목표 위치: {targetPosition}");
        
        StartCoroutine(MoveToPosition(targetPosition, teleportDuration, false, true));
        Debug.Log("텔레포트 코루틴 시작됨 (GameManager 알림 포함)");
    }

    // Start 타일로 이동 
    public void MoveToStart()
    {
        Debug.Log("=== PlayerManager.MoveToStart 호출됨 ===");
        
        if (isMoving)
        {
            Debug.Log("이미 이동 중이므로 Start 이동 요청 무시");
            return;
        }

        if (startTile == null)
        {
            Debug.LogError("Start 타일이 설정되지 않았습니다!");
            return;
        }

        // Start 타일로 이동할 때는 주사위 결과를 -1로 설정 (Start 타일 의미)
        currentDiceResult = -1;
        currentTileIndex = -1; // Start 타일은 -1 인덱스

        Vector3 targetPosition = CalculatePlayerPositionOnTile(startTile);
        Debug.Log($"Start 타일 목표 위치: {targetPosition}");
        
        // Start 타일로 이동할 때는 GameManager에게 알리지 않음 (특별한 경우)
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, false, false));
        Debug.Log("Start 타일 이동 코루틴 시작됨 (GameManager 알림 없음)");
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration = -1, bool showMission = false, bool notifyGameManager = false)
    {        
        if (duration < 0) duration = moveDuration;

        // CharacterController 비활성화 (물리 충돌 방지)
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) 
        {
            cc.enabled = false;
            Debug.Log("CharacterController 비활성화됨");
        }

        Vector3 startPosition = playerTransform.position;
        float elapsed = 0f;
        isMoving = true;
        
        Debug.Log($"이동 시작: {startPosition} → {targetPosition}");

        // 실제 이동 애니메이션
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 정확히 설정
        playerTransform.position = targetPosition;
        Debug.Log($"이동 완료: 최종 위치 = {playerTransform.position}");

        // CharacterController 재활성화
        if (cc != null) 
        {
            cc.enabled = true;
            Debug.Log("CharacterController 재활성화됨");
        }

        // 이동 상태 플래그 해제
        isMoving = false;
        Debug.Log("isMoving = false로 설정됨");

        // if (notifyGameManager && GameManager.Instance != null)
        // {
        //     Debug.Log($"GameManager에게 이동 완료 알림 - 주사위 결과: {currentDiceResult}");
        //     yield return null;
            
        //     GameManager.Instance.OnPlayerMovementCompleted(currentDiceResult);
            
        //     Debug.Log("GameManager 알림 완료");
        // }
        // else if (notifyGameManager && GameManager.Instance == null)
        // {
        //     Debug.LogError("GameManager.Instance가 null입니다! 이동 완료 알림 실패");
        // }

        // 🔥 새로운 이벤트 기반 알림 시스템
        if (notifyGameManager)
        {
            // 현재 타일 이름 확인
            string currentTileName = GetCurrentTileName();
            Debug.Log($"도착한 타일: {currentTileName} (인덱스: {currentTileIndex})");
            
            // 일반 타일 도착 이벤트 발생
            OnTileArrived?.Invoke(currentTileName, currentTileIndex);
            
            // SpellBook 타일인 경우 특별 이벤트도 발생
            if (currentTileName == "SpellBook")
            {
                Debug.Log("🔮 SpellBook 타일 감지! 전용 이벤트 발생");
                OnSpellBookTileArrived?.Invoke();
            }
        }

        Debug.Log("=== MoveToPosition 코루틴 완료 ===");
    }
    
    private Vector3 CalculateSafeLandingPosition(Transform tile)
    {
        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider == null)
        {
            Debug.Log($"타일 {tile.name}에 Collider가 없음. 기본 높이 오프셋 사용");
            return tile.position + Vector3.up * heightOffset;
        }

        Bounds bounds = tileCollider.bounds;
        Vector3 safePosition = new Vector3(bounds.center.x, bounds.max.y + heightOffset, bounds.center.z);

        Debug.Log($"타일 {tile.name}의 안전한 착지 위치 계산: {safePosition}");
        return safePosition;
    }

    // 현재 타일 인덱스 설정 (GameManager에서 호출)
    public void SetCurrentTileIndex(int index)
    {
        currentTileIndex = index;
        Debug.Log($"PlayerManager 타일 인덱스 설정: {index}");
    }

    // 디버그용 현재 상태 출력 - 싱글톤 버전에서는 활성화
    public void DebugCurrentState()
    {
        Debug.Log($"=== PlayerManager 현재 상태 ===");
        Debug.Log($"싱글톤 인스턴스: {(Instance != null ? "활성" : "비활성")}");
        Debug.Log($"isMoving: {isMoving}");
        Debug.Log($"currentTileIndex: {currentTileIndex}");
        Debug.Log($"currentDiceResult: {currentDiceResult}");
        Debug.Log($"플레이어 위치: {GetPlayerPosition()}");
        Debug.Log($"플레이어 Transform: {(playerTransform != null ? playerTransform.name : "null")}");
        
        if (tileList != null)
        {
            Debug.Log($"사용 가능한 타일 수: {tileList.Count}");
        }
        
        Debug.Log($"Start 타일: {(startTile != null ? startTile.name : "설정되지 않음")}");
    }

    // 추가 유틸리티 메서드들
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
        // Start 타일 처리 - 특별한 경우이므로 별도 처리
        if (currentTileIndex == -1)
        {
            return startTile != null ? NormalizeTileName(startTile.name) : "Start";
        }
        
        // 일반 타일 처리 - 유효한 인덱스인지 확인 후 정규화된 이름 반환
        if (IsValidTileIndex(currentTileIndex))
        {
            string rawTileName = tileList[currentTileIndex].name;
            string normalizedName = NormalizeTileName(rawTileName);
            
            // 디버깅을 위한 로그 출력 (개발 중에만 활성화)
            Debug.Log($"타일 이름 변환: '{rawTileName}' → '{normalizedName}'");
            
            return normalizedName;
        }
        
        // 예외 상황 처리
        Debug.LogWarning($"유효하지 않은 타일 인덱스: {currentTileIndex}");
        return "Unknown";
    }

    /// <summary>
    /// GameObject의 실제 이름을 게임 로직에서 사용하는 표준 이름으로 변환합니다.
    /// 예: "SpellBookTile" → "SpellBook", "GermanyTile" → "Germany"
    /// </summary>
    /// <param name="rawName">GameObject의 원시 이름</param>
    /// <returns>정규화된 타일 이름</returns>
    private string NormalizeTileName(string rawName)
    {
        // 입력 검증 - null이나 빈 문자열 처리
        if (string.IsNullOrEmpty(rawName))
        {
            Debug.LogWarning("타일 이름이 비어있습니다!");
            return "Unknown";
        }
        
        // 이름 정규화 과정 시작
        string normalizedName = rawName.Trim(); // 앞뒤 공백 제거
        
        // "Tile" 접미사 제거 - 대소문자 구분 없이 처리
        if (normalizedName.EndsWith("Tile", System.StringComparison.OrdinalIgnoreCase))
        {
            // "Tile" 부분을 제거하여 순수한 타일 이름만 추출
            normalizedName = normalizedName.Substring(0, normalizedName.Length - 4);
            Debug.Log($"'Tile' 접미사 제거됨: {rawName} → {normalizedName}");
        }
        
        // 추가적인 정리 작업들
        normalizedName = normalizedName.Trim(); // 다시 한번 공백 제거
        
        // 특별한 경우들에 대한 추가 처리
        normalizedName = HandleSpecialCases(normalizedName);
        
        // 최종 검증 - 빈 문자열이 되었다면 원본 이름 사용
        if (string.IsNullOrEmpty(normalizedName))
        {
            Debug.LogWarning($"정규화 과정에서 이름이 사라졌습니다. 원본 사용: {rawName}");
            return rawName;
        }
        
        return normalizedName;
    }

    /// <summary>
    /// 특별한 타일 이름들에 대한 추가 처리를 수행합니다.
    /// 예: 대소문자 통일, 특수 문자 처리 등
    /// </summary>
    /// <param name="tileName">기본 정규화가 완료된 타일 이름</param>
    /// <returns>특별 처리가 완료된 타일 이름</returns>
    private string HandleSpecialCases(string tileName)
    {
        // 특별한 경우들을 위한 매핑 테이블
        // 이 방법을 사용하면 나중에 새로운 특별 케이스를 쉽게 추가할 수 있습니다
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
        
        // 특별 케이스가 아니라면 원본 그대로 반환
        return tileName;
    }

    /// <summary>
    /// 개발자를 위한 디버깅 메서드 - 현재 타일 이름 변환 과정을 상세히 출력합니다.
    /// </summary>
    public void DebugTileNameConversion()
    {
        Debug.Log("=== 타일 이름 변환 디버깅 시작 ===");
        
        if (currentTileIndex == -1)
        {
            string startName = startTile != null ? startTile.name : "null";
            Debug.Log($"현재 위치: Start 타일 (원본: {startName})");
            Debug.Log($"변환된 이름: {GetCurrentTileName()}");
        }
        else if (IsValidTileIndex(currentTileIndex))
        {
            string rawName = tileList[currentTileIndex].name;
            string normalizedName = GetCurrentTileName();
            Debug.Log($"현재 위치: 인덱스 {currentTileIndex}");
            Debug.Log($"GameObject 원본 이름: '{rawName}'");
            Debug.Log($"정규화된 이름: '{normalizedName}'");
            
            // GameManager의 매핑 테이블과 비교
            Debug.Log($"GameManager 매핑 존재 여부: {CheckIfMappingExists(normalizedName)}");
        }
        else
        {
            Debug.LogError($"유효하지 않은 인덱스: {currentTileIndex}");
        }
        
        Debug.Log("=== 타일 이름 변환 디버깅 완료 ===");
    }

    /// <summary>
    /// GameManager의 tileToCoords 딕셔너리에 해당 이름이 존재하는지 확인합니다.
    /// 이 메서드는 디버깅 목적으로만 사용됩니다.
    /// </summary>
    /// <param name="tileName">확인할 타일 이름</param>
    /// <returns>매핑이 존재하면 true, 그렇지 않으면 false</returns>
    private bool CheckIfMappingExists(string tileName)
    {
        // GameManager의 인스턴스가 있다면 매핑 확인
        if (GameManager.Instance != null)
        {
            Vector2Int coords = GameManager.Instance.GetBingoCoordinatesForTile(tileName);
            return coords.x != -1 && coords.y != -1;
        }
        
        // GameManager가 없다면 확인할 수 없음
        Debug.LogWarning("GameManager.Instance가 null이어서 매핑을 확인할 수 없습니다.");
        return false;
    }
    
    // 다른 시스템에서 플레이어 이동을 요청할 때 사용
    public bool RequestPlayerMovement(PlayerMovementType targetType, int targetValue = -1)
    {
        Debug.Log($"=== 플레이어 이동 요청 받음 ===");
        Debug.Log($"이동 타입: {targetType}, 목표 값: {targetValue}");

        // 이동 중이면 요청 거부
        if (isMoving)
        {
            Debug.LogWarning("플레이어가 이미 이동 중입니다. 이동 요청이 거부되었습니다.");
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
                    Debug.LogError("주사위 결과 이동에는 1 이상의 값이 필요합니다.");
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
                    Debug.LogError($"유효하지 않은 타일 인덱스입니다: {targetValue}");
                    return false;
                }

            case PlayerMovementType.ReturnToStart:
                MoveToStart();
                return true;

            default:
                Debug.LogError($"알 수 없는 이동 타입입니다: {targetType}");
                return false;
        }
    }
    
    // 즉시 위치 변경 (애니메이션 없이)
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
        
        Debug.Log($"플레이어 위치 즉시 변경 완료: {targetPosition}");
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

    // 플레이어 이동 타입을 정의하는 열거형
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
        
        Debug.Log($"타일 {tile.name} 위치 계산:");
        Debug.Log($"  기본 위치: {basePosition}");
        Debug.Log($"  오프셋 적용: {tilePositionOffset}");
        Debug.Log($"  최종 위치: {offsetPosition}");
        
        return offsetPosition;
    }
}