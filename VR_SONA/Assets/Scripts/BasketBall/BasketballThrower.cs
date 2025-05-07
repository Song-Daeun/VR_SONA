using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballThrower : MonoBehaviour
{
    public GameObject basketballPrefab;   // 프리팹 연결용
    public Transform throwOrigin;         // 던지는 위치
    public float throwForce = 10f;
    public float spinForce = 3f;

    public float throwCooldown = 1f;      // 던질 수 있는 간격 (초)
    private float lastThrowTime = -Mathf.Infinity;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && Time.time - lastThrowTime > throwCooldown)
        {
            ThrowNewBall();
            lastThrowTime = Time.time;
        }
    }

    void ThrowNewBall()    {
        GameObject newBall = Instantiate(basketballPrefab, throwOrigin.position, Quaternion.identity);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        Vector3 direction = throwOrigin.forward + Vector3.up * 0.2f;
        Debug.Log("던지는 방향: " + direction);

        rb.AddForce(direction.normalized * throwForce, ForceMode.Impulse);
        rb.AddTorque(Vector3.right * spinForce, ForceMode.Impulse);
    }
}
