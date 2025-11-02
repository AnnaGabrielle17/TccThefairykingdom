using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuração")]
    public float speed = 12f;
    public int damage = 1;
    public float lifeTime = 4f;
    public bool destroyOnHit = true;

    private Vector2 moveDirection = Vector2.right;
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true; // projetil usa trigger para "visual hit"
    }

    // inicializa o projétil (chamado pela fada ao instanciar)
    public void Init(Vector2 dir, float speedValue, float lifeTimeValue, int damageValue)
    {
        moveDirection = dir.normalized;
        speed = speedValue;
        lifeTime = lifeTimeValue;
        damage = damageValue;

        // orientar visualmente (transform.right aponta para a direção do movimento)
        if (moveDirection != Vector2.zero)
            transform.right = moveDirection;

        // set velocity
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        // fallback: destruir após lifeTime
        if (Application.isPlaying) Destroy(gameObject, lifeTime);
        else DestroyImmediate(gameObject);
    }

    // caso Init não seja chamada por algum motivo, garantimos movimento padrão
    void Start()
    {
        if (rb != null && rb.linearVelocity == Vector2.zero)
            rb.linearVelocity = moveDirection * speed;

        if (!Mathf.Approximately(lifeTime, 0f))
            Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // ignora colisões com inimigos (opcional: usar layer mask mais robusta)
        if (other.CompareTag("Enemy")) return;

        // se atingir o player, tenta aplicar dano via FadaDano (mantém intervalos/piscar)
        if (other.CompareTag("Player"))
        {
            var fada = other.GetComponent<FadaDano>();
            if (fada != null)
            {
                // usa método existente que controla intervalo/piscar
                fada.TryTakeDamageFromExternal(damage);
            }
            else
            {
                // fallback: se você tiver outro componente de vida, tente chamá-lo
                var ph = other.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage);
            }

            // opcional: spawn de efeito visual de impacto (sprite / som) aqui

            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        // se colidiu com cenário/obstáculo, destrói também
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}