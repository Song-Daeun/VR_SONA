// WaterJetShooter.cs - V키(컨트롤러 X버튼) 사용
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WaterJetShooter : MonoBehaviour
{
    public GameObject waterDropPrefab;
    public Transform firePoint;

    public float baseForce = 5f;
    public float forceIncrement = 1f;
    public int maxClicks = 20;
    public float spawnInterval = 0.05f;
    public int dropletsPerShot = 1;

    public float decayTime = 0.05f;
    private float lastPressTime = 0f;

    private int currentClick = 0;
    private bool isFiring = false;

    private float currentClickDecreaseTimer = 0f;
    
    // Input System 액션 추가
    private InputAction waterJetAction;

    void OnEnable()
    {
        // PlayerInput 컴포넌트에서 액션 가져오기
        var inputAsset = GetComponent<PlayerInput>()?.actions;
        waterJetAction = inputAsset?.FindAction("ShootWaterJet"); // Input Actions에서 설정한 액션 이름
        
        if (waterJetAction != null)
        {
            waterJetAction.Enable();
            waterJetAction.performed += OnWaterJetPressed;
        }
    }

    void OnDisable()
    {
        if (waterJetAction != null)
        {
            waterJetAction.performed -= OnWaterJetPressed;
            waterJetAction.Disable();
        }
    }

    void OnWaterJetPressed(InputAction.CallbackContext ctx)
    {
        // 미션이 완료되었으면 아무 동작도 하지 않음
        if (WaterCollisionHandler.missionCompleted)
        {
            return;
        }

        if (currentClick < maxClicks && !isFiring)
        {
            StartCoroutine(FireWaterJet(currentClick));
            currentClick++;
            lastPressTime = Time.time;
        }
    }

    void Update()
    {
        // 미션이 완료되었으면 아무 동작도 하지 않음
        if (WaterCollisionHandler.missionCompleted)
        {
            return;
        }

        float interval = Time.time - lastPressTime;

        // 텀이 decayTime보다 길면 힘 급감시키기
        if (currentClick > 0 && interval > decayTime)
        {
            int decreaseAmount = Mathf.Max(1, currentClick / 2);
            currentClick -= decreaseAmount;
            if (currentClick < 0) currentClick = 0;

            lastPressTime = Time.time;
        }

        // XR Device Simulator용 키보드 입력 추가
        // if (Input.GetKeyDown(KeyCode.N) && currentClick < maxClicks && !isFiring)
        // {
        //     StartCoroutine(FireWaterJet(currentClick));
        //     currentClick++;
        //     lastPressTime = Time.time;
        // }
    }

    IEnumerator FireWaterJet(int clickCount)
    {
        isFiring = true;

        float currentForce = baseForce + clickCount * forceIncrement;

        for (int i = 0; i < dropletsPerShot; i++)
        {
            GameObject drop = Instantiate(waterDropPrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = drop.GetComponent<Rigidbody>();

            if (rb != null)
            {
                float fixedVerticalForce = 5f;
                Vector3 forwardDir = firePoint.forward.normalized;

                Vector3 forceVector = forwardDir * currentForce;
                forceVector.y = fixedVerticalForce;

                rb.AddForce(forceVector, ForceMode.Impulse);
            }

            Destroy(drop, 3f);

            yield return new WaitForSeconds(spawnInterval);
        }

        isFiring = false;
    }
}
