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
    public int dropletsPerShot = 1; // 항상 같은 개수만 생성

    private int currentClick = 0;
    private bool isFiring = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentClick < maxClicks && !isFiring)
        {
            StartCoroutine(FireWaterJet(currentClick));
            currentClick++;
        }
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
                Vector3 launchDir = (firePoint.forward + firePoint.up).normalized;
                rb.AddForce(launchDir * currentForce, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        isFiring = false;
    }
}
