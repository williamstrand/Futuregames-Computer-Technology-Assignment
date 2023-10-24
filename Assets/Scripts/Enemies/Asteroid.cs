using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour, IDamagable
{
    public Action<Asteroid> OnDestroy;

    [SerializeField] new Transform transform;

    [SerializeField] Transform spriteTransform;
    [SerializeField] float rotationSpeed;
    [SerializeField] Vector2 minMaxMoveSpeed;

    [Header("Health Pack")]
    [SerializeField] HealthPack healthPackPrefab;
    [Range(0, 1)][SerializeField] float healthPackSpawnChance;
    float moveSpeed;
    public float MoveSpeed => moveSpeed;

    Vector3 moveDirection;
    public Vector3 MoveDirection => moveDirection;
    int rotationDirection;

    [SerializeField] float lifeTime = 10f;
    float lifeTimer;

    [SerializeField] int hp;

    void Start()
    {
        moveSpeed = Random.Range(minMaxMoveSpeed.x, minMaxMoveSpeed.y);
        rotationDirection = Random.Range(0, 2) * 2 - 1;
    }

    void Update()
    {
        spriteTransform.rotation *= Quaternion.Euler(0, 0, rotationSpeed * rotationDirection * Time.deltaTime);

        lifeTimer += Time.deltaTime * moveSpeed;
        if (lifeTimer > lifeTime)
        {
            Destroy();
        }
    }

    public void Initialize(float size, Vector3 targetDirection)
    {
        moveDirection = (targetDirection - transform.localPosition).normalized;
        transform.localScale *= size;
    }

    public void Damage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Destroy();
        }
    }

    void Destroy()
    {
        var random = Random.Range(0, 1f);
        if (random <= healthPackSpawnChance)
        {
            Instantiate(healthPackPrefab, ((Component)this).transform.position, Quaternion.identity);
        }
        OnDestroy?.Invoke(this);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!other.gameObject.TryGetComponent(out IDamagable damagable)) return;
            damagable.Damage(1);
            Destroy();
        }
        else
        {
            moveDirection = Vector2.Reflect(moveDirection, other.GetContact(0).normal);
        }
    }
}
