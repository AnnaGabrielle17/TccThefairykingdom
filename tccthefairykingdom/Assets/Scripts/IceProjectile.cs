using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 8f;
    public float lifeTime = 3f;

    [Header("Dano")]
    public int damage = 1;

    [Tooltip("Marque a layer 'Enemy' aqui no Inspector (LayerMask)")]
    public LayerMask enemyLayer;

    [Header("Opções")]
    public bool rotateToVelocity = true; // rotaciona o sprite na direção do movimento

    int direction = 1; // 1 = direita, -1 = esquerda
    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Disparo horizontal compatível com implementações antigas
    public void SetDirection(int dir)
    {
        direction = dir >= 0 ? 1 : -1;
        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, 0f);

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;

        if (rotateToVelocity)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // Disparo com direção 2D (usado pelo FadinhaController)
    public void Launch(Vector2 directionVector)
    {
        if (rb == null) return;
        Vector2 dir = directionVector.normalized;
        rb.linearVelocity = dir * speed;

        if (dir.x != 0f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir.x > 0 ? 1 : -1);
            transform.localScale = s;
        }

        if (rotateToVelocity)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // checa por tag "Passaro"
        if (other.CompareTag("Passaro"))
        {
            Destroy(other.gameObject); // destrói o pássaro
            Destroy(gameObject);
            return;
        }

        // ignora jogador (ajuste conforme design)
        if (other.CompareTag("Player")) return;

        // Verifica se 'other' está na layer enemy (quando você usa LayerMask)
        bool isEnemyLayer = ((1 << other.gameObject.layer) & enemyLayer.value) != 0;

        if (isEnemyLayer)
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>() ?? other.GetComponentInParent<EnemyHealth>();

            Debug.Log($"IceProjectile hit '{other.name}'. EnemyHealth found? { (eh != null) }");

            if (eh != null)
            {
                eh.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.LogWarning($"IceProjectile: layer é Enemy, mas não encontrou EnemyHealth em '{other.name}' ou pais.");
                Destroy(gameObject);
                return;
            }
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }

}
