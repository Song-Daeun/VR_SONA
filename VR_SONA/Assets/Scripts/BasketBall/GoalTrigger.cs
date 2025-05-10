using UnityEngine;

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
            Debug.Log("Top 통과!");
        }
        else if (triggerType == TriggerType.Bottom)
        {
            if (passedTop)
            {
                ScoreManager.Instance.AddScore(1);
                Debug.Log("득점!");
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
