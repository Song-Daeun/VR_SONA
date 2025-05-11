using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance;
    
    void Awake()
    {
        // 씬 전환 시 이미 존재하는 XR Origin이 있다면 새로운 것은 제거
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        // 이 XR Origin을 유일한 인스턴스로 설정
        instance = this;
        
        // 씬이 바뀌어도 이 오브젝트를 유지
        DontDestroyOnLoad(this.gameObject);
    }
}
