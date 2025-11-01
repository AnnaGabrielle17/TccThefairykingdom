using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movimento (valores default)")]
    public Vector2 direction = Vector2.left;
    public float speed = 8f;
    public float lifeTime = 2f;

    [Header("Dano")]
    public int damage = 1;
    public string targetTag = "Player";

    Rigidbody2D rb;
    Collider2D col;
    Vector2 spawnPos;
    private bool initialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb == null) Debug.LogWarning("EnemyProjectile: Rigidbody2D faltando!");
        else
        {
            rb.gravityScale = 0f;
            rb.angularVelocity = 0f;
            // deixar Dynamic normalmente; se usar Kinematic adapte o movimento.
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (col != null) col.isTrigger = true;
    }

    // Init recomendado para garantir valores em runtime (chame de EnemyFairy)
    public void Init(Vector2 dir, float speedValue, float lifeTimeValue, int damageValue)
    {
        direction = dir.normalized;
        speed = Mathf.Abs(speedValue); // garante positivo
        lifeTime = Mathf.Max(0.01f, lifeTimeValue);
        damage = damageValue;
        initialized = true;
    }

    void Start()
    {
        spawnPos = transform.position;

        // se não foi inicializado por Init(), usa os valores existentes no prefab
        if (!initialized)
        {
            // nothing: usa direction/speed/lifeTime do prefab
        }

        if (rb != null)
        {
            // aplica velocidade física clara
            rb.linearVelocity = (direction.normalized) * speed;
        }
        else
        {
            // fallback: movimento por transform
            Debug.LogWarning("EnemyProjectile: sem Rigidbody2D, movendo por Transform (menos ideal).");
        }

        // autodestroy por segurança
        Destroy(gameObject, lifeTime);

        Debug.Log($"[Projectile] spawned at {transform.position} dir={direction} speed={speed} lifeTime={lifeTime}");
    }

    void Update()
    {
        // proteção adicional: se por algum motivo o projétil não sair,
        // destruímos se passar da distância esperada (evita travar no mundo).
        float maxDist = speed * lifeTime * 1.2f;
        if (Vector2.Distance(spawnPos, transform.position) > maxDist)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // acerta jogador
        if (other.CompareTag(targetTag))
        {
            var fada = other.GetComponent<FadaDano>();
            if (fada != null)
            {
                fada.TryTakeDamageFromExternal(damage);
                Debug.Log("[Projectile] atingiu Player, damage=" + damage);
            }
            else
            {
                Debug.LogWarning("[Projectile] atingiu Player mas não encontrou FadaDano.");
            }

            Destroy(gameObject);
            return;
        }

        // você pode decidir colidir com cenário: exemplo abaixo destrói
        // mas se quiser que passe por objetos, filtre por Layer/Tag
        Destroy(gameObject);
    }
}