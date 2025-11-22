using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class EnemyVitals : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 3f;
    public float currentHealth;

    [Header("Death & VFX")]
    public GameObject deathVfx; // opcional
    public float destroyDelay = 0f; // aguarda animação

    [Header("Optional")]
    public Animator animator;
    public string deathTriggerName = "Die";
    public string hurtTriggerName = "Hurt";

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Métodos que o Projectile pode chamar (float, int, double)
    public void TakeDamage(float amount)
    {
        InternalTakeDamage(amount);
    }

    public void TakeDamage(int amount)
    {
        InternalTakeDamage(amount);
    }

    public void TakeDamage(double amount)
    {
        InternalTakeDamage((float)amount);
    }

    void InternalTakeDamage(float amount)
    {
        if (amount <= 0f) return;
        currentHealth -= amount;

        if (animator != null && !string.IsNullOrEmpty(hurtTriggerName))
            animator.SetTrigger(hurtTriggerName);

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        // evita double-death
        if (!enabled) return;
        enabled = false;

        if (animator != null && !string.IsNullOrEmpty(deathTriggerName))
            animator.SetTrigger(deathTriggerName);

        if (deathVfx != null)
        {
            try { Instantiate(deathVfx, transform.position, Quaternion.identity); } catch { }
        }

        if (destroyDelay > 0f)
        {
            StartCoroutine(DelayedDestroy(destroyDelay));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DelayedDestroy(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }
}
