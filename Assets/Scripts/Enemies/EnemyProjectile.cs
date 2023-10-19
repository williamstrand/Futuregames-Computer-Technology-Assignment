using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] new Transform transform;
    [SerializeField] float speed;
    [SerializeField] float lifeTime;
    [SerializeField] int damage;

    float currentLifeTime;

    void Update()
    {
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            Destroy(gameObject);
        }

    }

    void FixedUpdate()
    {
        transform.localPosition += speed * Time.fixedDeltaTime * transform.right;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (!collision.TryGetComponent(out IDamagable damagable)) return;

        damagable.Damage(damage);
        Destroy(gameObject);
    }
}
