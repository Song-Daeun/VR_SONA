using UnityEngine;
using UnityEngine.XR;

public class HandVisualizer: MonoBehaviour
{
    [Tooltip("LeftHand 또는 RightHand 중 하나")]
    public XRNode handNode = XRNode.LeftHand;

    [Tooltip("XR Origin Hands 루트 (트래킹 스페이스)")]
    public Transform xrOrigin;

    void LateUpdate()
    {
        // 로컬 트래킹 스페이스 좌표
        Vector3 localPos = InputTracking.GetLocalPosition(handNode);
        Quaternion localRot = InputTracking.GetLocalRotation(handNode);

        // 월드 좌표로 변환
        Vector3 worldPos = xrOrigin.TransformPoint(localPos);
        Quaternion worldRot = xrOrigin.rotation * localRot;

        // 이 스크립트가 붙은 GameObject(transform)에 적용
        transform.SetPositionAndRotation(worldPos, worldRot);
    }
}
