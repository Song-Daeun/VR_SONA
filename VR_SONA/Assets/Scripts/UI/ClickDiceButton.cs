using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClickDiceButton : MonoBehaviour
{
    public Transform playerTransform;
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
        StartCoroutine(LoadDiceSceneAtPlayer());
    }

    private IEnumerator LoadDiceSceneAtPlayer()
    {
        // Additive 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        // DiceScene 찾기
        Scene diceScene = SceneManager.GetSceneByName("DiceScene");

        // Plane 이동
        GameObject[] rootObjects = diceScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == "Plane") // Plane 이름이 정확히 일치해야 함
            {
                obj.transform.position = playerTransform.position;
                break;
            }
        }
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
