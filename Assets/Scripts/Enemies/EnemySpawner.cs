using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public Action OnEnemyDestroyed;

    int enemyCount;
    const int MaxEnemyCount = 10;

    [SerializeField] Enemy enemyPrefab;
    [SerializeField] float spawnInterval;
    [SerializeField] float spawnRangeFromPlayer;
    [SerializeField] Transform player;

    float spawnTimer;

    void Start()
    {
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval) return;

        SpawnEnemy();
        spawnTimer = 0;
    }

    void SpawnEnemy()
    {
        if (enemyCount >= MaxEnemyCount) return;

        enemyCount++;
        var spawnPosition = (Vector2)player.localPosition + Random.insideUnitCircle.normalized * spawnRangeFromPlayer;
        var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform);
        enemy.PlayerTransform = player;
        enemy.OnDestroy += EnemyDestroyed;
    }

    void EnemyDestroyed()
    {
        enemyCount--;
        OnEnemyDestroyed?.Invoke();
    }
}
