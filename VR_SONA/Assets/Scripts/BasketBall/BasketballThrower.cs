using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private InputAction throwAction;

    void OnEnable()
    {
        var inputAsset = GetComponent<PlayerInput>()?.actions;
        throwAction = inputAsset?.FindAction("BasketBall"); 

        if (throwAction != null)
        {
            throwAction.Enable();
            throwAction.performed += OnThrowPressed;
        }
    }

    void OnDisable()
    {
        if (throwAction != null)
        {
            throwAction.performed -= OnThrowPressed;
            throwAction.Disable();
        }
    }

    void OnThrowPressed(InputAction.CallbackContext ctx)
    {
        if (Time.time - lastThrowTime > throwCooldown)
        {
            ThrowNewBall();
            lastThrowTime = Time.time;
        }
    }

    void ThrowNewBall()
    {
        GameObject newBall = Instantiate(basketballPrefab, throwOrigin.position, Quaternion.identity);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        Vector3 throwDirection = throwOrigin.forward.normalized;
        throwDirection.y += 0.7f;
        throwDirection = throwDirection.normalized;

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        rb.AddTorque(Vector3.right * spinForce, ForceMode.Impulse);
    }
}
