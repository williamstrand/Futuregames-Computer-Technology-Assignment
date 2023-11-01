using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
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

    TransformAccessArray spawnedAsteroidsTransforms;
    Asteroid[] allAsteroids;
    List<Asteroid> spawnedAsteroids = new List<Asteroid>();
    List<Asteroid> asteroidPool = new List<Asteroid>();

    NativeArray<Vector3> positionArray;
    NativeArray<Quaternion> rotationArray;
    NativeArray<Vector3> moveDirectionArray;
    NativeArray<float> moveSpeedArray;
    NativeArray<bool> isEnabledArray;

    float spawnTimer;

    JobHandle jobHandle;

    void Start()
    {
        allAsteroids = new Asteroid[PoolSize];
        var array = new List<Transform>();
        for (int i = 0; i < PoolSize; i++)
        {
            var asteroid = Instantiate(asteroidPrefab, transform);
            array.Add(asteroid.transform);
            allAsteroids[i] = asteroid;
            DisableAsteroid(asteroid);
        }

        spawnedAsteroidsTransforms = new TransformAccessArray(array.ToArray());
    }

    void OnEnable()
    {
        positionArray = new NativeArray<Vector3>(PoolSize, Allocator.TempJob);
        rotationArray = new NativeArray<Quaternion>(PoolSize, Allocator.TempJob);
        moveDirectionArray = new NativeArray<Vector3>(PoolSize, Allocator.TempJob);
        moveSpeedArray = new NativeArray<float>(PoolSize, Allocator.TempJob);
        isEnabledArray = new NativeArray<bool>(PoolSize, Allocator.TempJob);
    }

    void OnDisable()
    {
        positionArray.Dispose();
        rotationArray.Dispose();
        moveDirectionArray.Dispose();
        moveSpeedArray.Dispose();
        isEnabledArray.Dispose();
    }

    void Update()
    {
        UpdateAsteroidPosition();

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval) return;

        SpawnAsteroid();
        spawnTimer = 0;
    }

    void LateUpdate()
    {
        jobHandle.Complete();
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
        asteroid.IsEnabled = true;
        asteroidPool.Remove(asteroid);
        asteroid.gameObject.hideFlags = HideFlags.None;
    }

    void DisableAsteroid(Asteroid asteroid)
    {
        asteroid.gameObject.SetActive(false);
        asteroid.IsEnabled = false;
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

        for (int i = 0; i < PoolSize; i++)
        {
            moveDirectionArray[i] = allAsteroids[i].MoveDirection;
            moveSpeedArray[i] = allAsteroids[i].MoveSpeed;
            isEnabledArray[i] = allAsteroids[i].IsEnabled;
        }

        var calculateMovementJob = new AsteroidCalculateMovementJob
        {
            PositionArray = positionArray,
            RotationArray = rotationArray,
            MoveDirectionArray = moveDirectionArray,
            MoveSpeedArray = moveSpeedArray,
            IsEnabledArray = isEnabledArray,

            RotationSpeed = allAsteroids[0].RotationSpeed,
            DeltaTime = Time.deltaTime
        };

        jobHandle = calculateMovementJob.Schedule(spawnedAsteroidsTransforms);
    }

    void OnDestroy()
    {
        spawnedAsteroidsTransforms.Dispose();
    }

    [BurstCompile]
    struct AsteroidCalculateMovementJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> PositionArray;
        public NativeArray<Quaternion> RotationArray;
        [ReadOnly] public NativeArray<Vector3> MoveDirectionArray;
        [ReadOnly] public NativeArray<float> MoveSpeedArray;
        [ReadOnly] public NativeArray<bool> IsEnabledArray;

        [ReadOnly] public float RotationSpeed;
        [ReadOnly] public float DeltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            if (!IsEnabledArray[index]) return;
            PositionArray[index] = transform.position;
            RotationArray[index] = transform.rotation;

            transform.position += MoveSpeedArray[index] * DeltaTime * MoveDirectionArray[index];
            transform.rotation *= quaternion.EulerXYZ(0, 0, RotationSpeed * DeltaTime * Mathf.Deg2Rad);
        }
    }
}