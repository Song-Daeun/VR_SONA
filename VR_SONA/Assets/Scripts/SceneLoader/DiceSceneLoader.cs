using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiceSceneLoader : MonoBehaviour
{
    public Transform cameraTransform;
    public float distanceInFront = 20f;
    public float heightOffset = 0f;
    public bool faceCamera = true;

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0; // 수평만 고려
        forward.Normalize();

        Vector3 targetPosition = cameraTransform.position + forward * distanceInFront;
        targetPosition.y += heightOffset;
        transform.position = targetPosition;

        if (faceCamera)
        {
            Vector3 lookDir = transform.position - cameraTransform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    public IEnumerator LoadDiceScene()
    {
        if (SceneManager.GetSceneByName("DiceScene").isLoaded)
        {
            Debug.Log("🎲 DiceScene already loaded");
            yield break;
        }

        var asyncLoad = SceneManager.LoadSceneAsync("DiceScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);
        Debug.Log("🎲 DiceScene loaded");
    }

    public IEnumerator UnloadDiceScene()
    {
        Scene diceScene = SceneManager.GetSceneByName("DiceScene");
        if (!diceScene.IsValid() || !diceScene.isLoaded)
        {
            Debug.Log("🎲 DiceScene not loaded");
            yield break;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync("DiceScene");
        yield return new WaitUntil(() => asyncUnload.isDone);
        Debug.Log("🧹 DiceScene unloaded");
    }
}
