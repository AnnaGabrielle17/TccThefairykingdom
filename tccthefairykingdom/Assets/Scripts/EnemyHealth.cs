using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
     public int maxHp = 3;
    public GameObject crystalPrefab;      // prefab do cristal que será instanciado quando morrer
    public float deathDelay = 0.4f;       // tempo até destruir o inimigo (para animação)
    public bool destroyOnDie = true;

    private int currentHp;
    private bool isDead = false;

    // caches
    private Collider2D[] collidersChildren;
    private Rigidbody2D rb;
    private Animator animator;
    private MonoBehaviour[] behavioursToDisable; // por exemplo: EnemyFairy

    void Awake()
    {
        currentHp = maxHp;
        collidersChildren = GetComponentsInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // opcional: desabilitar scripts que devam parar ao morrer
        // aqui pegamos todos os MonoBehaviour neste GameObject (por segurança)
        behavioursToDisable = GetComponents<MonoBehaviour>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp -= amount;

        // hit animation opcional
        if (animator != null) animator.SetTrigger("Hit");

        if (currentHp <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // play death animation (se houver)
        if (animator != null) animator.SetTrigger("Die");

        // desativa coliders para evitar novas colisões
        if (collidersChildren != null)
        {
            foreach (var c in collidersChildren)
            {
                if (c != null) c.enabled = false;
            }
        }

        // para o rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // desabilita scripts que não fazem sentido rodar quando morto (ex: EnemyFairy)
        if (behavioursToDisable != null)
        {
            foreach (var b in behavioursToDisable)
            {
                if (b == this) continue; // não desativa esse script agora
                // você pode filtrar por nome do script se quiser
                b.enabled = false;
            }
        }

        // instancia o cristal (se atribuído)
        if (crystalPrefab != null)
        {
            Instantiate(crystalPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("EnemyHealth: crystalPrefab não atribuído no Inspector.");
        }

        // destrói o inimigo depois do delay (ou desativa)
        if (destroyOnDie)
            Destroy(gameObject, deathDelay);
        else
            gameObject.SetActive(false);
    }
}