// 공이 정상적으로 들어갔을 때만 골 판정
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger 충돌 감지됨: " + other.name);

        if (other.CompareTag("Basketball"))
        {
            Debug.Log("Basketball 태그 감지됨");

            if (other.attachedRigidbody != null && other.attachedRigidbody.velocity.y < 0)
            {
                Debug.Log("공이 아래로 이동 중! 점수 +1");
                ScoreManager.Instance.AddScore(1);
            }
        }
    }
}