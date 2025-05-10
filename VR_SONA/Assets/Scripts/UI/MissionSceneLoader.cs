using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSceneLoader : MonoBehaviour
{
    public void LoadMissionScene()
    {
        SceneManager.LoadScene("MissionBasketballScene", LoadSceneMode.Additive);
    }

    public void UnloadMissionScene()
    {
        SceneManager.UnloadSceneAsync("MissionBasketballScene");
    }
}
