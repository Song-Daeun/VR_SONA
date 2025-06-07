using UnityEngine;

public class DiceSceneLoader : MonoBehaviour
{
    public Transform cameraTransform; 
    public float distanceInFront = 20f;
    public float heightOffset = 0.0f; 
    public bool faceCamera = true;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0; // 수평만 고려 
        forward.Normalize();

        Vector3 targetPosition = cameraTransform.position + forward * distanceInFront;
        targetPosition.y += heightOffset;

        transform.position = targetPosition;

        if (faceCamera)
        {
            Vector3 lookDir = transform.position - cameraTransform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}
