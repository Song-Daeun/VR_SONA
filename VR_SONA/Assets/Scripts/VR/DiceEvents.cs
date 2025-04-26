using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class DebugDiceEvents : MonoBehaviour
{
    XRGrabInteractable _ix;

    void Awake()
    {
        _ix = GetComponent<XRGrabInteractable>();
        _ix.onHoverEntered.AddListener(inter => Debug.Log("Hover 시작: " + inter.transform.name));
        _ix.onHoverExited .AddListener(inter => Debug.Log("Hover 끝: "  + inter.transform.name));
        _ix.onSelectEntered.AddListener(inter => Debug.Log("Grab 시작: " + inter.transform.name));
        _ix.onSelectExited .AddListener(inter => Debug.Log("Grab 끝: "  + inter.transform.name));
    }
}
