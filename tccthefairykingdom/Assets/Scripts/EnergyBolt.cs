using UnityEngine;

public class EnergyBolt : MonoBehaviour
{
   [Header("Movimento")]
    public float speed = 8f;
    public float lifeTime = 3f;

    [Header("Dano (impacto)")]
    public int damage = 1;

    [Header("DOT (opcional)")]
    public bool applyDOT = false;
    public int dotDamagePerTick = 1;
    public float dotDuration = 3f;
    public float dotTickInterval = 1f;

    Rigidbody2D rb;
    Vector2 direction = Vector2.left;
    ParticleSystem psChild;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        psChild = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        if (psChild != null) psChild.Play();
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
        else
            transform.Translate((Vector3)direction.normalized * speed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Defina a direção logo após instanciar o prefab.
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.001f) direction = dir.normalized;

        // Ajuste a escala X para "virar" o sprite caso necessário
        Vector3 s = transform.localScale;
        s.x = Mathf.Sign(direction.x) * Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se colidiu com o Player (tag "Player")
        if (other.CompareTag("Player"))
        {
            // tenta achar FadaDano no próprio collider ou em um parent
            FadaDano fada = other.GetComponent<FadaDano>();
            if (fada == null) fada = other.GetComponentInParent<FadaDano>();

            if (fada != null)
            {
                // Aplica dano imediato respeitando o intervalo (usa o método centralizado do FadaDano)
                fada.TryTakeDamageFromExternal(damage);

                // Opcional: aplica DOT (damage over time) chamando o método público do FadaDano
                if (applyDOT)
                {
                    fada.ApplyDOT(dotDamagePerTick, dotDuration, dotTickInterval);
                }
            }
            else
            {
                Debug.LogWarning("EnergyBolt: objeto com tag 'Player' não possui FadaDano. Não foi aplicado dano.");
            }

            // efeito de impacto: solta o sistema de partículas (se houver) para que conclua o burst
            if (psChild != null)
            {
                psChild.transform.parent = null; // solta do projétil
                psChild.Stop(true, ParticleSystemStopBehavior.StopEmitting); // para emitir novos, deixa os existentes terminarem
            }

            Destroy(gameObject);
            return;
        }

        // Se quiser que acabe ao colidir com o chão/obstáculo, ajuste pelo layer "Ground"
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0 && other.gameObject.layer == groundLayer)
        {
            // solta particulas (se houver) e destrói
            if (psChild != null)
            {
                psChild.transform.parent = null;
                psChild.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            Destroy(gameObject);
            return;
        }

        // Outros comportamentos de colisão podem ser adicionados aqui (ex: inimigos, paredes, etc.)
    }
}