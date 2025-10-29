using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public Vector2 direction = Vector2.left;
    public float speed = 8f;
    public float lifeTime = 2f;

    [Header("Dano")]
    public int damage = 1;
    public string targetTag = "Player";

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        // configurações seguras por script (pode ajustar no Inspector também)
        rb.gravityScale = 0f;
        rb.angularVelocity = 0f;
        // deixamos kinematic = false para usar velocity; se preferir Kinematic, mude e use MovePosition.
        rb.isKinematic = false;
        col.isTrigger = true;
    }

    void Start()
    {
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // só reage ao jogador (usa tag)
        if (other.CompareTag(targetTag))
        {
            var dano = other.GetComponent<FadaDano>();
            if (dano != null)
            {
                // usa a função pública existente (respeita intervaloDano e faz piscar)
                dano.TryTakeDamageFromExternal(damage);
            }
            else
            {
                // fallback: tenta encontrar PlayerHealth ou outro
                Debug.LogWarning("EnemyProjectile: jogador atingido mas não encontrou FadaDano no objeto com Tag 'Player'.");
            }

            Destroy(gameObject);
        }
        else
        {
            // opcional: destruir ao colidir com cenário
            // if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) Destroy(gameObject);
        }
    }
}