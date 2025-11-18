using UnityEngine;
using System.Reflection;

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

        // Caso: atingiu o player (ou objeto com componentes de player)
        if (other.CompareTag("Player"))
        {
            // 1) Tenta FadaDano (preferência)
            var fada = other.GetComponent<FadaDano>();
            if (fada != null)
            {
                // se estiver com escudo, consome escudo e destrói o projétil
                if (fada.IsShielded())
                {
                    fada.ShieldHit();
                    if (destroyOnHit) Destroy(gameObject);
                    return;
                }

                // sem escudo, aplica dano via API existente
                fada.TryTakeDamageFromExternal(damage);
                if (destroyOnHit) Destroy(gameObject);
                return;
            }

            // 2) Fallback: PlayerHealth (pode não existir ou não ter métodos de escudo)
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // usa reflexão para checar IsShielded() (se existir) sem causar erro de compilação
                MethodInfo isShieldMI = ph.GetType().GetMethod("IsShielded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool phIsShielded = false;
                if (isShieldMI != null)
                {
                    try
                    {
                        phIsShielded = (bool)isShieldMI.Invoke(ph, null);
                    }
                    catch
                    {
                        phIsShielded = false;
                    }
                }

                if (phIsShielded)
                {
                    // tenta invocar ShieldHit() via reflexão (se existir)
                    MethodInfo shieldHitMI = ph.GetType().GetMethod("ShieldHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (shieldHitMI != null)
                    {
                        shieldHitMI.Invoke(ph, null);
                    }
                    else
                    {
                        Debug.LogWarning("[EnemyProjectile] PlayerHealth detectado como protegido, mas não possui ShieldHit().");
                    }

                    if (destroyOnHit) Destroy(gameObject);
                    return;
                }

                // se não está protegido, tenta invocar TakeDamage(int) (se existir)
                MethodInfo takeDamageMI = ph.GetType().GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (takeDamageMI != null)
                {
                    // chama TakeDamage(damage)
                    takeDamageMI.Invoke(ph, new object[] { damage });
                }
                else
                {
                    Debug.LogWarning("[EnemyProjectile] PlayerHealth encontrado, mas não possui TakeDamage(int). Nenhuma ação aplicada.");
                }

                if (destroyOnHit) Destroy(gameObject);
                return;
            }

            // 3) Se for Player sem componentes conhecidos, destrói o projétil para evitar passagem
            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        // Se bateu em qualquer outra coisa (cenário, parede, etc.)
        if (destroyOnHit) Destroy(gameObject);
    }
}
