using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HeraBossController : MonoBehaviour
{
    public enum State { MovingLeft, Hovering }
    public State state = State.MovingLeft;

    [Header("Movement")]
    public float leftSpeed = 3f;
    public float hoverSpeed = 4f;
    public float stopX = 0f;
    public bool stopWhenXLessThan = true;

    [Header("Combat")]
    public int maxHealth = 500;
    public int contactDamage = 30;
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float projectileSpeed = 7f;
    public float fireRate = 1.0f;

    [Header("References")]
    public Transform player;

    [Header("Cutscene de vitória (boss)")]
    public bool triggerVictoryCutsceneOnDeath = true;
    public float bossDeathDelay = 0.6f; // tempo antes de chamar a cutscene (sincronizar com animação)

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
        var ice = proj.GetComponent<HeraIceProjectile>();
        if (ice != null)
        {
            ice.Init(dir, projectileSpeed);
        }
        else
        {
            var rbp = proj.GetComponent<Rigidbody2D>();
            if (rbp != null)
            {
                rbp.linearVelocity = dir * projectileSpeed;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // animação/som/drops aqui se quiser

        if (triggerVictoryCutsceneOnDeath)
        {
            // Primeiro: tenta usar o GameManager_Cutscene_Simple (se você está usando essa versão)
            var gmSimple = GameManager_Cutscene_Simple.Instance;
            if (gmSimple != null)
            {
                gmSimple.OnEnemyDeathWithDelay(bossDeathDelay);
            }
            else
            {
                // Se não encontrou, tenta achar um GameObject chamado "GameManager"
                // e enviar uma mensagem "OnEnemyDeathWithDelay" (não falhará se não existir).
                GameObject gmObj = GameObject.Find("GameManager");
                if (gmObj != null)
                {
                    // SendMessage tenta chamar qualquer método público com esse nome no GameManager.
                    gmObj.SendMessage("OnEnemyDeathWithDelay", bossDeathDelay, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.LogWarning("triggerVictoryCutsceneOnDeath está true, mas não encontrei GameManager_Cutscene_Simple nem GameObject 'GameManager' para enviar a mensagem.");
                }
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(stopX, -50f, 0f), new Vector3(stopX, 50f, 0f));
    }
}
