using UnityEngine;

public class ShowBasketButtonsOnCameraEnter : MonoBehaviour
{
    public GameObject buttonCanvas;

    void Start()
    {
        buttonCanvas.SetActive(false); // 시작할 때 꺼두기
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log("Entered Tile2(7): " + other.name);
            buttonCanvas.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log("Exited Tile2(7): " + other.name);
            buttonCanvas.SetActive(false);
        }
    }
}
