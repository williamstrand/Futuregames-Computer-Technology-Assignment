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
    [SerializeField] int PoolSize = 100;

    List<Asteroid> spawnedAsteroids = new List<Asteroid>();
    List<Asteroid> asteroidPool = new List<Asteroid>();

    float spawnTimer;

    void Start()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var asteroid = Instantiate(asteroidPrefab, transform);
            DisableAsteroid(asteroid);
        }
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
        if (asteroidPool.Count == 0) return;

        var spawnPosition = (Vector2)player.localPosition + Random.insideUnitCircle.normalized * spawnRangeFromPlayer;
        var size = Random.Range(minMaxSize.x, minMaxSize.y + 1);
        var asteroid = asteroidPool[0];
        EnableAsteroid(asteroid);
        asteroid.transform.position = spawnPosition;
        asteroid.Initialize(size, player.localPosition);
        asteroid.OnDestroy += AsteroidDestroyed;
        spawnedAsteroids.Add(asteroid);
    }

    void EnableAsteroid(Asteroid asteroid)
    {
        asteroid.gameObject.SetActive(true);
        asteroidPool.Remove(asteroid);
        asteroid.gameObject.hideFlags = HideFlags.None;
    }

    void DisableAsteroid(Asteroid asteroid)
    {
        asteroid.gameObject.SetActive(false);
        asteroidPool.Add(asteroid);
        asteroid.gameObject.hideFlags = HideFlags.HideInHierarchy;
    }

    void AsteroidDestroyed(Asteroid asteroid)
    {
        spawnedAsteroids.Remove(asteroid);
        DisableAsteroid(asteroid);
        OnAsteroidDestroyed?.Invoke();
    }

    void UpdateAsteroidPosition()
    {
        if (spawnedAsteroids.Count <= 0) return;

        var positionArray = new NativeArray<float3>(spawnedAsteroids.Count, Allocator.TempJob);
        var moveDirectionArray = new NativeArray<float3>(spawnedAsteroids.Count, Allocator.TempJob);
        var moveSpeedArray = new NativeArray<float>(spawnedAsteroids.Count, Allocator.TempJob);
        var rotationArray = new NativeArray<float>(spawnedAsteroids.Count, Allocator.TempJob);

        for (int i = 0; i < spawnedAsteroids.Count; i++)
        {
            positionArray[i] = spawnedAsteroids[i].transform.position;
            moveDirectionArray[i] = spawnedAsteroids[i].MoveDirection;
            moveSpeedArray[i] = spawnedAsteroids[i].MoveSpeed;
            rotationArray[i] = spawnedAsteroids[i].transform.rotation.eulerAngles.z;
        }

        var job = new AsteroidMoveJob
        {
            PositionsArray = positionArray,
            MoveDirectionArray = moveDirectionArray,
            MoveSpeedArray = moveSpeedArray,
            RotationArray = rotationArray,
            RotationSpeed = spawnedAsteroids[0].RotationSpeed,
            DeltaTime = Time.deltaTime
        };

        var jobHandle = job.Schedule(spawnedAsteroids.Count, 50);
        jobHandle.Complete();

        for (int i = 0; i < spawnedAsteroids.Count; i++)
        {
            spawnedAsteroids[i].transform.SetPositionAndRotation(positionArray[i], Quaternion.Euler(0, 0, rotationArray[i]));
        }

        positionArray.Dispose();
        moveDirectionArray.Dispose();
        moveSpeedArray.Dispose();
        rotationArray.Dispose();
    }

    [BurstCompile]
    struct AsteroidMoveJob : IJobParallelFor
    {
        public NativeArray<float3> PositionsArray;
        public NativeArray<float3> MoveDirectionArray;
        public NativeArray<float> MoveSpeedArray;
        public NativeArray<float> RotationArray;

        public float RotationSpeed;
        public float DeltaTime;

        public void Execute(int index)
        {
            PositionsArray[index] += MoveSpeedArray[index] * DeltaTime * MoveDirectionArray[index];
            RotationArray[index] += RotationSpeed * DeltaTime;
        }
    }
}