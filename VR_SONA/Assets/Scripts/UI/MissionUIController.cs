using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;

public class MissionUIController : MonoBehaviour
{
    public GameObject missionPanel;
    public Button loadButton;
    public Button unloadButton;

    public float distanceFromCamera = 15.0f;
    public float heightOffset = 1.5f;

    private Transform cameraTransform;
    private MissionSceneLoader missionSceneLoader;

    private void Start()
    {
        cameraTransform = Camera.main?.transform;

        if (missionPanel != null)
            missionPanel.SetActive(false);

        missionSceneLoader = FindObjectOfType<MissionSceneLoader>();

        if (loadButton != null)
            loadButton.onClick.AddListener(OnConfirm);
        if (unloadButton != null)
            unloadButton.onClick.AddListener(OnCancel);
    }

    public void ShowMissionPanel()
    {
        if (cameraTransform == null || missionPanel == null) return;

        Vector3 position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        position.y += heightOffset;

        missionPanel.transform.position = position;
        missionPanel.transform.rotation = Quaternion.LookRotation(position - cameraTransform.position);
        missionPanel.SetActive(true);
    }

    private void OnConfirm()
    {
        Debug.Log("🟡 '예' 버튼 클릭됨");

        if (missionSceneLoader != null)
        {
            Debug.Log("🟢 missionSceneLoader 찾음 → LoadMissionScene 호출");
            missionSceneLoader.LoadMissionScene();
        }
        else
        {
            Debug.LogError("🔴 missionSceneLoader 가 null임 → 씬 로드 실패");
        }

        missionPanel.SetActive(false);
    }

    private void OnCancel()
    {
        missionPanel.SetActive(false);
        DiceManager.Instance?.SetDiceButtonVisible(true);
    }
}