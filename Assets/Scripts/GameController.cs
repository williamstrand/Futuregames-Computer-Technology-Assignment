using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Action<int> OnScoreUpdated;
    public Action<int> OnGameOver;

    [SerializeField] PlayerController player;

    [SerializeField] UiController uiController;
    [SerializeField] CameraController cameraController;

    [Header("Spawners")]
    [SerializeField] AsteroidSpawner asteroidSpawner;
    [SerializeField] EnemySpawner enemySpawner;

    public int Score { get; private set; } = 0;
    const int ScoreOnEnemyDestroy = 500;
    const int ScoreOnAsteroidDestroy = 100;

    void OnEnable()
    {
        asteroidSpawner.OnAsteroidDestroyed += AsteroidDestroyed;
        enemySpawner.OnEnemyDestroyed += EnemyDestroyed;
        player.OnPlayerKilled += PlayerKilled;
    }

    void OnDisable()
    {
        asteroidSpawner.OnAsteroidDestroyed -= AsteroidDestroyed;
        enemySpawner.OnEnemyDestroyed -= EnemyDestroyed;
    }

    void Start()
    {
        Time.timeScale = 1;
        uiController.Initialize(player, this);
        cameraController.Initialize(player.transform);
    }

    void EnemyDestroyed()
    {
        Score += ScoreOnEnemyDestroy;
        OnScoreUpdated?.Invoke(Score);
    }

    void AsteroidDestroyed()
    {
        Score += ScoreOnAsteroidDestroy;
        OnScoreUpdated?.Invoke(Score);
    }

    void PlayerKilled()
    {
        Time.timeScale = 0.3f;
        OnGameOver?.Invoke(Score);
    }
}
