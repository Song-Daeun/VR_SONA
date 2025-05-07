using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballThrower : MonoBehaviour
{
    public GameObject basketballPrefab;   // 프리팹 연결용
    public Transform throwOrigin;         // 던지는 위치
    public float throwForce = 14f;
    public float spinForce = 3f;

    public float throwCooldown = 1f;      // 던질 수 있는 간격 (초)
    private float lastThrowTime = -Mathf.Infinity;

    [Header("포물선 조정")]
    [Range(0f, 2f)] public float forwardMultiplier = 1.0f;
    [Range(0f, 2f)] public float upwardMultiplier = 0.9f;
    public float throwAngle = 35f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && Time.time - lastThrowTime > throwCooldown)
        {
            ThrowNewBall();
            lastThrowTime = Time.time;
        }
    }

    void ThrowNewBall()
    {
        GameObject newBall = Instantiate(basketballPrefab, throwOrigin.position, Quaternion.identity);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        float angleInRadians = throwAngle * Mathf.Deg2Rad;

        Vector3 forwardXZ = new Vector3(throwOrigin.forward.x, 0, throwOrigin.forward.z).normalized;

        Vector3 throwDirection =
            forwardXZ * Mathf.Cos(angleInRadians) * forwardMultiplier +
            Vector3.up * Mathf.Sin(angleInRadians) * upwardMultiplier;

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        rb.AddTorque(Vector3.right * spinForce, ForceMode.Impulse);
    }
}
