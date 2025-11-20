using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ToxicBubble : MonoBehaviour
{
    [Header("Lifetime & visual")]
    public float lifetime = 3.0f;
    [Tooltip("Alpha inicial")]
    public float maxAlpha = 0.85f;

    [Header("Força de movimento (FORÇA a direção)")]
    [Tooltip("Velocidade Y aplicada continuamente; negativo = desce")]
    public float forcedDownSpeed = 1.0f;   // valor positivo aqui; será aplicado como -forcedDownSpeed
    [Tooltip("Velocidade horizontal (drift). Pode ser negativo ou positivo pequeno.")]
    public float forcedDriftX = 0.05f;
    [Tooltip("Se true, sobrescreve constante velocidade (maior certeza); se false, aplica suavemente.")]
    public bool hardOverwriteVelocity = true;

    [Header("Dano")]
    public int hitDamage = 1;
    public bool destroyOnHit = true;

    Rigidbody2D rb;
    SpriteRenderer sr;
    float birthTime;
    float seed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        seed = Random.value * 10f;
    }

    void Start()
    {
        birthTime = Time.time;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = maxAlpha;
            sr.color = c;
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // schedule destroy
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // ALWAYS ensure Y is negative (descer)
        float wantY = -Mathf.Abs(forcedDownSpeed);

        // small wobble for X to be more natural (adds per-instance noise)
        float wobble = (Mathf.PerlinNoise(Time.time * 0.9f, seed) - 0.5f) * 2f * forcedDriftX;

        if (rb != null)
        {
            if (hardOverwriteVelocity)
            {
                rb.linearVelocity = new Vector2(wobble, wantY);
            }
            else
            {
                // suaviza a transição para a velocidade desejada
                Vector2 v = rb.linearVelocity;
                v.x = Mathf.Lerp(v.x, wobble, 5f * Time.fixedDeltaTime);
                v.y = Mathf.Lerp(v.y, wantY, 6f * Time.fixedDeltaTime);
                rb.linearVelocity = v;
            }
        }
        else
        {
            // fallback: mover por transform caso não haja Rigidbody2D
            transform.position += new Vector3(wobble, wantY, 0f) * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<FadaDano>(out var fada))
        {
            fada.TryTakeDamageFromExternal(hitDamage);
            if (destroyOnHit) Destroy(gameObject);
        }

        int envLayer = LayerMask.NameToLayer("Environment");
        if (other.gameObject.layer == envLayer) Destroy(gameObject);
    }
}
