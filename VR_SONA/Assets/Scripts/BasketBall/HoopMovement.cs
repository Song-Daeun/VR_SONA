using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopMovement : MonoBehaviour
{
    public float speed = 2f;
    public float moveRange = 3f;

    private Vector3 startPosition;
    private Vector3 ringLocalOffset;
    private Rigidbody ringRb;

    void Start()
    {
        startPosition = transform.position;

        // Ring의 Rigidbody 가져오기
        Transform ringTransform = transform.Find("Ring");
        if (ringTransform != null)
        {
            ringRb = ringTransform.GetComponent<Rigidbody>();
            ringLocalOffset = ringTransform.localPosition; // 자식 위치 저장!
        }
        else
        {
            Debug.LogError("Ring 오브젝트를 찾을 수 없습니다.");
        }
    }

    void FixedUpdate()
    {
        float offset = Mathf.Sin(Time.time * speed) * moveRange;
        Vector3 newParentPos = startPosition + new Vector3(offset, 0, 0);

        // 부모 이동
        transform.position = newParentPos;

        // 자식 Rigidbody도 원래의 로컬 위치만큼 더해서 이동
        if (ringRb != null)
        {
            ringRb.MovePosition(newParentPos + transform.rotation * ringLocalOffset);
        }
    }
}
