using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHp = 3;
    public GameObject crystalPrefab;
    public float deathDelay = 0.4f;
    public bool destroyOnDie = true;

    [Header("Quando acertado (piscar/invulnerabilidade)")]
    public float invulnerableDuration = 0.6f;
    public int flashCount = 6; // quantas vezes pisca durante a invul
    public Color flashColor = Color.white; // cor de "hit"

    [Header("Cutscene de vitória (apenas se true)")]
    public bool triggerVictoryCutsceneOnDeath = false; // leave false para inimigas normais

    private int currentHp;
    private bool isDead = false;
    private bool invulnerable = false;

    private Collider2D[] collidersChildren;
    private Rigidbody2D rb;
    private Animator animator;
    private MonoBehaviour[] behavioursToDisable;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    void Awake()
    {
        currentHp = maxHp;
        collidersChildren = GetComponentsInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        behavioursToDisable = GetComponents<MonoBehaviour>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (invulnerable) return;

        currentHp -= amount;

        if (animator != null)
            animator.SetTrigger("Hit");

        if (invulnerableDuration > 0f)
            StartCoroutine(FlashWhenHit());

        if (currentHp <= 0)
            Die();
    }

    IEnumerator FlashWhenHit()
    {
        invulnerable = true;
        float interval = invulnerableDuration / Mathf.Max(1, flashCount);
        for (int i = 0; i < flashCount; i++)
        {
            for (int s = 0; s < spriteRenderers.Length; s++)
            {
                if (spriteRenderers[s] != null)
                    spriteRenderers[s].color = (i % 2 == 0) ? flashColor : originalColors[s];
            }
            yield return new WaitForSeconds(interval);
        }

        for (int s = 0; s < spriteRenderers.Length; s++)
            if (spriteRenderers[s] != null) spriteRenderers[s].color = originalColors[s];

        invulnerable = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null) animator.SetTrigger("Die");

        if (collidersChildren != null)
        {
            foreach (var c in collidersChildren)
                if (c != null) c.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (behavioursToDisable != null)
        {
            foreach (var b in behavioursToDisable)
            {
                if (b == this) continue;
                b.enabled = false;
            }
        }

        if (crystalPrefab != null)
        {
            Instantiate(crystalPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("EnemyHealth: crystalPrefab não atribuído.");
        }

        // ********* Aqui está a correção *********
        // Chama o GameManager correto (versão Simple) usando OnEnemyDeathWithDelay,
        // para que o GameManager trate o delay e mostre a cutscene.
        if (triggerVictoryCutsceneOnDeath)
        {
            // Tenta GameManager_Cutscene_Simple primeiro
            var gmSimple = GameManager_Cutscene_Simple.Instance;
            if (gmSimple != null)
            {
                gmSimple.OnEnemyDeathWithDelay(deathDelay);
            }
            else
            {
                // Se você estiver usando outra classe de GameManager (com nome diferente),
                // você pode adicionar tratamentos aqui, ou só logar o warning.
                Debug.LogWarning("triggerVictoryCutsceneOnDeath está true, mas GameManager_Cutscene_Simple.Instance é null.");
            }
        }

        // destruir ou desativar inimigo (a chamada à cutscene já está sendo tratada pelo GameManager)
        if (destroyOnDie)
            Destroy(gameObject, deathDelay); // mantém small delay para animação
        else
            gameObject.SetActive(false);
    }
}
