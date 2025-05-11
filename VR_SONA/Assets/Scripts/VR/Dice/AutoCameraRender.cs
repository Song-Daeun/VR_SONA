using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AutoCameraRender : MonoBehaviour
{
    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        
        if (canvas != null)
        {
            Camera mainCamera = FindAppropriateCamera();
            
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
                // Debug.Log($"자동으로 Event Camera 할당됨: {mainCamera.name}");
                AdjustCanvasPosition(mainCamera.transform);
            }
            else
            {
                Debug.LogWarning("Event Camera를 찾을 수 없습니다. 수동으로 설정해주세요.");
            }
        }
        else
        {
            Debug.LogError("Canvas 컴포넌트를 찾을 수 없습니다. 이 스크립트는 Canvas 오브젝트에 부착되어야 합니다.");
        }
    }
    
    private Camera FindAppropriateCamera()
    {        
        // Scene에 있는 모든 카메라를 순회하며 적절한 것 찾기
        Camera[] allCameras = FindObjectsOfType<Camera>();
        
        if (allCameras.Length > 0)
        {
            // 이름이나 위치를 기반으로 VR 카메라를 식별
            foreach (Camera camera in allCameras)
            {
                // 일반적인 VR 카메라 이름 패턴
                if (camera.name.Contains("Main") || camera.name.Contains("Eye") || 
                    camera.name.Contains("Head") || camera.name.Contains("Center"))
                {
                    return camera;
                }
                
                // MainCamera 태그를 가진 카메라 우선 선택
                if (camera.CompareTag("MainCamera"))
                {
                    // Debug.Log($"MainCamera 태그를 가진 카메라를 찾았습니다: {camera.name}");
                    return camera;
                }
            }
            return allCameras[0];
        }
        
        // 정말로 카메라를 찾을 수 없는 경우
        Debug.LogError("씬에 어떤 카메라도 찾을 수 없습니다.");
        return null;
    }
    
    private void AdjustCanvasPosition(Transform cameraTransform)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("Camera Transform이 null입니다. Canvas 위치 조정을 건너뜁니다.");
            return;
        }
        
        // Canvas를 카메라 앞 적절한 위치에 배치
        Vector3 cameraForward = cameraTransform.forward;
        float distance = 2.0f; // 카메라로부터의 거리
    }
}