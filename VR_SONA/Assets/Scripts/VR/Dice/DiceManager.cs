using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiceManager : MonoBehaviour
{
    // ================================ //
    // Singleton Pattern
    // ================================ //
    public static DiceManager Instance { get; private set; }
    public GameManager gameManager;

    [Header("Scene Management")]
    [SerializeField] private string diceSceneName = "DiceScene";
    
    [Header("References")]
    public Transform playerTransform;
    
    [Header("Debugging")]
    public bool showDebugLogs = true;
    
    // 상태 관리 변수들
    private bool isDiceSceneLoaded = false;
    private DiceSceneManager currentDiceSceneManager;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // DiceScene 로드 
    public void LoadDiceScene()
    {
        // DiceButton 비활성화
        SetDiceButtonVisible(false);

        if (isDiceSceneLoaded)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("주사위 씬이 이미 로드되어 있습니다.");
            }
            return;
        }

        DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
        if (sceneManager != null)
        {
            if (showDebugLogs)
            {
                Debug.Log("기존 DiceSceneManager 발견 - 재설정 진행");
            }

            sceneManager.ResetDice(); // 주사위 초기화
            PlayerManager pm = FindObjectOfType<PlayerManager>();
            if (pm != null)
            {
                sceneManager.playerManager = pm;
                sceneManager.AlignSceneToPlayer();
            }

            isDiceSceneLoaded = true;
            SetupDiceScene();
            return;
        }

        // DiceSceneManager가 없다면 새로 씬 로드
        StartCoroutine(LoadDiceSceneCoroutine());
    }
    
    private IEnumerator LoadDiceSceneCoroutine()
    {
        if (showDebugLogs)
        {
            Debug.Log(" 주사위 씬 로드 시작");
        }

        // 주사위 씬을 추가로 로드
        var asyncLoad = SceneManager.LoadSceneAsync(diceSceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        isDiceSceneLoaded = true;

        // 주사위 씬 설정 및 콜백 등록
        SetupDiceScene();

        if (showDebugLogs)
        {
            Debug.Log(" 주사위 씬 로드 완료");
        }
    }

    // DiceSceneManager와 DiceManager를 연결
    private void SetupDiceScene()
    {
        // DiceSceneManager 찾기
        currentDiceSceneManager = FindObjectOfType<DiceSceneManager>();
        if (currentDiceSceneManager == null)
        {
            Debug.LogError("DiceSceneManager를 찾을 수 없습니다!");
            return;
        }

        // PlayerManager 찾기 및 씬 초기화
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            currentDiceSceneManager.InitializeScene(playerManager);
            
            if (showDebugLogs)
            {
                Debug.Log("PlayerManager 연결 및 씬 정렬 완료");
            }
        }
        else
        {
            Debug.LogError("PlayerManager를 찾을 수 없습니다!");
        }

        // 콜백 등록
        currentDiceSceneManager.SetCallbacks(
            OnDiceResultReceived,    // 주사위 결과를 받았을 때
            OnDiceSceneComplete      // 주사위 씬이 완료되었을 때
        );

        if (showDebugLogs)
        {
            Debug.Log("DiceSceneManager 콜백 등록 완료");
        }
    }

    // 콜백 메소드들 - DiceSceneManager에서 호출
    // 주사위 결과를 받았을 때 호출
    private void OnDiceResultReceived(int result)
    {
        if (showDebugLogs)
        {
            Debug.Log($"주사위 결과 수신: {result}");
        }
        
        // DiceResultUI를 직접 찾아서 처리
        DiceResultUI diceResultUI = FindObjectOfType<DiceResultUI>();
        if (diceResultUI != null)
        {
            diceResultUI.ShowResult(result, () => {
                // UI 표시 완료 후 GameManager에게 플레이어 이동 요청
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnDiceRolled(result);
                }
            });
            
            if (showDebugLogs)
            {
                Debug.Log($"DiceResultUI를 통해 결과 {result} 표시 완료");
            }
        }
        else
        {
            Debug.LogError("DiceResultUI를 찾을 수 없습니다!");
            
            // UI가 없어도 게임은 계속 진행되도록 GameManager 호출
            // if (GameManager.Instance != null)
            // {
            //     GameManager.Instance.OnDiceRolled(result);
            // }
        }
    }
    // DiceScene 언로드 및 MainGameScene 복귀
    private void OnDiceSceneComplete()
    {
        if (showDebugLogs)
        {
            Debug.Log("주사위 씬 완료 - 언로드 시작");
        }

        UnloadDiceScene();

        SetDiceButtonVisible(true);
    }

    // ================================ //
    // 주사위 씬 언로드
    // ================================ //
    public void UnloadDiceScene()
    {
        if (!isDiceSceneLoaded)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ 주사위 씬이 로드되어 있지 않습니다.");
            }
            return;
        }

        StartCoroutine(UnloadDiceSceneCoroutine());
    }

    private IEnumerator UnloadDiceSceneCoroutine()
    {
        if (showDebugLogs)
        {
            Debug.Log("🎲 주사위 씬 언로드 시작");
        }
        
        Scene diceScene = SceneManager.GetSceneByName(diceSceneName);
        
        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            Debug.LogWarning("⚠️ 주사위 씬을 찾을 수 없습니다.");
            isDiceSceneLoaded = false;
            currentDiceSceneManager = null;
            yield break;
        }

        // 주사위 씬 언로드
        var asyncUnload = SceneManager.UnloadSceneAsync(diceSceneName);
        yield return new WaitUntil(() => asyncUnload.isDone);
        
        isDiceSceneLoaded = false;
        currentDiceSceneManager = null;
        
        if (showDebugLogs)
        {
            Debug.Log("✅ 주사위 씬 언로드 완료");
        }
    }

    // ================================ //
    // UI 제어 - UIManager에게 위임
    // ================================ //
    
    /// <summary>
    /// 주사위 버튼의 표시/숨김을 제어합니다.
    /// 실제 UI 조작은 UIManager에게 위임하여 책임을 분리합니다.
    /// </summary>
    public void SetDiceButtonVisible(bool visible)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🎲 주사위 UI {(visible ? "활성화" : "비활성화")} 요청 - UIManager에게 위임");
        }
        
        // UIManager를 통한 표준 방식으로 UI 제어
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(visible);
            
            if (showDebugLogs)
            {
                Debug.Log("✅ UIManager를 통한 주사위 UI 제어 성공");
            }
        }
        else
        {
            // UIManager.Instance가 없을 경우 직접 찾기 시도
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowDiceUI(visible);
                
                if (showDebugLogs)
                {
                    Debug.Log("✅ FindObjectOfType으로 UIManager를 찾아서 제어 성공");
                }
            }
            else
            {
                Debug.LogError("❌ UIManager를 찾을 수 없습니다! 주사위 UI 제어 실패");
            }
        }
    }

    // ================================ //
    // 외부 인터페이스 메소드들 (기존 코드와의 호환성 유지)
    // ================================ //
    
    /// <summary>
    /// 기존 코드와의 호환성을 위한 메소드입니다.
    /// 뒤로가기 버튼이 눌렸을 때 호출됩니다.
    /// </summary>
    public void OnBackButtonClicked(bool showButtonAfter = true)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🔙 뒤로가기 버튼 클릭됨 - showButtonAfter: {showButtonAfter}");
        }

        // 현재 DiceSceneManager가 있다면 뒤로가기 처리 요청
        if (currentDiceSceneManager != null)
        {
            currentDiceSceneManager.OnBackButtonPressed();
        }
        else
        {
            // DiceSceneManager가 없다면 직접 언로드
            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ DiceSceneManager가 없어서 직접 언로드 처리");
            }
            
            UnloadDiceScene();
            
            // 버튼 표시 옵션에 따라 UI 제어
            if (showButtonAfter)
            {
                SetDiceButtonVisible(true);
            }
        }
    }

    /// <summary>
    /// 기존 코드와의 호환성을 위한 메소드입니다.
    /// 외부에서 주사위 버튼 클릭을 시뮬레이션할 때 사용합니다.
    /// </summary>
    public void DiceButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("🎲 주사위 버튼 클릭 이벤트 수신");
        }
        
        LoadDiceScene();
    }

    // ================================ //
    // 상태 확인 메소드들
    // ================================ //
    
    /// <summary>
    /// 현재 주사위 씬이 로드되어 있는지 확인합니다.
    /// </summary>
    public bool IsDiceSceneLoaded()
    {
        return isDiceSceneLoaded;
    }

    /// <summary>
    /// 현재 주사위 결과를 처리 중인지 확인합니다.
    /// </summary>
    public bool IsProcessingDiceResult()
    {
        if (currentDiceSceneManager != null)
        {
            return currentDiceSceneManager.IsProcessingResult();
        }
        return false;
    }

    // ================================ //
    // 에러 처리 및 정리 메소드들
    // ================================ //
    
    /// <summary>
    /// 강제로 주사위 시스템을 정리합니다. (긴급 상황용)
    /// 예상치 못한 오류가 발생했을 때 시스템을 안전한 상태로 복구하는 데 사용됩니다.
    /// </summary>
    public void ForceCleanup()
    {
        if (showDebugLogs)
        {
            Debug.Log("⛔ 주사위 시스템 강제 정리 시작");
        }

        // 진행 중인 모든 코루틴 중단
        StopAllCoroutines();

        // DiceSceneManager의 작업 중단
        if (currentDiceSceneManager != null)
        {
            currentDiceSceneManager.ForceStopResultProcessing();
        }

        // 씬이 로드되어 있다면 강제 언로드
        if (isDiceSceneLoaded)
        {
            StartCoroutine(ForceUnloadDiceScene());
        }

        // 상태 초기화
        ResetManagerState();

        // UI 복구 - 주사위 버튼 다시 표시
        SetDiceButtonVisible(true);

        if (showDebugLogs)
        {
            Debug.Log("✅ 주사위 시스템 강제 정리 완료");
        }
    }

    /// <summary>
    /// 강제 언로드를 위한 별도 코루틴입니다.
    /// 일반적인 언로드 과정에서 오류가 발생했을 때 사용됩니다.
    /// </summary>
    private IEnumerator ForceUnloadDiceScene()
    {
        Scene diceScene = SceneManager.GetSceneByName(diceSceneName);
        
        if (diceScene.IsValid() && diceScene.isLoaded)
        {
            var asyncUnload = SceneManager.UnloadSceneAsync(diceSceneName);
            
            // 최대 5초까지만 대기 (무한 대기 방지)
            float timeout = 5f;
            float elapsed = 0f;
            
            while (!asyncUnload.isDone && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!asyncUnload.isDone)
            {
                Debug.LogError("❌ 주사위 씬 강제 언로드 타임아웃!");
            }
        }
        
        ResetManagerState();
    }

    /// <summary>
    /// 매니저의 내부 상태를 초기 상태로 리셋합니다.
    /// </summary>
    private void ResetManagerState()
    {
        isDiceSceneLoaded = false;
        currentDiceSceneManager = null;
        
        if (showDebugLogs)
        {
            Debug.Log("🔄 DiceManager 상태 리셋 완료");
        }
    }

    // ================================ //
    // Unity 생명주기 메소드들
    // ================================ //
    
    /// <summary>
    /// 매니저가 파괴될 때 정리 작업을 수행합니다.
    /// 메모리 누수를 방지하고 안전한 종료를 보장합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (showDebugLogs)
        {
            Debug.Log("🗑️ DiceManager 파괴 - 정리 작업 수행");
        }

        // 진행 중인 작업들 정리
        StopAllCoroutines();
        
        // 씬이 로드되어 있다면 정리
        if (isDiceSceneLoaded)
        {
            // 비동기 언로드는 파괴 시점에서 시작만 하고 완료를 기다리지 않음
            if (SceneManager.GetSceneByName(diceSceneName).IsValid())
            {
                SceneManager.UnloadSceneAsync(diceSceneName);
            }
        }
        
        // 참조 정리
        currentDiceSceneManager = null;
    }

    /// <summary>
    /// 애플리케이션이 포커스를 잃었을 때의 처리입니다.
    /// VR 환경에서는 헤드셋을 벗었을 때 등의 상황에서 호출될 수 있습니다.
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && IsProcessingDiceResult())
        {
            if (showDebugLogs)
            {
                Debug.Log("📱 애플리케이션 포커스 잃음 - 주사위 처리 일시 정지");
            }
            
            // 필요하다면 여기서 게임을 일시 정지하거나 상태를 저장할 수 있습니다
            // 예: Time.timeScale = 0f;
        }
    }

    // ================================ //
    // 디버그 및 개발 도구 메소드들
    // ================================ //
    
    /// <summary>
    /// 개발 중 테스트를 위한 메소드입니다.
    /// Inspector에서 호출하거나 디버그 콘솔에서 사용할 수 있습니다.
    /// </summary>
    [ContextMenu("Debug: Force Load Dice Scene")]
    public void DebugLoadDiceScene()
    {
        if (Application.isPlaying)
        {
            Debug.Log("🛠️ 디버그: 주사위 씬 강제 로드");
            LoadDiceScene();
        }
    }

    [ContextMenu("Debug: Force Unload Dice Scene")]
    public void DebugUnloadDiceScene()
    {
        if (Application.isPlaying)
        {
            Debug.Log("🛠️ 디버그: 주사위 씬 강제 언로드");
            UnloadDiceScene();
        }
    }

    [ContextMenu("Debug: Show Current State")]
    public void DebugShowCurrentState()
    {
        Debug.Log($"🛠️ DiceManager 현재 상태:");
        Debug.Log($"   - 씬 로드됨: {isDiceSceneLoaded}");
        Debug.Log($"   - DiceSceneManager 연결됨: {(currentDiceSceneManager != null)}");
        Debug.Log($"   - 결과 처리 중: {IsProcessingDiceResult()}");
    }
}