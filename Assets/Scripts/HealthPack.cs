using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [SerializeField] int healAmount;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent<PlayerController>(out var player)) return;

        player.Heal(healAmount);

        Destroy(gameObject);
    }
}
