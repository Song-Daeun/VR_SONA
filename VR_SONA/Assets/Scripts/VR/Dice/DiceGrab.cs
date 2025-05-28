using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceGrab : MonoBehaviour
{
    public DiceSceneManager diceSceneManager;

    private void OnEnable()
    {
        var interactable = GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnDisable()
    {
        var interactable = GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnGrabbed);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (diceSceneManager != null)
        {
            Debug.Log("🔥 Grab됨!");
            diceSceneManager.ActivateDiceDetection(); // 이제부터 감지 시작
        }
    }
}
