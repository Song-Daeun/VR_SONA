using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiceManager : MonoBehaviour
{
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

        currentDiceSceneManager.SetCallbacks(
            OnDiceResultReceived,    // 주사위 결과를 받았을 때
            OnDiceSceneComplete      // 주사위 씬이 완료되었을 때
        );

        if (showDebugLogs)
        {
            Debug.Log("DiceSceneManager 콜백 등록 완료");
        }
    }

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
    }

    // 주사위 씬 언로드
    public void UnloadDiceScene()
    {
        if (!isDiceSceneLoaded)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("주사위 씬이 로드되어 있지 않습니다.");
            }
            return;
        }

        StartCoroutine(UnloadDiceSceneCoroutine());
    }

    private IEnumerator UnloadDiceSceneCoroutine()
    {
        if (showDebugLogs)
        {
            Debug.Log(" 주사위 씬 언로드 시작");
        }
        
        Scene diceScene = SceneManager.GetSceneByName(diceSceneName);
        
        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
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
            Debug.Log("주사위 씬 언로드 완료");
        }
    }

    // UI 제어 
    public void SetDiceButtonVisible(bool visible)
    {
        if (showDebugLogs)
        {
            Debug.Log($"주사위 UI {(visible ? "활성화" : "비활성화")} 요청 - UIManager에게 위임");
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDiceUI(visible);
            
            if (showDebugLogs)
            {
                Debug.Log("UIManager를 통한 주사위 UI 제어 성공");
            }
        }
        else
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowDiceUI(visible);
                
                if (showDebugLogs)
                {
                    Debug.Log("FindObjectOfType으로 UIManager를 찾아서 제어 성공");
                }
            }
            else
            {
                Debug.LogError("UIManager를 찾을 수 없습니다! 주사위 UI 제어 실패");
            }
        }
    }

    public void DiceButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("주사위 버튼 클릭 이벤트 수신");
        }
        
        LoadDiceScene();
    }

    public bool IsDiceSceneLoaded()
    {
        return isDiceSceneLoaded;
    }

    public bool IsProcessingDiceResult()
    {
        if (currentDiceSceneManager != null)
        {
            return currentDiceSceneManager.IsProcessingResult();
        }
        return false;
    }

    public void ForceCleanup()
    {
        if (showDebugLogs)
        {
            Debug.Log("주사위 시스템 강제 정리 시작");
        }

        StopAllCoroutines();

        if (currentDiceSceneManager != null)
        {
            currentDiceSceneManager.ForceStopResultProcessing();
        }

        if (isDiceSceneLoaded)
        {
            StartCoroutine(ForceUnloadDiceScene());
        }

        ResetManagerState();
        SetDiceButtonVisible(true);

        if (showDebugLogs)
        {
            Debug.Log("주사위 시스템 강제 정리 완료");
        }
    }

    private IEnumerator ForceUnloadDiceScene()
    {
        Scene diceScene = SceneManager.GetSceneByName(diceSceneName);
        
        if (diceScene.IsValid() && diceScene.isLoaded)
        {
            var asyncUnload = SceneManager.UnloadSceneAsync(diceSceneName);
            
            float timeout = 5f;
            float elapsed = 0f;
            
            while (!asyncUnload.isDone && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!asyncUnload.isDone)
            {
                Debug.LogError("주사위 씬 강제 언로드 타임아웃!");
            }
        }
        
        ResetManagerState();
    }

    private void ResetManagerState()
    {
        isDiceSceneLoaded = false;
        currentDiceSceneManager = null;
        
        if (showDebugLogs)
        {
            Debug.Log("DiceManager 상태 리셋 완료");
        }
    }

    private void OnDestroy()
    {
        if (showDebugLogs)
        {
            Debug.Log("DiceManager 파괴 - 정리 작업 수행");
        }

        StopAllCoroutines();
        if (isDiceSceneLoaded)
        {
            if (SceneManager.GetSceneByName(diceSceneName).IsValid())
            {
                SceneManager.UnloadSceneAsync(diceSceneName);
            }
        }
        currentDiceSceneManager = null;
    }
}