using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceResultDetector : MonoBehaviour
{
    [System.Serializable]
    public class DiceNumber
    {
        public int number;   
        public Vector3 localPosition;    
        public Vector3 localNormal;      
        public Vector3 numberUpDirection;
        public Transform faceObject;    
    }
    
    [Header("Dice Numbers Settings")]
    public DiceNumber[] diceNumbers = new DiceNumber[8];

    [Header("Physics Settings")]
    public Rigidbody diceRigidbody;               
    public float stopThreshold = 0.1f;         
    public float stableTime = 1.0f;

    [Header("Camera Reference")]
    public Camera playerCamera;

    [Header("Debugging Settings")]
    public bool showDebugLogs = true;    
    public bool drawDebugGizmos = true;

    private bool resultConfirmed = false;      
    private int lastResult = -1;               
    
    private void Start()
    {
        AutoConnectFaceObjects();
        SetupDiceFacesFromObjects();

        if (playerCamera == null)
            playerCamera = Camera.main;

        StartCoroutine(WatchDiceUntilStop());
    }

    private IEnumerator WatchDiceUntilStop()
    {
        if (diceRigidbody == null)
            yield break;

        float timer = 0f;

        while (true)
        {
            if (diceRigidbody.velocity.magnitude < stopThreshold &&
                diceRigidbody.angularVelocity.magnitude < stopThreshold)
            {
                timer += Time.deltaTime;

                if (timer >= stableTime)
                    break;
            }
            else
            {
                timer = 0f;
            }

            yield return null;
        }

        if (resultConfirmed) yield break;

        int result = GetVisibleNumber();

        if (result == lastResult)
            yield break;

        resultConfirmed = true;
        lastResult = result;

        DiceSceneManager sceneManager = FindObjectOfType<DiceSceneManager>();
        if (sceneManager != null)
        {
            sceneManager.OnDiceResultDetected(result);
        }
    }

    [ContextMenu("Auto Connect Face Objects")]
    private void AutoConnectFaceObjects()
    {    
        if (diceNumbers == null || diceNumbers.Length != 8)
        {
            diceNumbers = new DiceNumber[8];
            for (int i = 0; i < 8; i++)
                diceNumbers[i] = new DiceNumber();
        }

        for (int i = 1; i <= 8; i++)
        {
            string planeName = $"Plane_{i}";
            Transform planeTransform = transform.Find(planeName);
            if (planeTransform != null)
            {
                int index = i - 1;
                diceNumbers[index].number = i;
                diceNumbers[index].faceObject = planeTransform;
            }
        }
    }

    [ContextMenu("Setup Dice Faces From Objects")]
    private void SetupDiceFacesFromObjects()
    { 
        for (int i = 0; i < diceNumbers.Length; i++)
        {
            if (diceNumbers[i].faceObject != null)
            {
                diceNumbers[i].localPosition = transform.InverseTransformPoint(diceNumbers[i].faceObject.position);
                Vector3 worldNormal = diceNumbers[i].faceObject.TransformDirection(Vector3.back);
                diceNumbers[i].localNormal = transform.InverseTransformDirection(worldNormal);
                diceNumbers[i].numberUpDirection = Vector3.up;
            }
        }
    }

    public int GetVisibleNumber()
    {
        if (diceNumbers == null || diceNumbers.Length == 0)
            return -1;

        return GetBottomFacingNumber();
    }

    private int GetBottomFacingNumber()
    {
        float lowestY = float.MaxValue;
        int bottomNumber = 1;
        bool hasValidFace = false;

        for (int i = 0; i < diceNumbers.Length; i++)
        {
            DiceNumber face = diceNumbers[i];
            if (face.faceObject != null)
            {
                hasValidFace = true;
                Vector3 worldPosition = face.faceObject.position;

                if (worldPosition.y < lowestY)
                {
                    lowestY = worldPosition.y;
                    bottomNumber = face.number;
                }
            }
        }

        if (!hasValidFace)
            return 1;

        return bottomNumber;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || diceNumbers == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);

        float lowestY = float.MaxValue;
        int lowestFaceIndex = -1;

        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            if (face.faceObject != null)
            {
                Vector3 pos = face.faceObject.position;
                if (pos.y < lowestY)
                {
                    lowestY = pos.y;
                    lowestFaceIndex = i;
                }
            }
        }

        for (int i = 0; i < diceNumbers.Length; i++)
        {
            var face = diceNumbers[i];
            if (face.faceObject != null)
            {
                Vector3 pos = face.faceObject.position;

                Gizmos.color = (i == lowestFaceIndex) ? Color.red : Color.green;
                Gizmos.DrawSphere(pos, 0.03f);

                Gizmos.color = Color.blue;
                Vector3 normal = face.faceObject.TransformDirection(Vector3.back);
                Gizmos.DrawRay(pos, normal * 0.1f);

#if UNITY_EDITOR
                UnityEditor.Handles.color = (i == lowestFaceIndex) ? Color.red : Color.green;
                UnityEditor.Handles.Label(pos + normal * 0.05f, face.number.ToString());
#endif
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
    }
}
