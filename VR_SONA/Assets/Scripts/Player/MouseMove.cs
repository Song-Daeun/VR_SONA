using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    public float sesitivity = 500f;
    // 마우스 민감도
    public float rotationX;
    // X축의 위치
    public float rotationY;
    // Y축의 위치
    void Start()
    {
        
    }

    void Update()
    {
        float mouseMoveX = Input.GetAxis("Mouse X");
        // 마우스 X축 움직임값을 받아서 mouseMoveX 변수에 저장
        
        float mouseMoveY = Input.GetAxis("Mouse Y");
        // 마우스 Y축 움직임값을 받아서 mouseMoveY 변수에 저장

        rotationY += mouseMoveX * sesitivity * Time.deltaTime;
        // rotationY 변수의 값은 rotationY + (mouseMoveX * 마우스 민감도 * Time.deltaTime)
        
        rotationX += mouseMoveY * sesitivity * Time.deltaTime;
        // rotationX 변수의 값은 rotationX + (mouseMoveY * 마우스 민감도 * Time.deltaTime)
        

        /*
         * 아래의 if문은 마우스로 이동하는 시야가 사람의 시야처럼 보이기 하기 위함 
         */
        if (rotationX > 35f)
        {
            rotationX = 35f;
            // 위로 35도 이상 넘어가지 못하게 (고개가 과하게 젖혀지지 않게)
        }

        if(rotationX < -30f)
        {
            rotationX = -30f;
            // 아래로 30도 이상 넘어가지 못하게 (고개가 과하게 꼬꾸라지지 않게)
        }

        transform.eulerAngles = new Vector3(-rotationX, rotationY, 0);
        // rotationX값, rotationY값, Z는 0값을 Vector3 형식으로 변환하고,
        // transform 오일러각에 저장함.
    }
}