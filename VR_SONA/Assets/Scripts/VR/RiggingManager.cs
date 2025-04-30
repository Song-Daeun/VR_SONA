using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiggingManager : MonoBehaviour
{
    public Transform leftHandIk;
    public Transform rightHandIk;

    public Transform leftHandController;
    public Transform rightHandController;

    public Vector3[] leftOffset;    /// 0: Position, 1: Rotation
    public Vector3[] rightOffset;

    void LateUpdate()
    {
        MappingHandTransform(leftHandIk, leftHandController, true);
        MappingHandTransform(rightHandIk, rightHandController, false);
    }

    private void MappingHandTransform(Transform ik, Transform controller, bool isLeft)
    {
        var offset = isLeft ? leftOffset : rightOffset;

        ik.position = controller.TransformPoint(offset[0]);
        ik.rotation = controller.rotation * Quaternion.Euler(offset[1]);
    }
}
