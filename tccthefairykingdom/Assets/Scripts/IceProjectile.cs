using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class IceProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 8f;
    public float lifeTime = 3f;

    [Header("Dano")]
    public int damage = 1;

    [Tooltip("Se quiser que o projétil também acerte inimigos por layer")]
    public LayerMask enemyLayer;

    [Header("Opções")]
    public bool rotateToVelocity = true;            // rotaciona o sprite para a direção do movimento
    public bool destroyOnNonTriggerCollision = true; // destrói ao colidir com colisor não-trigger (paredes)

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

    public void Launch(Vector2 directionVector)
    {
        if (rb == null) return;
        Vector2 dir = directionVector.normalized;
        rb.linearVelocity = dir * speed;

        if (Mathf.Abs(dir.x) > 0.001f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir.x > 0 ? 1f : -1f);
            transform.localScale = s;
        }

        if (rotateToVelocity)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // Compatibilidade: disparo horizontal simples
    public void SetDirection(int dir)
    {
        Launch(new Vector2(dir >= 0 ? 1f : -1f, 0f));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 1) Prioridade: acertar o player que usa FadaDano (se existir)
        FadaDano fadaDano = other.GetComponent<FadaDano>();
        if (fadaDano == null) fadaDano = other.GetComponentInParent<FadaDano>();
        if (fadaDano != null)
        {
            fadaDano.TryTakeDamageFromExternal(damage);
            Destroy(gameObject);
            return;
        }

        // 2) Se for o Player: tenta SendMessage para métodos comuns (TomarDano / TakeDamage)
        if (other.CompareTag("Player"))
        {
            // tenta chamar métodos comuns sem depender de tipos concretos
            // primeiro tenta "TomarDano" (nome PT-BR usado no seu projeto)
            other.SendMessage("TomarDano", damage, SendMessageOptions.DontRequireReceiver);
            // também tenta "TryTakeDamageFromExternal" caso algum outro script implemente
            other.SendMessage("TryTakeDamageFromExternal", damage, SendMessageOptions.DontRequireReceiver);
            // e "TakeDamage" por compatibilidade com projetos em inglês
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            Destroy(gameObject);
            return;
        }

        // 3) Acertar inimigos por layer (se configurado) — usa SendMessage para compatibilidade
        bool isEnemyLayer = ((1 << other.gameObject.layer) & enemyLayer.value) != 0;
        if (isEnemyLayer)
        {
            // tenta alguns métodos comuns (TakeDamage, TomarDano, Damage)
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            other.SendMessage("TomarDano", damage, SendMessageOptions.DontRequireReceiver);
            other.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);

            Destroy(gameObject);
            return;
        }

        // 4) Caso não seja nada dos acima: se colidiu com um colisor "solido" (não trigger), destrói
        if (!other.isTrigger && destroyOnNonTriggerCollision)
        {
            Destroy(gameObject);
        }
    }
}
