using UnityEngine;

public class PoderPassaroProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6f;
    public Vector2 direction = Vector2.left; // por padrão vai para a esquerda
    public float lifeTime = 6f;

    [Header("Dano")]
    public int damage = 1;

    Rigidbody2D rb;

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

    private void Start()
    {
        rb.linearVelocity = direction.normalized * speed;
        Debug.Log($"[PoderPassaroProjectile] Start: {name} pos={transform.position} vel={rb.linearVelocity}");
        Destroy(gameObject, lifeTime);
    }

    // permite ajustar a direção/velocidade logo após instanciar
    public void Initialize(Vector2 dir, float speedOverride = -1f, int damageOverride = -1)
    {
        direction = dir.normalized;
        if (speedOverride > 0) speed = speedOverride;
        rb.linearVelocity = direction * speed;
        if (damageOverride >= 0) damage = damageOverride;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[PoderPassaroProjectile] OnTriggerEnter2D: {name} hit {other.name} tag={other.tag}");

        if (other == null) return;

        // tenta aplicar dano no componente FadaDano (se existir)
        var fada = other.GetComponent<FadaDano>();
        if (fada != null)
        {
            fada.TryTakeDamageFromExternal(damage);
        }

        // destruir projétil ao colidir com qualquer coisa (se quiser filtrar, mude aqui)
        Destroy(gameObject);
    }
}
