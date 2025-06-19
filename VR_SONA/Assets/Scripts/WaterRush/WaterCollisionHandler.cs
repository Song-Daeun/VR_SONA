using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WaterCollisionHandler : MonoBehaviour
{
    public GameObject successText;
    public GameObject failText;
    public GameObject returnButton; 

    private float startTime;
    public static bool missionCompleted = false;
    private Coroutine failCoroutine;

    void Start()
    {
        startTime = Time.time;
        missionCompleted = false; 
        successText.SetActive(false);
        failText.SetActive(false);
        returnButton.SetActive(false);

        failCoroutine = StartCoroutine(FailCheckAfterTime(10f));
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("MissionEnd"))
        {
            float elapsedTime = Time.time - startTime;

            if (failCoroutine != null)
            {
                StopCoroutine(failCoroutine);
                failCoroutine = null;
            }

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
            BasGameManager.MissionResult = true; 
            ShowSuccess();
        }
        else
        {
            BasGameManager.MissionResult = false; 
            ShowFailure();
        }
    }

    private void ShowSuccess()
    {
        Debug.Log("showsuccesss called");

        successText.SetActive(true);
        failText.SetActive(false);
        returnButton.SetActive(true);
    }

    private void ShowFailure()
    {
        Debug.Log("showfailure called");

        failText.SetActive(true);
        successText.SetActive(false);
        returnButton.SetActive(true);
    }

    IEnumerator FailCheckAfterTime(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);

        if (!missionCompleted)
        {
            missionCompleted = true;
            BasGameManager.MissionResult = false; // 실패 결과 저장
            ShowFailure();
        }
        else
        {
            Debug.Log("[WaterCollision] 미션이 이미 완료되어 타이머 종료");
        }
    }
}