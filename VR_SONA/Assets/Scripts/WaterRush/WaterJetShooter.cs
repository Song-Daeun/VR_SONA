using UnityEngine;
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

    public float decayTime = 0.05f; // 텀 기준 시간 (0.05초 이상 쉬면 힘 급감)
    private float lastPressTime = 0f;

    private int currentClick = 0;
    private bool isFiring = false;

    private float currentClickDecreaseTimer = 0f;

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

        if (Input.GetKeyDown(KeyCode.Space) && currentClick < maxClicks && !isFiring)
        {
            StartCoroutine(FireWaterJet(currentClick));
            currentClick++;
            lastPressTime = Time.time;
        }
    }

    IEnumerator FireWaterJet(int clickCount)
    {
        isFiring = true;

        float currentForce = baseForce + clickCount * forceIncrement;

        for (int i = 0; i < dropletsPerShot; i++)
        {
            GameObject drop = Instantiate(waterDropPrefab, firePoint.position, Quaternion.identity);
            // Destroy(drop, 3f);
            Rigidbody rb = drop.GetComponent<Rigidbody>();

            if (rb != null)
            {
                float fixedVerticalForce = 5f;
                Vector3 forwardDir = firePoint.forward.normalized;

                Vector3 forceVector = forwardDir * currentForce;
                forceVector.y = fixedVerticalForce;

                rb.AddForce(forceVector, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        isFiring = false;
    }
}
