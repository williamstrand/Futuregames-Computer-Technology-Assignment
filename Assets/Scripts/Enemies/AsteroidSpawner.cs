using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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

    List<Asteroid> spawnedAsteroids = new List<Asteroid>();

    float spawnTimer;

    public struct AsteroidInfo
    {
        public float3 Position;
        public float3 MoveDirection;
        public float MoveSpeed;
        public float DeltaTime;
    }

    void Update()
    {
        UpdateAsteroidPosition();

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
        spawnedAsteroids.Add(asteroid);
    }

    void AsteroidDestroyed(Asteroid asteroid)
    {
        spawnedAsteroids.Remove(asteroid);
        OnAsteroidDestroyed?.Invoke();
    }

    void UpdateAsteroidPosition()
    {
        var input = new NativeArray<AsteroidInfo>(1000, Allocator.Persistent);
        var output = new NativeArray<AsteroidInfo>(1000, Allocator.Persistent);

        for (int i = 0; i < spawnedAsteroids.Count; i++)
        {
            input[i] = new AsteroidInfo
            {
                Position = spawnedAsteroids[i].transform.localPosition,
                MoveSpeed = spawnedAsteroids[i].MoveSpeed,
                MoveDirection = spawnedAsteroids[i].MoveDirection,
                DeltaTime = Time.deltaTime
            };
        }

        var job = new AsteroidMoveJob
        {
            Input = input,
            Output = output
        };

        job.Schedule().Complete();

        for (int i = 0; i < spawnedAsteroids.Count; i++)
        {
            spawnedAsteroids[i].transform.localPosition = output[i].Position;
        }

        input.Dispose();
        output.Dispose();
    }
}


[BurstCompile(CompileSynchronously = true)]
struct AsteroidMoveJob : IJob
{
    [ReadOnly] public NativeArray<AsteroidSpawner.AsteroidInfo> Input;
    [WriteOnly] public NativeArray<AsteroidSpawner.AsteroidInfo> Output;

    public void Execute()
    {
        for (int i = 0; i < Input.Length; i++)
        {
            var output = Input[i];
            output.Position += Input[i].MoveSpeed * Input[i].DeltaTime * Input[i].MoveDirection;
            Output[i] = output;
        }
    }
}