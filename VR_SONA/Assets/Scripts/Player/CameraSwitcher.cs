using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera firstPersonCam;
    public CinemachineVirtualCamera thirdPersonCam;

    private bool isFirstPerson = true;

    void Start()
    {
        // 시작할 때 원하는 카메라에 우선순위 부여
        firstPersonCam.Priority = 10;
        thirdPersonCam.Priority = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // C 키로 전환
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                firstPersonCam.Priority = 10;
                thirdPersonCam.Priority = 0;
            }
            else
            {
                firstPersonCam.Priority = 0;
                thirdPersonCam.Priority = 10;
            }
        }
    }
}