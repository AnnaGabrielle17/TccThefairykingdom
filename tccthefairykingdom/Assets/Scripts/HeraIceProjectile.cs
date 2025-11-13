using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HeraIceProjectile : MonoBehaviour
{
    [Header("Propriedades do projétil")]
    public int damage = 25;
    public float lifeTime = 5f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            // opcional: deixar kinematic se você não usa física
            // rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // Método público que o HeraBossController está chamando
    public void Init(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        // Destrói automaticamente após lifeTime
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ao colidir com o player, tenta aplicar dano (compatível com seu sistema)
        if (other.CompareTag("Player"))
        {
            var fada = other.GetComponent<FadaDano>();
            if (fada != null)
            {
                // usa TryTakeDamageFromExternal para respeitar intervaloDano do teu script
                fada.TryTakeDamageFromExternal(damage);
            }
            else
            {
                var ph = other.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
            }

            Destroy(gameObject);
            return;
        }

        // Se bater em um obstáculo/chão (opcional: ajustar por layers/tags)
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            other.gameObject.layer == LayerMask.NameToLayer("Obstacles") ||
            other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }
    }
}