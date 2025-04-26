using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class XRCamera : MonoBehaviour
{
    public Transform playerTransform;  // 인스펙터에서 Player 오브젝트 할당
    private XROrigin xrOrigin;
    private Camera xrCamera;
    private Vector3 cameraOffset = new Vector3(0, 1.7f, 0);  // 눈 높이 조정
    
    private void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin != null)
        {
            xrCamera = xrOrigin.Camera;
        }
        
        // 플레이어 찾기 (할당되지 않은 경우)
        if (playerTransform == null)
        {
            var player = GameObject.Find("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // 캐릭터 이동 스크립트 찾아서 cameraTransform 설정
        if (playerTransform != null && xrCamera != null)
        {
            CharacterMove characterMove = playerTransform.GetComponent<CharacterMove>();
            if (characterMove != null)
            {
                characterMove.cameraTransform = xrCamera.transform;
                Debug.Log("캐릭터 이동 스크립트의 카메라 참조가 설정되었습니다.");
            }
        }
    }
    
    private void LateUpdate()
    {
        // XR이 활성화되어 있지만 디바이스가 감지되지 않은 경우에만 실행
        // 이렇게 하면 실제 VR 모드에서는 이 스크립트가 아무 일도 하지 않음
        if (UnityEngine.XR.XRSettings.enabled && !UnityEngine.XR.XRSettings.isDeviceActive)
        {
            if (xrOrigin != null && xrCamera != null && playerTransform != null)
            {
                // XR Origin의 위치를 플레이어 위치로 설정
                // 이렇게 하면 XR 시스템이 카메라를 제어해도 플레이어와 함께 움직임
                xrOrigin.transform.position = playerTransform.position;
                
                // 선택사항: 회전도 동기화 (필요에 따라 주석 처리 가능)
                xrOrigin.transform.rotation = playerTransform.rotation;
                
                // 카메라 높이 설정 (눈 높이)
                Transform cameraOffsetTransform = xrOrigin.CameraFloorOffsetObject.transform;
                xrCamera.transform.localPosition = cameraOffset;
                
                // 디버그 정보
                Debug.Log("개발 모드: 카메라를 플레이어에 동기화 중");
            }
        }
    }

}
