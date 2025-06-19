using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private InputActionAsset inputActions;
    private InputAction waterJetAction;

    void OnEnable()
    {
        waterJetAction = new InputAction("WaterJet", InputActionType.Button);
        waterJetAction.AddBinding("<XRController>{RightHand}/primaryButton");
        waterJetAction.AddBinding("<Keyboard>/space");

        
        waterJetAction.performed += OnWaterJetPressed;
        waterJetAction.Enable();
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
        if (WaterCollisionHandler.missionCompleted)
            return;

        float interval = Time.time - lastPressTime;

        if (currentClick > 0 && interval > decayTime)
        {
            int decreaseAmount = Mathf.Max(1, currentClick / 2);
            currentClick -= decreaseAmount;
            currentClick = Mathf.Max(currentClick, 0);
            lastPressTime = Time.time;
        }
    }

    IEnumerator FireWaterJet(int clickCount)
    {
        isFiring = true;
        float currentForce = (baseForce + clickCount * forceIncrement) * 1.7f;
        
        for (int i = 0; i < dropletsPerShot; i++)
        {
            GameObject drop = Instantiate(waterDropPrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = drop.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 forwardDir = firePoint.forward.normalized;

                float verticalForce = 3.0f; 
                float horizontalForce = currentForce * 2.6f; 
                
                Vector3 forceVector = forwardDir * horizontalForce;
                forceVector.y = verticalForce;

                rb.AddForce(forceVector, ForceMode.Impulse);
            }

            Destroy(drop, 10f);
            yield return new WaitForSeconds(spawnInterval);
        }
        isFiring = false;
    }
}
