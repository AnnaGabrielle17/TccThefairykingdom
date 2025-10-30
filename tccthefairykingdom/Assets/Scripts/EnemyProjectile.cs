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

    // componentes
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // configurações seguras por script (pode ajustar no Inspector também)
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.angularVelocity = 0f;
            rb.isKinematic = false; // usando velocity
        }

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Start()
    {
        // aplica velocidade inicial (caso Init não tenha sido chamado)
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    // Método público seguro para inicializar o projétil ao instanciar
    public void Init(Vector2 dir, float spd, float lifeTimeSec, int dmg)
    {
        direction = dir.normalized;
        speed = spd;
        lifeTime = lifeTimeSec;
        damage = dmg;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // reinicia timer de destruição
        CancelInvoke(nameof(DestroySelf));
        Invoke(nameof(DestroySelf), Mathf.Max(0.01f, lifeTime));
    }

    void DestroySelf()
    {
        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (other.CompareTag(targetTag))
        {
            var dano = other.GetComponent<FadaDano>();
            if (dano != null)
            {
                dano.TryTakeDamageFromExternal(damage);
            }
            else
            {
                Debug.LogWarning("EnemyProjectile: jogador atingido mas não encontrou FadaDano no objeto com Tag 'Player'.");
            }

            DestroySelf();
        }
        else
        {
            // opcional: destruir ao colidir com cenário (descomente e ajuste layer)
            // if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) DestroySelf();
        }
    }
}