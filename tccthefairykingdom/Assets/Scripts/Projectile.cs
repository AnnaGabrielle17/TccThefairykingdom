using UnityEngine;

public class Projectile : MonoBehaviour
{
  public float speed = 8f;
    public float lifeTime = 3f;
    public int damage = 1;

    // Defina a layer dos inimigos via Inspector (por exemplo: "Enemy")
    public LayerMask enemyLayer;

    int direction = 1;
    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, 0f);

        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(int dir)
    {
        direction = dir >= 0 ? 1 : -1;
        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, 0f);

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora colisões com jogador
        if (other.CompareTag("Player")) return;

        // 1) Filtrar por LayerMask: somente prosseguir se 'other' estiver na layer de inimigo
        if (((1 << other.gameObject.layer) & enemyLayer.value) != 0)
        {
            // tenta pegar EnemyHealth (mais seguro que apenas CompareTag)
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damage);
            }
            else
            {
                // se não tem EnemyHealth, mas é a layer de inimigo, você pode considerar percorrer parents:
                var parentEh = other.GetComponentInParent<EnemyHealth>();
                if (parentEh != null) parentEh.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // 2) Se não for inimigo (por exemplo projétil inimigo), apenas ignoramos/desconsideramos.
        // (Se chegou aqui, não é enemy — evitar destruir projéteis inimigos)
        // Você pode opcionalmente destruir o projétil ao bater em paredes:
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
        // Caso sua parede/use triggers diferentes, ajuste a lógica acima conforme necessário.
    }
}