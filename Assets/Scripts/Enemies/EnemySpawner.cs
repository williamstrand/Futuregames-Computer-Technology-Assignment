using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
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

    JobHandle jobHandle;

    float spawnTimer;
    Enemy[] allEnemies;
    TransformAccessArray enemyTransforms;

    NativeArray<Vector3> moveDirectionArray;
    NativeArray<float> moveSpeedArray;
    NativeArray<float2> targetDirectionArray;

    void Start()
    {
        allEnemies = new Enemy[PoolSize];
        var array = new Transform[PoolSize];
        spawnTimer = spawnInterval;
        for (int i = 0; i < PoolSize; i++)
        {
            var enemy = Instantiate(enemyPrefab, transform);
            allEnemies[i] = enemy;
            array[i] = enemy.transform;
            DisableEnemy(enemy);
        }

        enemyTransforms = new TransformAccessArray(array);
    }

    void OnEnable()
    {
        moveDirectionArray = new NativeArray<Vector3>(PoolSize, Allocator.TempJob);
        moveSpeedArray = new NativeArray<float>(PoolSize, Allocator.TempJob);
        targetDirectionArray = new NativeArray<float2>(PoolSize, Allocator.TempJob);
    }

    void OnDisable()
    {
        moveDirectionArray.Dispose();
        moveSpeedArray.Dispose();
        targetDirectionArray.Dispose();
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
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            moveDirectionArray[i] = allEnemies[i].transform.right;
            moveSpeedArray[i] = allEnemies[i].CurrentSpeed;
            targetDirectionArray[i] = allEnemies[i].TargetLookDirection;
        }

        var job = new EnemyMoveJob
        {
            MoveDirectionArray = moveDirectionArray,
            MoveSpeedArray = moveSpeedArray,
            TargetDirectionArray = targetDirectionArray,
            RotationSpeed = allEnemies[0].RotationSpeed,
            DeltaTime = Time.deltaTime
        };

        jobHandle = job.Schedule(enemyTransforms);
    }

    void LateUpdate()
    {
        jobHandle.Complete();
    }

    [BurstCompile]
    struct EnemyMoveJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> MoveDirectionArray;
        public NativeArray<float> MoveSpeedArray;
        public NativeArray<float2> TargetDirectionArray;

        public float DeltaTime;
        public float RotationSpeed;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position += MoveSpeedArray[index] * DeltaTime * MoveDirectionArray[index];
            var targetAngle = math.atan2(TargetDirectionArray[index].y, TargetDirectionArray[index].x);
            var currentAngle = math.atan2(MoveDirectionArray[index].y, MoveDirectionArray[index].x);
            var angle = math.lerp(currentAngle, targetAngle, DeltaTime * RotationSpeed);
            transform.rotation = quaternion.EulerXYZ(0, 0, angle);
        }
    }
}
