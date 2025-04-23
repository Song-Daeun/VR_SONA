using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Debug.Log("▶ Rigidbody 초기 속도: " + rb.velocity);
        Debug.Log("▶ Gravity 설정: " + Physics.gravity);
    }

    void FixedUpdate()
    {
        Debug.Log("현재 속도: " + rb.velocity + ", 위치: " + transform.position);
        Debug.Log("실시간 Gravity: " + Physics.gravity);
    }
}
