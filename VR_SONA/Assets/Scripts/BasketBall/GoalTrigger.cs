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
        }
        else if (triggerType == TriggerType.Bottom)
        {
            if (passedTop)
            {
                ScoreManager.Instance.AddScore(1);       
                BasGameManager.Instance.AddGoal();    
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
