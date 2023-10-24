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

    const int MaxEnemyCount = 10;

    [SerializeField] Enemy enemyPrefab;
    [SerializeField] float spawnInterval;
    [SerializeField] float spawnRangeFromPlayer;
    [SerializeField] Transform player;

    List<Enemy> spawnedEnemies = new List<Enemy>();

    float spawnTimer;

    void Start()
    {
        spawnTimer = spawnInterval;
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
        if (spawnedEnemies.Count >= MaxEnemyCount) return;

        var spawnPosition = (Vector2)player.localPosition + Random.insideUnitCircle.normalized * spawnRangeFromPlayer;
        var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform);
        enemy.PlayerTransform = player;
        enemy.OnDestroy += EnemyDestroyed;
        spawnedEnemies.Add(enemy);
    }

    void EnemyDestroyed(Enemy enemy)
    {
        spawnedEnemies.Remove(enemy);
        OnEnemyDestroyed?.Invoke();
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

        var jobHandle = job.Schedule(spawnedEnemies.Count, 50);
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
