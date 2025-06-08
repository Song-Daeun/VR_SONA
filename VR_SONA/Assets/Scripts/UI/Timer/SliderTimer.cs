using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class slider1 : MonoBehaviour
{
    private Slider sd;
    private float TimeRemain = 480f;

    void Start()
    {
        sd = GetComponent<Slider>();
        sd.maxValue = TimeRemain;
        sd.value = TimeRemain;
    }

    void Update()
    {
        TimeRemain -= Time.deltaTime;
        sd.value = TimeRemain;

        if (TimeRemain <= 0f)
        {
            EndRound();
        }
    }

    void EndRound()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void AddTime(float sec)
    {
        TimeRemain += sec;
        if (TimeRemain > sd.maxValue)
            sd.maxValue = TimeRemain;
        sd.value = TimeRemain;

        Debug.Log("Current TimeRemain: " + TimeRemain);

    }

}