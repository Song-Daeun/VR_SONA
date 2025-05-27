using UnityEngine;
using System.Collections;

public class WaterCollisionHandler : MonoBehaviour
{
    public GameObject successText;
    public GameObject failText;

    private float startTime;
    private bool missionCompleted = false;
    private Coroutine failCoroutine;

    void Start()
    {
        startTime = Time.time;
        successText.SetActive(false);
        failText.SetActive(false);
        failCoroutine = StartCoroutine(FailCheckAfterTime(10f));
        
        Debug.Log($"[WaterCollision] 미션 시작! 시작 시간: {startTime}");
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log($"[WaterCollision] OnParticleCollision 호출됨. missionCompleted: {missionCompleted}");
        
        if (missionCompleted) 
        {
            Debug.Log("[WaterCollision] 미션이 이미 완료되어 충돌 무시");
            return;
        }

        Debug.Log($"[WaterCollision] 충돌 감지: {other.name}, 태그: {other.tag}");
        
        if (other.CompareTag("MissionEnd"))
        {
            float elapsedTime = Time.time - startTime;
            Debug.Log($"[WaterCollision] 미션 엔드 충돌! 경과 시간: {elapsedTime}초");
            
            missionCompleted = true;
            
            // 코루틴 강제 중지
            if (failCoroutine != null)
            {
                StopCoroutine(failCoroutine);
                Debug.Log("[WaterCollision] 실패 코루틴 강제 중지");
            }
            
            Debug.Log($"[WaterCollision] missionCompleted를 true로 설정");
            
            if (elapsedTime <= 10f)
            {
                Debug.Log("[WaterCollision] 성공 조건 만족 - SUCCESS 텍스트 활성화");
                successText.SetActive(true);
                failText.SetActive(false);
                Debug.Log($"[WaterCollision] SUCCESS 활성화됨: {successText.activeInHierarchy}, FAIL 비활성화됨: {!failText.activeInHierarchy}");
            }
            else
            {
                Debug.Log("[WaterCollision] 시간 초과 - FAIL 텍스트 활성화");
                failText.SetActive(true);
                successText.SetActive(false);
            }
        }
    }

    IEnumerator FailCheckAfterTime(float limit)
    {
        Debug.Log($"[WaterCollision] 타이머 시작: {limit}초 대기");
        yield return new WaitForSeconds(limit);
        
        Debug.Log($"[WaterCollision] 타이머 종료. missionCompleted: {missionCompleted}");
        
        if (!missionCompleted)
        {
            Debug.Log("[WaterCollision] 타이머로 인한 실패 처리");
            missionCompleted = true;
            failText.SetActive(true);
            successText.SetActive(false);
        }
        else
        {
            Debug.Log("[WaterCollision] 미션이 이미 완료되어 타이머 무시됨");
        }
    }

    void Update()
    {
        // 매 프레임마다 텍스트 상태 확인 (디버깅용)
        if (Input.GetKeyDown(KeyCode.P)) // 스페이스바 대신 P키 사용
        {
            Debug.Log($"[WaterCollision] 현재 상태 - SUCCESS: {successText.activeInHierarchy}, FAIL: {failText.activeInHierarchy}, missionCompleted: {missionCompleted}");
        }
    }
}