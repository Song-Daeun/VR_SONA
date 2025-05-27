using UnityEngine;

public class WaterCollisionHandler : MonoBehaviour
{
    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("MissionEnd"))
        {
            Debug.Log("물줄기가 EndPoint에 닿았습니다! 미션 클리어!");
            // 여기에 미션 클리어 로직 추가
        }
    }
}
