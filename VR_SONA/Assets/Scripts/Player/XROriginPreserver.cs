using UnityEngine;

public class XROriginPreserver : MonoBehaviour
{
    private static XROriginPreserver instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환에도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);  // 중복 방지
        }
    }
}
