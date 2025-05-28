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

        // 10초 후 실패 체크 코루틴 시작
        failCoroutine = StartCoroutine(FailCheckAfterTime(10f));

        Debug.Log($"[WaterCollision] 미션 시작! 시작 시간: {startTime}");
    }

    void OnParticleCollision(GameObject other)
    {
        // endpoint와 충돌했는지 확인
        if (other.CompareTag("MissionEnd"))
        {
            float elapsedTime = Time.time - startTime;
            Debug.Log($"[WaterCollision] Endpoint 충돌! 경과 시간: {elapsedTime}초");

            // 실패 코루틴이 아직 실행 중이라면 중지
            if (failCoroutine != null)
            {
                StopCoroutine(failCoroutine);
                failCoroutine = null;
                Debug.Log("[WaterCollision] 실패 타이머 코루틴 중지");
            }

            // 이미 완료되었더라도 실패 방지는 했으므로 성공 조건 체크
            if (!missionCompleted)
            {
                CompleteMission(elapsedTime);
            }
        }
    }

    private void CompleteMission(float elapsedTime)
    {
        missionCompleted = true;

        if (elapsedTime <= 10f)
        {
            Debug.Log($"[WaterCollision] 성공! {elapsedTime:F2}초에 완료");
            ShowSuccess();
        }
        else
        {
            Debug.Log($"[WaterCollision] 실패! {elapsedTime:F2}초 - 시간 초과");
            ShowFailure();
        }

    }

    private void ShowSuccess()
    {
        successText.SetActive(true);
        failText.SetActive(false);
        Debug.Log("[WaterCollision] SUCCESS 텍스트 표시");

        StartCoroutine(StopGameAfterDelay(1f));
    }

    private void ShowFailure()
    {
        failText.SetActive(true);
        successText.SetActive(false);
        Debug.Log("[WaterCollision] FAIL 텍스트 표시");

        StartCoroutine(StopGameAfterDelay(1f));
    }


    private IEnumerator StopGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);              
        Time.timeScale = 0f;
        Debug.Log("[WaterCollision] 게임 정지됨 (1초 지연)");
    }

    IEnumerator FailCheckAfterTime(float timeLimit)
    {
        Debug.Log($"[WaterCollision] 실패 타이머 시작: {timeLimit}초");

        yield return new WaitForSeconds(timeLimit);

        if (!missionCompleted)
        {
            Debug.Log("[WaterCollision] 10초 경과 - 실패 처리");
            missionCompleted = true;
            ShowFailure();
        }
        else
        {
            Debug.Log("[WaterCollision] 미션이 이미 완료되어 타이머 종료");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            float currentTime = Time.time - startTime;
            Debug.Log($"[WaterCollision] === 현재 상태 ===");
            Debug.Log($"경과 시간: {currentTime:F2}초");
            Debug.Log($"미션 완료: {missionCompleted}");
            Debug.Log($"SUCCESS 활성화: {successText.activeInHierarchy}");
            Debug.Log($"FAIL 활성화: {failText.activeInHierarchy}");
            Debug.Log($"실패 타이머 실행 중: {failCoroutine != null}");
        }
    }
}
