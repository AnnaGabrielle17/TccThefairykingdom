using UnityEngine;

public class Projectile : MonoBehaviour
{
  public float speed = 8f;
    public float lifeTime = 3f;
    public int damage = 1;

    [Tooltip("Marque a layer 'Enemy' aqui no Inspector (LayerMask)")]
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

        if (other == null) return;

        // checa por tag e destrói o pássaro
        if (other.CompareTag("Passaro"))
        {
            Destroy(other.gameObject); // destrói o pássaro inteiro
            Destroy(gameObject);       // destrói o projétil
            return;
        }
        if (other.CompareTag("Player")) return;

        // Verifica se 'other' está na layer enemy (se você estiver usando layers)
        bool isEnemyLayer = ((1 << other.gameObject.layer) & enemyLayer.value) != 0;

        // Se for inimigo (pela layer), tenta aplicar dano
        if (isEnemyLayer)
        {
            // procura EnemyHealth no collider ou em um pai (caso o collider esteja em child)
            EnemyHealth eh = other.GetComponent<EnemyHealth>() ?? other.GetComponentInParent<EnemyHealth>();

            Debug.Log($"Projectile hit '{other.name}'. EnemyHealth found? { (eh != null) }");

            if (eh != null)
            {
                eh.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.LogWarning($"Projectile: layer é Enemy, mas não encontrou EnemyHealth em '{other.name}' ou pais.");
                Destroy(gameObject);
                return;
            }
        }

        // Caso não seja inimigo:
        // - se bateu em parede (não trigger), destrói; senão (ex.: projétil inimigo) ignora
        if (!other.isTrigger) Destroy(gameObject);
    }
}