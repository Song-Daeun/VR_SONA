using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Basketball"))
        {
            Rigidbody rb = other.attachedRigidbody;

            // 아래 방향으로 충분히 떨어지고 있을 때만 득점 처리
            if (rb != null && rb.velocity.y < -0.5f)
            {
                ScoreManager.Instance.AddScore(1);
                Debug.Log("정상 득점!");
            }
        }
    }
}
