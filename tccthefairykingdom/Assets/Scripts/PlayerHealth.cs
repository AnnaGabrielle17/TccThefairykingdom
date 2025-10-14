using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
     [Header("HP")]
    [SerializeField, Min(1)] private int maxHp = 18;
    [SerializeField] private int currentHp = -1; // -1 indica "ainda não inicializado"

    [Header("UI")]
    [SerializeField] private HealthBarUIByImage healthBar; // arraste o healthbar no Inspector

    private void Awake()
    {
        // se currentHp não foi definido no Inspector, inicializa com max
        if (currentHp < 0) currentHp = maxHp;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
    }

    private void Start()
    {
        UpdateBar();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        currentHp = Mathf.Clamp(currentHp - damage, 0, maxHp);
        UpdateBar();
        if (currentHp == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHp, maxHp);
            // se você usou frames invertidos, troque por:
            // healthBar.SetHealth_InvertedFrames(currentHp, maxHp);
        }
    }

    private void Die()
    {
        Debug.Log("Player morreu!");
    }

    // Teste rápido: pressione K para dano e L para curar
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.L)) Heal(10);
    }
}

