using UnityEngine;

public class HoopMovement : MonoBehaviour
{
    public float speed = 2f;
    public float moveRange = 3f;

    private Vector3 localStartPosition;
    private Vector3 ringLocalOffset;
    private Rigidbody ringRb;

    void Start()
    {
        localStartPosition = transform.localPosition;

        Transform ringTransform = transform.Find("Ring");
        if (ringTransform != null)
        {
            ringRb = ringTransform.GetComponent<Rigidbody>();
            ringLocalOffset = ringTransform.localPosition;
        }
        else
        {
            Debug.LogError("Ring 오브젝트를 찾을 수 없습니다.");
        }
    }

    void FixedUpdate()
    {
        float offset = Mathf.Sin(Time.time * speed) * moveRange;
        Vector3 newLocalPos = localStartPosition + new Vector3(offset, 0, 0);

        transform.localPosition = newLocalPos;

        if (ringRb != null)
        {
            Vector3 worldPos = transform.parent.TransformPoint(newLocalPos + ringLocalOffset);
            ringRb.MovePosition(worldPos);
        }
    }
}
