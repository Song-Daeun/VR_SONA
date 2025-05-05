using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandVisualizer : MonoBehaviour
{
    public XRHandSubsystem handSubsystem;
    public bool isLeft = true;

    void Start()
    {
        handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();
    }

    void Update()
    {
        if (handSubsystem == null) return;

        XRHand hand = isLeft ? handSubsystem.leftHand : handSubsystem.rightHand;

        if (hand.isTracked)
        {
            // 손의 중심 관절 (예: Wrist) 기준 위치 가져오기
            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose pose))
            {
                transform.SetPositionAndRotation(pose.position, pose.rotation);
            }
        }
    }
}