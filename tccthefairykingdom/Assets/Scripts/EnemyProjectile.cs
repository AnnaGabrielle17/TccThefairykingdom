using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public Vector2 direction = Vector2.left;
    public float speed = 22f;       // ajuste no Inspector
    public float lifeTime = 6f;     // ajuste no Inspector

    [Header("Dano")]
    public int damage = 1;
    public string targetTag = "Player";

    Rigidbody2D rb;
    Collider2D col;
    private Vector2 spawnPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.angularVelocity = 0f;
            rb.isKinematic = false;
        }

        if (col != null)
            col.isTrigger = true;
    }

    void Start()
    {
        spawnPos = transform.position;

        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
        else
            Debug.LogWarning("EnemyProjectile: Rigidbody2D não encontrado — movimento por física não será aplicado.");

        // Safety: autodestroy por tempo
        Destroy(gameObject, lifeTime);
        Debug.Log($"[Projectile] spawned at {spawnPos} dir={direction} speed={speed} lifeTime={lifeTime}");
    }

    void Update()
    {
        // opcional: destrói se exceder certa distância (proteção extra)
        float maxDist = speed * lifeTime * 1.1f; // margem
        if (Vector2.Distance(spawnPos, transform.position) > maxDist)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // se atingir o jogador (por tag)
        if (other.CompareTag(targetTag))
        {
            var fada = other.GetComponent<FadaDano>();
            if (fada != null)
            {
                fada.TryTakeDamageFromExternal(damage);
            }
            else
            {
                Debug.LogWarning("EnemyProjectile: Player atingido mas não encontrou FadaDano no objeto.");
            }

            Destroy(gameObject);
            return;
        }

        // opcional: destruir ao colidir com cenário/obstáculo — se não quiser, comente
        Destroy(gameObject);
    }
}