using UnityEngine;
using UnityEngine.UI;

public class SpellBookTile : MonoBehaviour
{
    public GameObject buttonCanvas;
    public GameObject TimeButton;
    public GameObject MoveButton;
    public GameObject MessageText;

    public float additionalTime = 30f;
    private bool isTimeAdded = false;

    void Start()
    {
        buttonCanvas.SetActive(false);
        if (TimeButton != null) TimeButton.SetActive(false);
        if (MoveButton != null) MoveButton.SetActive(false);
        if (MessageText != null) MessageText.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log("Entered Tile: " + other.name);
            buttonCanvas.SetActive(true);

            if (TimeButton != null) TimeButton.SetActive(true);
            if (MoveButton != null) MoveButton.SetActive(true);
            if (MessageText != null) MessageText.SetActive(true);
        }
    }

    public void OnClickAddTime()
    {   
        if (isTimeAdded) return;

        slider1 timer = FindObjectOfType<slider1>();
        if (timer != null)
        {
            timer.AddTime(additionalTime);
            isTimeAdded = true;
            Debug.Log("Add 30sec from TimeButton");
        }
    }
}
