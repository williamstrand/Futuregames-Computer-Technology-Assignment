using System;
using UnityEngine;

/// <summary>
/// Initialization and updates.
/// </summary>
public partial class PlayerController : MonoBehaviour, IDamagable
{
    [Header("Components")]
    [SerializeField] new Transform transform;
    [SerializeField] Transform parentTransform;

    void Start()
    {
        maxHp = Hp;
    }

    void Update()
    {
        GetInput();
        UpdateSpeed();
        UpdateRotation();
        MoveCharacter();
        UpdateAttackTimer();
        UpdateHitInvulnerabilityTimer();
    }

    void GetInput()
    {
        TargetLookDirection = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
        if (Input.GetMouseButton(0))
        {
            TargetSpeed = MaxSpeed;
        }
        else
        {
            TargetSpeed = 0;
        }

        if (Input.GetMouseButton(1))
        {
            Shoot();
        }
    }
}

/// <summary>
/// Movement.
/// </summary>
public partial class PlayerController
{
    [Header("Movement")]
    [SerializeField] protected float MaxSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float rotationSpeed;

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
    }
}

/// <summary>
/// Combat.
/// </summary>
public partial class PlayerController
{
    public Action<int> OnHealthChange;
    public Action OnPlayerKilled;

    [field: Header("Combat")]
    [field: SerializeField] public int Hp { get; private set; }
    int maxHp;
    [SerializeField] Transform shootPoint;
    [SerializeField] PlayerProjectile projectilePrefab;
    [SerializeField] float attackSpeed;
    [SerializeField] int collisionDamage;
    float attackTimer;

    [SerializeField] float hitInvulnerabilityDuration = 1;
    float hitInvulnerabilityTimer;

    void Shoot()
    {
        if (attackTimer > 0) return;

        Instantiate(projectilePrefab, shootPoint.position, transform.rotation, parentTransform).gameObject.hideFlags = HideFlags.HideInHierarchy;
        attackTimer = 1 / attackSpeed;
    }

    void UpdateAttackTimer()
    {
        attackTimer -= Time.deltaTime;
    }

    void UpdateHitInvulnerabilityTimer()
    {
        hitInvulnerabilityTimer -= Time.deltaTime;
    }

    public void Damage(int damage)
    {
        if (hitInvulnerabilityTimer > 0) return;

        hitInvulnerabilityTimer = hitInvulnerabilityDuration;

        Hp -= damage;
        OnHealthChange?.Invoke(Hp);
        if (Hp <= 0)
        {
            Destroy();
        }
    }

    public void Heal(int amount)
    {
        Hp += amount;
        Hp = Mathf.Min(Hp, maxHp);
        OnHealthChange?.Invoke(Hp);
    }

    void Destroy()
    {
        OnPlayerKilled?.Invoke();
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Damage(collisionDamage);
        }
    }
}
