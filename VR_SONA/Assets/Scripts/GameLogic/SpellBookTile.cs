using UnityEngine;

public class SpellBookTile : MonoBehaviour
{
    public float additionalTime = 30f;  // 증가시킬 시간 (초)
    private bool isUsed = false;        // 한 번만 작동하도록 방지

    void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;

        if (other.CompareTag("MainCamera"))
        {
            slider1 timer = FindObjectOfType<slider1>();
            if (timer != null)
            {
                timer.AddTime(additionalTime);
                isUsed = true; // 중복 작동 방지
                Debug.Log("Add 30sec: ");
            }
        }
    }
}
