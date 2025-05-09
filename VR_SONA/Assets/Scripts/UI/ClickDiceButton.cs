using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickDiceButton : MonoBehaviour
{
    private Button diceButton;
    
    private void Start()
    {
        // 현재 게임 오브젝트에서 Button 컴포넌트 찾기
        diceButton = GetComponent<Button>();
        diceButton.onClick.AddListener(DiceButton_clicked);
    }
    public void DiceButton_clicked()
    {
        Debug.Log("버튼이 클릭되었습니다!");
    }
    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 해제
        if (diceButton != null)
        {
            diceButton.onClick.RemoveListener(DiceButton_clicked);
        }
    }
}
