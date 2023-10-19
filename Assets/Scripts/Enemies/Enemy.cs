using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    public Action OnDestroy;

    [SerializeField] new Transform transform;
    [HideInInspector] public Transform PlayerTransform;

    [Header("Movement")]
    [SerializeField] protected float MaxSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float rotationSpeed;

    [Header("Combat")]
    [SerializeField] int hp;
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float attackSpeed;
    [SerializeField] Transform shootPoint;
    [SerializeField] int collisionDamage;
    float attackTimer;


    float targetSpeed;
    protected float TargetSpeed
    {
        get => targetSpeed;
        set => targetSpeed = Mathf.Clamp(value, 0, MaxSpeed);
    }

    float currentSpeed;

    Vector2 targetLookDirection;

    protected Vector2 TargetLookDirection
    {
        get => targetLookDirection;
        set => targetLookDirection = value.normalized;
    }

    void Update()
    {
        UpdateSpeed();
        UpdateRotation();
        MoveCharacter();
        TargetLookDirection = PlayerTransform.position - transform.position;
        TargetSpeed = MaxSpeed;
        UpdateAttackTimer();
    }

    void UpdateAttackTimer()
    {
        attackTimer -= Time.deltaTime;
    }

    void MoveCharacter()
    {
        var velocity = transform.right * currentSpeed;
        transform.localPosition += velocity * Time.deltaTime;
    }

    void UpdateSpeed()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, TargetSpeed, Time.deltaTime * acceleration);
    }

    void UpdateRotation()
    {
        var targetAngle = Mathf.Atan2(TargetLookDirection.y, TargetLookDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), Time.deltaTime * rotationSpeed);
        var currentAngle = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;

        if (!(Mathf.Abs(targetAngle - currentAngle) < 60)) return;
        if (Vector2.Distance(transform.position, PlayerTransform.position) > 10) return;

        Shoot();
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
        OnDestroy?.Invoke();
        Destroy(gameObject);
    }

    void Shoot()
    {
        if (attackTimer > 0) return;

        Instantiate(projectilePrefab, shootPoint.position, transform.rotation).gameObject.hideFlags = HideFlags.HideInHierarchy;
        attackTimer = 1 / attackSpeed;
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Damage(collisionDamage);
        }
    }
}
