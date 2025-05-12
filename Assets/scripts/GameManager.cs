using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float gameSpeed = 1f;
    public float difficultyIncreaseRate = 0.1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Optionally increase speed over time
        gameSpeed += difficultyIncreaseRate * Time.deltaTime;
    }
}
