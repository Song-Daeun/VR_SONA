using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
    public Transform cameraTransform;
    public float distanceFromCamera = 2.0f;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 카메라 앞 위치 계산
        transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;

        // 항상 카메라를 바라보게
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}