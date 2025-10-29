using UnityEngine;

public class EnemyProjectile1 : MonoBehaviour
{
   [Header("Movimento (privado, use Init)")]
    [SerializeField] private Vector2 direction = Vector2.left;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Dano")]
    [SerializeField] private int damage = 1;
    [SerializeField] private string targetTag = "Player";

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.angularVelocity = 0f;
        rb.isKinematic = false;
        col.isTrigger = true;
    }

    void Start()
    {
        // aplica velocidade inicial (caso Init não tenha sido chamado antes do Start)
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Inicializa/configura o projétil logo após instanciá-lo.
    /// </summary>
    public void Init(Vector2 dir, float spd, float life, int dmg, string targetTagOverride = null)
    {
        direction = dir.normalized;
        speed = spd;
        lifeTime = life;
        damage = dmg;
        if (!string.IsNullOrEmpty(targetTagOverride)) targetTag = targetTagOverride;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        // garante destruição no tempo pedido (reinicia a contagem)
        CancelInvoke(nameof(DestroySelf));
        Invoke(nameof(DestroySelf), lifeTime);
    }

    void DestroySelf()
    {
        if (gameObject != null) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (other.CompareTag(targetTag))
        {
            var dano = other.GetComponent<FadaDano>();
            if (dano != null)
            {
                // usa a API existente da fada (respeita intervaloDano e piscar)
                dano.TryTakeDamageFromExternal(damage);
            }
            else
            {
                Debug.LogWarning("EnemyProjectile: jogador atingido mas não encontrou FadaDano no objeto com Tag 'Player'.");
            }

            Destroy(gameObject);
        }
    }
}