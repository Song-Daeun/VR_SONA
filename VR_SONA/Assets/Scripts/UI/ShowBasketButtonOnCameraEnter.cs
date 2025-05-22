using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ShowBasketButtonsOnCameraEnter : MonoBehaviour
{
    public GameObject buttonCanvas;
    public GameObject loadButton;
    public GameObject unloadButton;
    public GameObject missionMessageText;

    void Start()
    {
        buttonCanvas.SetActive(false);
        if (loadButton != null) loadButton.SetActive(false);
        if (unloadButton != null) unloadButton.SetActive(false);

        if (missionMessageText != null) missionMessageText.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log("Entered Tile: " + other.name);
            buttonCanvas.SetActive(true);

            // 버튼들도 다시 켜줌
            if (loadButton != null) loadButton.SetActive(true);
            if (unloadButton != null) unloadButton.SetActive(true);

            if (missionMessageText != null) missionMessageText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log("Exited Tile: " + other.name);
            buttonCanvas.SetActive(false);

            if (loadButton != null) loadButton.SetActive(false);
            if (unloadButton != null) unloadButton.SetActive(false);
            
            if (missionMessageText != null) missionMessageText.SetActive(false);
        }
    }
}
