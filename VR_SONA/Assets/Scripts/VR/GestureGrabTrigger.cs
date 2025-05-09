using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

public class GestureGrabTrigger : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public bool isLeft = true;
    private XRHandSubsystem handSubsystem;
    private bool grabbing = false;

    void Start()
    {
        handSubsystem = XRGeneralSettings.Instance?
            .Manager?.activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();
    }

    public void CheckForGrab()
    {
        if (handSubsystem == null || rayInteractor == null)
            return;

        XRHand hand = isLeft ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        if (
            hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbPose) &&
            hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexPose)
        )
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);

            if (distance < 0.02f && !grabbing)
            {
                grabbing = true;

                // Try to get target interactable
                if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    if (hit.collider.TryGetComponent<XRBaseInteractable>(out var interactable))
                    {
                        rayInteractor.StartManualInteraction(interactable);
                    }
                }
            }
            else if (distance >= 0.02f && grabbing)
            {
                grabbing = false;
                rayInteractor.EndManualInteraction();
            }
        }
    }
}
