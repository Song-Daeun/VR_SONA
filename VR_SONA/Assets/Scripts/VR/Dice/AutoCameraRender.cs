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
        Camera[] allCameras = FindObjectsOfType<Camera>();
        
        if (allCameras.Length > 0)
        {
            foreach (Camera camera in allCameras)
            {
                if (camera.name.Contains("Main") || camera.name.Contains("Eye") || 
                    camera.name.Contains("Head") || camera.name.Contains("Center"))
                {
                    return camera;
                }

                if (camera.CompareTag("MainCamera"))
                {
                    return camera;
                }
            }
            return allCameras[0];
        }
        return null;
    }
    
    private void AdjustCanvasPosition(Transform cameraTransform)
    {
        if (cameraTransform == null)
        {
            return;
        }
    }
}