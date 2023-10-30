using System;
using System.Collections.Generic;
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
        var rotationArray = new NativeArray<float>(spawnedEnemies.Count, Allocator.TempJob);

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            positionArray[i] = spawnedEnemies[i].transform.position;
            moveDirectionArray[i] = spawnedEnemies[i].transform.right;
            moveSpeedArray[i] = spawnedEnemies[i].CurrentSpeed;
            rotationArray[i] = spawnedEnemies[i].transform.rotation.eulerAngles.z;
        }

        var job = new EnemyMoveJob
        {
            PositionsArray = positionArray,
            MoveDirectionArray = moveDirectionArray,
            MoveSpeedArray = moveSpeedArray,
            RotationArray = rotationArray,
            DeltaTime = Time.deltaTime
        };

        var jobHandle = job.Schedule(spawnedEnemies.Count, 100);
        jobHandle.Complete();

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            spawnedEnemies[i].transform.SetPositionAndRotation(positionArray[i], Quaternion.Euler(0, 0, rotationArray[i]));
        }

        positionArray.Dispose();
        moveDirectionArray.Dispose();
        moveSpeedArray.Dispose();
        rotationArray.Dispose();
    }

    struct EnemyMoveJob : IJobParallelFor
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
        }
    }
}
