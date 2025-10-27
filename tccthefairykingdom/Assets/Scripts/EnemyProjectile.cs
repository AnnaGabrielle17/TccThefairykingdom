using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 1;

    private Vector2 direction;
    private float speed = 8f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    // inicializador opcional chamado pelo EnemyFairy se não usar Rigidbody direto
    public void Init(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>(); // substitua pelo seu componente de vida
            if (ph != null) ph.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // opcional: destruir em paredes
        // if (other.CompareTag("Obstacle")) Destroy(gameObject);
    }
}