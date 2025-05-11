using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public enum TriggerType { Top, Bottom }
    public TriggerType triggerType;

    private static bool passedTop = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Basketball"))
            return;

        if (triggerType == TriggerType.Top)
        {
            passedTop = true;
            Debug.Log("Top 통과");
        }
        else if (triggerType == TriggerType.Bottom)
        {
            if (passedTop)
            {
                ScoreManager.Instance.AddScore(1);       // 점수 UI 반영
                GameManager.Instance.AddGoal();          // 성공 조건 반영
                Debug.Log("득점 AddGoal 호출됨");
                passedTop = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Basketball"))
            return;

        if (triggerType == TriggerType.Top)
        {
            passedTop = false;
        }
    }
}
