using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BasketballThrower : MonoBehaviour
{
    public GameObject basketballPrefab;
    public Transform throwOrigin;
    public float throwForce = 14f;
    public float spinForce = 3f;

    public float throwCooldown = 1f;
    private float lastThrowTime = -Mathf.Infinity;

    [Header("포물선 조정")]
    [Range(0f, 2f)] public float forwardMultiplier = 1.0f;
    [Range(0f, 2f)] public float upwardMultiplier = 0.9f;
    public float throwAngle = 35f;

    // 생성된 농구공을 추적할 리스트
    private List<GameObject> spawnedBalls = new List<GameObject>();

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void Update()
    {
        if (!IsMissionBasketballSceneLoaded())
            return;

        bool isThrowInput = Input.GetKeyDown(KeyCode.N)
                            || Input.GetKeyDown(KeyCode.JoystickButton0)
                            || Input.GetMouseButtonDown(0);

        if (isThrowInput && Time.time - lastThrowTime > throwCooldown)
        {
            ThrowNewBall();
            lastThrowTime = Time.time;
        }
    }

    bool IsMissionBasketballSceneLoaded()
    {
        return SceneManager.GetSceneByName("MissionBasketballScene").isLoaded;
    }

    void ThrowNewBall()
    {
        GameObject newBall = Instantiate(basketballPrefab, throwOrigin.position, Quaternion.identity);
        spawnedBalls.Add(newBall);

        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        Vector3 throwDirection = throwOrigin.forward.normalized;
        throwDirection.y += 0.7f;
        throwDirection = throwDirection.normalized;

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        rb.AddTorque(Vector3.right * spinForce, ForceMode.Impulse);
    }

    void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "MissionBasketballScene")
        {
            // 생성된 농구공 모두 삭제
            foreach (var ball in spawnedBalls)
            {
                if (ball != null)
                    Destroy(ball);
            }
            spawnedBalls.Clear();
        }
    }
}