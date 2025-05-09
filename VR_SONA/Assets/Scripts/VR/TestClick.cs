using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TestClick : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            Debug.Log("✅ 버튼 클릭됨!");
        });
    }
    void Update() {
    if (EventSystem.current.IsPointerOverGameObject()) {
        Debug.Log("🎯 UI에 커서 올라감");
    }
    }
}
