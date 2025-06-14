using UnityEngine;
using UnityEngine.UI;

public class SliderTimer : MonoBehaviour
{
    // ================================ //
    // Singleton & 컴포넌트
    // ================================ //
    public static SliderTimer Instance;
    private Slider sd;
    
    // ================================ //
    // 타이머 변수들
    // ================================ //
    private float TimeRemain;
    private float MaxTime;
    private bool hasTimeEnded = false; // 중복 호출 방지

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        sd = GetComponent<Slider>();
        
        // PlayerState에서 초기 시간 설정 가져오기
        TimeRemain = PlayerState.InitialGameTime;
        MaxTime = PlayerState.InitialGameTime;
        
        // 슬라이더 초기화
        sd.maxValue = MaxTime;
        sd.value = TimeRemain;
        
        Debug.Log($"⏰ 슬라이더 타이머 초기화: {TimeRemain}/{MaxTime}초");
    }

    void Update()
    {
        // 이미 시간이 끝났으면 더 이상 처리하지 않음
        if (hasTimeEnded) return;

        // 시간 감소
        TimeRemain -= Time.deltaTime;
        
        // 슬라이더 값 업데이트
        sd.value = TimeRemain;
        
        // 시간 종료 체크
        if (TimeRemain <= 0f)
        {
            hasTimeEnded = true; // 중복 호출 방지
            TimeRemain = 0f;     // 시간을 0으로 고정
            
            OnTimeUp();
        }
    }

    // ================================ //
    // 시간 종료 처리 (수정됨)
    // ================================ //
    private void OnTimeUp()
    {
        Debug.Log("⏰ SliderTimer: 시간 종료!");
        
        // 우선순위: GameEndManager → GameManager (fallback)
        if (GameEndManager.Instance != null)
        {
            Debug.Log("⏰ GameEndManager를 통해 시간 만료 처리");
            GameEndManager.Instance.EndGameDueToTimeUp();
        }
        else if (GameManager.Instance != null)
        {
            Debug.Log("⏰ GameManager를 통해 시간 만료 처리 (fallback)");
            GameManager.Instance.OnTimeUp();
        }
        else
        {
            Debug.LogError("❌ GameEndManager와 GameManager 모두 찾을 수 없습니다!");
        }
    }

    // ================================ //
    // 공개 메소드 (다른 스크립트에서 호출)
    // ================================ //
    public void AddTime(float seconds)
    {
        // 이미 시간이 끝났으면 시간 추가를 허용하지 않음
        if (hasTimeEnded) 
        {
            Debug.Log("⏰ 시간이 이미 종료되어 시간 추가가 무시됨");
            return;
        }

        TimeRemain += seconds;
        
        // 최대 시간 제한 (25% 추가까지만 허용)
        float maxAllowedTime = MaxTime * 1.25f;
        if (TimeRemain > maxAllowedTime)
        {
            TimeRemain = maxAllowedTime;
        }
        
        // 슬라이더 값 즉시 업데이트
        if (sd != null)
        {
            sd.value = TimeRemain;
        }
        
        Debug.Log($"⏰ 시간 추가: +{seconds}초, 남은 시간: {TimeRemain:F1}초");
    }

    public float GetRemainingTime()
    {
        return TimeRemain;
    }

    public float GetTimePercentage()
    {
        return TimeRemain / MaxTime;
    }

    public bool IsTimeUp()
    {
        return hasTimeEnded;
    }

    // ================================ //
    // 타이머 제어 메소드 (추가)
    // ================================ //
    public void PauseTimer()
    {
        enabled = false; // Update 중단
        Debug.Log("⏰ 타이머 일시정지");
    }

    public void ResumeTimer()
    {
        if (!hasTimeEnded)
        {
            enabled = true; // Update 재개
            Debug.Log("⏰ 타이머 재개");
        }
    }

    public void ResetTimer()
    {
        hasTimeEnded = false;
        TimeRemain = MaxTime;
        
        if (sd != null)
        {
            sd.value = TimeRemain;
        }
        
        enabled = true; // Update 활성화
        Debug.Log($"⏰ 타이머 리셋: {MaxTime}초");
    }

    // ================================ //
    // 디버그용 (에디터에서만)
    // ================================ //
#if UNITY_EDITOR
    void OnGUI()
    {
        // 에디터에서 현재 시간 표시 (선택사항)
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 10, 200, 30), $"Time: {TimeRemain:F1}s / {MaxTime}s");
            GUI.Label(new Rect(10, 40, 200, 30), $"Ended: {hasTimeEnded}");
        }
    }
#endif
}