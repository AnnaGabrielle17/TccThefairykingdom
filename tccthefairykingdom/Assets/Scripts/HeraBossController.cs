using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HeraBossController : MonoBehaviour
{
    public enum State { MovingLeft, Hovering }
    public State state = State.MovingLeft;

    [Header("Movement")]
    public float leftSpeed = 3f;         // velocidade enquanto vai para a esquerda
    public float hoverSpeed = 4f;        // velocidade de seguir no eixo Y
    public float stopX = 0f;             // X onde ela para de se mover no eixo X (ajuste no Inspector)
    public bool stopWhenXLessThan = true; // se true: para quando transform.x <= stopX (útil dependendo da sua cena)

    [Header("Combat")]
    public int maxHealth = 500;
    public int contactDamage = 30;       // dano se colidir com o player
    public Transform firePoint;          // ponto de onde sai o projétil
    public GameObject projectilePrefab;  // prefab do poder (imagem que você mostrou)
    public float projectileSpeed = 7f;
    public float fireRate = 1.0f;        // tiros por segundo enquanto está em Hovering

    [Header("References")]
    public Transform player;             // arrastar o player no Inspector

    Rigidbody2D rb;
    int currentHealth;
    float fireCooldown = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        if (player == null) return;

        switch (state)
        {
            case State.MovingLeft:
                MoveLeft();
                CheckStopX();
                break;

            case State.Hovering:
                HoverFollowY();
                HandleShooting();
                break;
        }
    }

    void MoveLeft()
    {
        // movimento constante para a esquerda
        Vector2 pos = rb.position;
        pos.x += -leftSpeed * Time.deltaTime;
        rb.MovePosition(pos);
    }

    void CheckStopX()
    {
        if (stopWhenXLessThan)
        {
            if (transform.position.x <= stopX)
            {
                EnterHover();
            }
        }
        else
        {
            if (transform.position.x >= stopX)
            {
                EnterHover();
            }
        }
    }

    void EnterHover()
    {
        state = State.Hovering;
        // centraliza X para evitar drift. Mantém a X atual.
        rb.position = new Vector2(transform.position.x, transform.position.y);
    }

    void HoverFollowY()
    {
        float targetY = player.position.y;
        float newY = Mathf.MoveTowards(transform.position.y, targetY, hoverSpeed * Time.deltaTime);
        rb.MovePosition(new Vector2(transform.position.x, newY));
    }

    void HandleShooting()
    {
        if (projectilePrefab == null || firePoint == null) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            ShootAtPlayer();
            fireCooldown = 1f / Mathf.Max(0.0001f, fireRate);
        }
    }

    void ShootAtPlayer()
    {
        Vector2 dir = (player.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        // tenta configurar um script no projétil que aceite Init
        var ice = proj.GetComponent<HeraIceProjectile>();
if (ice != null) {
    ice.Init(dir, projectileSpeed);
}
        else
        {
            // fallback: procura Rigidbody2D
            var rbp = proj.GetComponent<Rigidbody2D>();
            if (rbp != null)
            {
                rbp.linearVelocity = dir * projectileSpeed;
            }
        }
    }

    // dano por contato
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var ph = collision.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(contactDamage);
            }
        }
    }

    // chamar para receber dano
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // anim, som, drop, etc
        Destroy(gameObject);
    }

    // visualização no editor: linha vertical do stopX
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(stopX, -50f, 0f), new Vector3(stopX, 50f, 0f));
    }
}