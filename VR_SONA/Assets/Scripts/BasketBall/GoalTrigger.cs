// 공이 정상적으로 들어갔을 때만 골 판정
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Basketball"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            // 속도가 아래 방향인지 확인 (위→아래로 통과한 경우만 인정)
            if (rb.velocity.y < 0f)
            {
                ScoreManager.instance.AddScore(1);
                Debug.Log("골인!");
            }
        }
    }
}