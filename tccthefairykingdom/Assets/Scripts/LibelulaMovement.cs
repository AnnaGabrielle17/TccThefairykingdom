using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LibelulaMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;               // velocidade para a esquerda
    public bool useFixedUpdate = true;     // mover em FixedUpdate (recomendado)

    [Header("Sprite Orientation")]
    [Tooltip("Marque true se o desenho da sprite já estiver apontando PARA A ESQUERDA.")]
    public bool spriteArtFacesLeft = true;
    [Tooltip("Se true, usa SpriteRenderer.flipX. Se false, usa transform.localScale.x.")]
    public bool useSpriteRendererFlip = true;

    [Header("Health")]
    public float maxHealth = 3f;
    public float currentHealth;

    [Header("Death & VFX")]
    public GameObject deathVfx;            
    public float destroyDelay = 0f;        

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // Ajusta a orientação da sprite para que fique apontando PARA A ESQUERDA
        // (já que a libélula voa sempre para a esquerda)
        EnsureFacingLeft();
    }

    void Start()
    {
        // nada a mais necessário aqui, mas mantive separado caso queira alterar em runtime
    }

    void FixedUpdate()
    {
        if (useFixedUpdate) MoveLeft();
    }

    void Update()
    {
        if (!useFixedUpdate) MoveLeft();
    }

    void MoveLeft()
    {
        // Mantém velocidade horizontal constante para a esquerda
        Vector2 vel = rb.linearVelocity;
        vel.x = -Mathf.Abs(speed);
        rb.linearVelocity = vel;
    }

    void EnsureFacingLeft()
    {
        bool desiredFacingLeft = true;

        if (useSpriteRendererFlip && sr != null)
        {
            // flipX = true inverte horizontalmente; queremos flipX = false se a arte já olha para a esquerda
            sr.flipX = !desiredFacingLeft && spriteArtFacesLeft ? false : !spriteArtFacesLeft;
            // Simplifica: se spriteArtFacesLeft==true => flipX = false
            sr.flipX = !spriteArtFacesLeft;
        }
        else
        {
            Vector3 s = transform.localScale;
            // Se a arte aponta para a esquerda, deixamos localScale.x positivo para que ela mostre p/ esquerda.
            // Caso a arte apontasse para a direita (spriteArtFacesLeft = false), colocamos localScale.x negativo.
            s.x = Mathf.Abs(s.x) * (spriteArtFacesLeft ? 1f : -1f);
            transform.localScale = s;
        }
    }

    // --- Métodos TakeDamage para compatibilidade com Projectile (float/int/double) ---
    public void TakeDamage(float amount)
    {
        InternalTakeDamage(amount);
    }

    public void TakeDamage(int amount)
    {
        InternalTakeDamage((float)amount);
    }

    public void TakeDamage(double amount)
    {
        InternalTakeDamage((float)amount);
    }

    void InternalTakeDamage(float amount)
    {
        if (amount <= 0f) return;
        currentHealth -= amount;
        // opcional: Debug.Log($"Libelula recebeu {amount} dano. HP restante: {currentHealth}");

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (deathVfx != null)
        {
            try { Instantiate(deathVfx, transform.position, Quaternion.identity); } catch { }
        }

        if (destroyDelay > 0f) Destroy(gameObject, destroyDelay);
        else Destroy(gameObject);
    }
}
