using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using System.Collections;

public class MissionObject : MonoBehaviour
{
    private XROrigin xrOrigin;
    public float distanceFromPlayer = 2.0f;

    void Start()
    {
        StartCoroutine(MoveToPlayerAfterFrame());
    }

    IEnumerator MoveToPlayerAfterFrame()
    {
        yield return null; // 한 프레임 대기

        xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin을 찾을 수 없습니다!");
        }
        else
        {
            Vector3 playerPos = xrOrigin.Camera.transform.position;
            Quaternion playerRot = xrOrigin.Camera.transform.rotation;
            Vector3 offset = playerRot * Vector3.forward * distanceFromPlayer;
            Vector3 desiredPos = playerPos + offset;
            transform.position = desiredPos;
            transform.rotation = Quaternion.Euler(0, playerRot.eulerAngles.y, 0);
        }
    }
}