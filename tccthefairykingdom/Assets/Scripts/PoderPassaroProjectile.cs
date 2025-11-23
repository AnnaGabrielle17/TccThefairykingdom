using UnityEngine;

/// <summary>
/// Projétil do pássaro. Notifica o shooter quando é destruído/desativado para que ele remova da lista.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PoderPassaroProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6f;
    public Vector2 direction = Vector2.left; // por padrão vai para a esquerda
    public float lifeTime = 6f;

    [Header("Dano")]
    public int damage = 1;

    [HideInInspector] public BirdController owner; // referência ao pássaro que disparou (opcional)
    [HideInInspector] public BirdShooter shooter;  // referência ao shooter que gerencia este projétil (opcional)

    Rigidbody2D rb;
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        // apenas define a velocidade se já foi inicializado
        if (initialized && rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        // schedule destruction by lifeTime (se estiver usando pooling, essa linha pode ser substituída por lógica do pool)
        CancelInvoke(nameof(SelfDestruct));
        if (lifeTime > 0f) Invoke(nameof(SelfDestruct), lifeTime);
    }

    private void OnDisable()
    {
        // Notifica o shooter que este projétil foi destruído/desativado (pool)
        if (shooter != null)
        {
            shooter.NotifyProjectileDestroyed(this);
        }
        // cancelar qualquer invoke pendente
        CancelInvoke(nameof(SelfDestruct));
    }

    private void OnDestroy()
    {
        // safety: também notifica caso seja destruído
        if (shooter != null)
        {
            shooter.NotifyProjectileDestroyed(this);
        }
    }

    private void SelfDestruct()
    {
        // tenta usar pooling se existir, senão destrói
        var pooled = GetComponent<PooledObject>();
        if (pooled != null)
        {
            try { pooled.ReturnToPool(); return; }
            catch { /* fallback */ }
        }

        Destroy(gameObject);
    }

    // permite ajustar a direção/velocidade logo após instanciar
    public void Initialize(Vector2 dir, float speedOverride = -1f, int damageOverride = -1, BirdController owner = null, BirdShooter shooter = null)
    {
        direction = dir.normalized;
        if (speedOverride > 0) speed = speedOverride;
        if (damageOverride >= 0) damage = damageOverride;
        this.owner = owner;
        this.shooter = shooter;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        initialized = true;
    }

    private void Update()
    {
        // segurança: se o dono existir e estiver marcado como morto, auto-destrói (evita acertar player após a morte do dono)
        if (owner != null)
        {
            var bc = owner;
            if (bc != null && bc.IsDead)
            {
                // se pooling: ReturnToPool, senão Destroy
                var pooled = GetComponent<PooledObject>();
                if (pooled != null)
                {
                    try { pooled.ReturnToPool(); return; }
                    catch { /* fallback */ }
                }
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        Debug.Log($"[PoderPassaroProjectile] OnTriggerEnter2D: {name} hit {other.name} tag={other.tag}");

        // tenta aplicar dano no componente FadaDano (se existir)
        var fada = other.GetComponent<FadaDano>();
        if (fada != null)
        {
            fada.TryTakeDamageFromExternal(damage);
        }

        // destruir/retornar ao pool ao colidir com qualquer coisa (mude lógica se quiser filtrar)
        var pooled = GetComponent<PooledObject>();
        if (pooled != null)
        {
            try { pooled.ReturnToPool(); return; }
            catch { /* fallback */ }
        }

        Destroy(gameObject);
    }
}
