using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuração")]
    public float speed = 12f;          // usado apenas como fallback
    public int damage = 1;
    public float lifeTime = 4f;
    public bool destroyOnHit = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        // Segurança física: sem gravidade e sem rotação física
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.angularVelocity = 0f;
        }

        if (col != null) col.isTrigger = true;

        // Forçar visual neutro (evita flips/rotations herdados)
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            Mathf.Abs(transform.localScale.y),
            Mathf.Abs(transform.localScale.z)
        );

        if (sr != null)
        {
            sr.flipX = false;
            sr.flipY = false;
        }
    }

    /// <summary>
    /// Lança o projétil com uma velocidade vetorial (por exemplo: new Vector2(-speed, ySpeed)).
    /// Não rotaciona o transform — apenas aplica velocidade ao Rigidbody2D.
    /// </summary>
    public void Launch(Vector2 velocity, int damageValue = 1, float life = -1f)
    {
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            // fallback: move via transform (menor precisão)
            transform.Translate((Vector3)velocity * Time.deltaTime);
        }

        damage = damageValue;
        if (life > 0f) lifeTime = life;

        if (Application.isPlaying) Destroy(gameObject, lifeTime);
        else DestroyImmediate(gameObject);
    }

    // Compatibilidade: se alguém chamar Init(dir, speed,...)
    public void Init(Vector2 dir, float speedValue, float lifeTimeValue, int damageValue)
    {
        if (dir == Vector2.zero) dir = Vector2.left;
        Vector2 vel = dir.normalized * speedValue;
        Launch(vel, damageValue, lifeTimeValue);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            var fada = other.GetComponent<FadaDano>();
            if (fada != null)
            {
                fada.TryTakeDamageFromExternal(damage);
            }
            else
            {
                var ph = other.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage);
            }

            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        if (destroyOnHit) Destroy(gameObject);
    }
}