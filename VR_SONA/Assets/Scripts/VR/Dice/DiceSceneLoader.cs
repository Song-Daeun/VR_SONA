using UnityEngine;

public class DiceSceneLoader : MonoBehaviour
{
    public Transform cameraTransform; // XR Origin 안의 Main Camera
    public float distanceInFront = 2f; // 몇 미터 앞에 놓을지
    public float heightOffset = 0.0f; // 필요하면 높이 조절
    public bool faceCamera = true;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0; // 수평만 고려 (고개 숙여도 UI 안 따라가게)
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
