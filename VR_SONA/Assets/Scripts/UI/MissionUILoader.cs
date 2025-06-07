// MissionUILoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionUILoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject missionPanel;
    public Button loadButton;
    public Button unloadButton;

    [Header("Settings")]
    public float distanceFromCamera = 15.0f;
    public float heightOffset = 1.5f;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main?.transform;

        if (missionPanel != null)
            missionPanel.SetActive(false);

        if (loadButton != null)
            loadButton.onClick.AddListener(OnConfirm);

        if (unloadButton != null)
            unloadButton.onClick.AddListener(OnCancel);
    }

    public void ShowMissionPanel()
    {
        if (cameraTransform == null || missionPanel == null)
            return;

        Vector3 position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        position.y += heightOffset;

        missionPanel.transform.position = position;
        missionPanel.transform.rotation = Quaternion.LookRotation(position - cameraTransform.position);
        missionPanel.SetActive(true);
    }

    private void OnConfirm()
    {
        SceneManager.LoadScene("MissionWaterRushScene", LoadSceneMode.Additive);
        if (missionPanel != null)
            missionPanel.SetActive(false);

        // DiceManager.Instance?.SetDiceButtonVisible(true);
    }

    private void OnCancel()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);

        DiceManager.Instance?.SetDiceButtonVisible(true);
    }
}
