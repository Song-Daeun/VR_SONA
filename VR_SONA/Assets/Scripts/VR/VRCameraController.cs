using UnityEngine;
using UnityEngine.XR;

public class VRCameraController : MonoBehaviour
{
    public Camera vrCamera;
    public float smoothSpeed = 0.125f; // 부드러운 움직임을 위한 값
    public float eyeHeight = 1.7f;
    public Vector3 eyeOffset = new Vector3(0, 0, 0);

    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }
    }
    

    void LateUpdate()
    {
        if (vrCamera == null) 
            return; // 카메라가 없으면 아무것도 하지 않음
        
        // 카메라를 플레이어의 눈 위치에 배치
        vrCamera.transform.position = transform.position + new Vector3(0, eyeHeight, 0) + eyeOffset;
        
        // 카메라가 플레이어와 동일한 방향을 바라보게 함
        vrCamera.transform.rotation = transform.rotation;
    }
}