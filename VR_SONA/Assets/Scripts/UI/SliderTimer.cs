using UnityEngine;
using UnityEngine.UI;

public class SliderTimer : MonoBehaviour
{
    // Singleton & 컴포넌트
    public static SliderTimer Instance;
    private Slider sd;
    
    // 타이머 변수들
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
        
        TimeRemain = PlayerState.InitialGameTime;
        MaxTime = PlayerState.InitialGameTime;
        
        sd.maxValue = MaxTime;
        sd.value = TimeRemain;
        
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
            hasTimeEnded = true; 
            TimeRemain = 0f;    
            
            OnTimeUp();
        }
    }

    // 시간 종료 처리
    private void OnTimeUp()
    {

        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.EndGameDueToTimeUp();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeUp();
        }
        else
        {
            Debug.LogError("GameEndManager와 GameManager 모두 찾을 수 없습니다!");
        }
    }

    public void AddTime(float seconds)
    {
        // 이미 시간이 끝났으면 시간 추가를 허용하지 않음
        if (hasTimeEnded) 
        {
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

    // 타이머 제어 메소드 
    public void PauseTimer()
    {
        enabled = false; // Update 중단
    }

    public void ResumeTimer()
    {
        if (!hasTimeEnded)
        {
            enabled = true; // Update 재개
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
        
        enabled = true; 
    }
}