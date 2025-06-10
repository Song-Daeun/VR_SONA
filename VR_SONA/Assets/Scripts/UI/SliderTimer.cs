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
        // 시간 감소
        TimeRemain -= Time.deltaTime;
        
        // 슬라이더 값 업데이트
        sd.value = TimeRemain;
        
        // 시간 종료 체크 - GameManager에게 직접 알림
        if (TimeRemain <= 0f)
        {
            Debug.Log("⏰ 시간 종료! GameManager에게 알림");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimeUp();
            }
            else
            {
                Debug.LogError("❌ GameManager.Instance를 찾을 수 없습니다!");
            }
            
            // 중복 호출 방지를 위해 시간을 0으로 고정
            TimeRemain = 0f;
        }
    }

    // ================================ //
    // 공개 메소드 (다른 스크립트에서 호출)
    // ================================ //
    public void AddTime(float seconds)
    {
        TimeRemain += seconds;
        
        // 최대 시간 제한 (25% 추가까지만 허용)
        float maxAllowedTime = MaxTime * 1.25f;
        if (TimeRemain > maxAllowedTime)
        {
            TimeRemain = maxAllowedTime;
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
        return TimeRemain <= 0f;
    }
}