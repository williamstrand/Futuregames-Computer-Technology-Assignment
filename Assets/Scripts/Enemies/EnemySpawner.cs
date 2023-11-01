using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public Action OnEnemyDestroyed;

    [SerializeField] Enemy enemyPrefab;
    [SerializeField] float spawnInterval;
    [SerializeField] float spawnRangeFromPlayer;
    [SerializeField] Transform player;

    [SerializeField] int PoolSize = 10;
    List<Enemy> spawnedEnemies = new List<Enemy>();
    List<Enemy> enemyPool = new List<Enemy>();

    float spawnTimer;

    void Start()
    {
        spawnTimer = spawnInterval;
        for (int i = 0; i < PoolSize; i++)
        {
            var enemy = Instantiate(enemyPrefab, transform);
            DisableEnemy(enemy);
        }
    }

    void Update()
    {
        UpdateEnemyPositions();
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval) return;

        SpawnEnemy();
        spawnTimer = 0;
    }

    void SpawnEnemy()
    {
        if (enemyPool.Count == 0) return;

        var spawnPosition = (Vector2)player.localPosition + Random.insideUnitCircle.normalized * spawnRangeFromPlayer;
        var enemy = enemyPool[0];
        EnableEnemy(enemy);
        enemy.transform.position = spawnPosition;
        enemy.Initialize();
        enemy.PlayerTransform = player;
        enemy.OnDestroy += EnemyDestroyed;
        spawnedEnemies.Add(enemy);
    }

    void EnemyDestroyed(Enemy enemy)
    {
        enemy.OnDestroy -= EnemyDestroyed;
        spawnedEnemies.Remove(enemy);
        DisableEnemy(enemy);
        OnEnemyDestroyed?.Invoke();
    }

    void DisableEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemyPool.Add(enemy);
        enemy.gameObject.hideFlags = HideFlags.HideInHierarchy;
    }

    void EnableEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemyPool.Remove(enemy);
        enemy.gameObject.hideFlags = HideFlags.None;

    }

    void UpdateEnemyPositions()
    {
        if (spawnedEnemies.Count <= 0) return;

        var positionArray = new NativeArray<float3>(spawnedEnemies.Count, Allocator.TempJob);
        var moveDirectionArray = new NativeArray<float3>(spawnedEnemies.Count, Allocator.TempJob);
        var moveSpeedArray = new NativeArray<float>(spawnedEnemies.Count, Allocator.TempJob);
        var targetDirectionArray = new NativeArray<float2>(spawnedEnemies.Count, Allocator.TempJob);
        var rotationArray = new NativeArray<quaternion>(spawnedEnemies.Count, Allocator.TempJob);

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            positionArray[i] = spawnedEnemies[i].transform.position;
            moveDirectionArray[i] = spawnedEnemies[i].transform.right;
            moveSpeedArray[i] = spawnedEnemies[i].CurrentSpeed;
            targetDirectionArray[i] = spawnedEnemies[i].TargetLookDirection;
        }

        var job = new EnemyMoveJob
        {
            PositionsArray = positionArray,
            MoveDirectionArray = moveDirectionArray,
            MoveSpeedArray = moveSpeedArray,
            TargetDirectionArray = targetDirectionArray,
            RotationArray = rotationArray,
            RotationSpeed = spawnedEnemies[0].RotationSpeed,
            DeltaTime = Time.deltaTime
        };

        var jobHandle = job.Schedule(spawnedEnemies.Count, 100);
        jobHandle.Complete();

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            spawnedEnemies[i].transform.SetPositionAndRotation(positionArray[i], rotationArray[i]);
        }

        positionArray.Dispose();
        moveDirectionArray.Dispose();
        moveSpeedArray.Dispose();
        targetDirectionArray.Dispose();
        rotationArray.Dispose();
    }

    [BurstCompile]
    struct EnemyMoveJob : IJobParallelFor
    {
        public NativeArray<float3> PositionsArray;
        public NativeArray<float3> MoveDirectionArray;
        public NativeArray<float> MoveSpeedArray;
        public NativeArray<float2> TargetDirectionArray;
        public NativeArray<quaternion> RotationArray;

        public float DeltaTime;
        public float RotationSpeed;

        public void Execute(int index)
        {
            PositionsArray[index] += MoveSpeedArray[index] * DeltaTime * MoveDirectionArray[index];
            var targetAngle = math.atan2(TargetDirectionArray[index].y, TargetDirectionArray[index].x);
            var currentAngle = math.atan2(MoveDirectionArray[index].y, MoveDirectionArray[index].x);
            var angle = math.lerp(currentAngle, targetAngle, DeltaTime * RotationSpeed);
            RotationArray[index] = quaternion.EulerXYZ(0, 0, angle);

        }
    }
}
