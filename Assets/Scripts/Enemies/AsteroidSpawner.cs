using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    public Action OnAsteroidDestroyed;

    [SerializeField] Asteroid asteroidPrefab;
    [SerializeField] float spawnInterval;
    [SerializeField] float spawnRangeFromPlayer;
    [SerializeField] Vector2 minMaxSize;
    [SerializeField] Transform player;

    float spawnTimer;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval) return;

        SpawnAsteroid();
        spawnTimer = 0;
    }

    void SpawnAsteroid()
    {
        var spawnPosition = (Vector2)player.localPosition + Random.insideUnitCircle.normalized * spawnRangeFromPlayer;
        var size = Random.Range(minMaxSize.x, minMaxSize.y + 1);
        var asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity, transform);
        asteroid.Initialize(size, player.localPosition);
        asteroid.OnDestroy += AsteroidDestroyed;
    }

    void AsteroidDestroyed()
    {
        OnAsteroidDestroyed?.Invoke();
    }
}
