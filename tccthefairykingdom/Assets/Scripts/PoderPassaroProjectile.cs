using UnityEngine;

public class PoderPassaroProjectile : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6f;
    public Vector2 direction = Vector2.left; // por padrão vai para a esquerda
    public float lifeTime = 6f;

    [Header("Dano")]
    public int damage = 1;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // configuração segura se foi adicionado via código
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    // permite ajustar a direção/velocidade logo após instanciar
    public void Initialize(Vector2 dir, float speedOverride = -1f, int damageOverride = -1)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed;
        if (speedOverride > 0) { speed = speedOverride; rb.linearVelocity = direction * speed; }
        if (damageOverride >= 0) damage = damageOverride;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // tenta aplicar dano no componente FadaDano (se existir)
        var fada = other.GetComponent<FadaDano>();
        if (fada != null)
        {
            // Use o método público que já lida com intervalos e efeitos visuais
            fada.TryTakeDamageFromExternal(damage);

            // se quiser aplicar DOT em vez de dano imediato:
            // fada.ApplyDOT(1, 3f, 1f); // exemplo
        }

        // opcional: evitar destruir se colidir com objetos que não devem destruir
        // se quiser só atravessar plataforma, checar tag/layer aqui

        // destruir projétil ao colidir com qualquer coisa (ou filtrar por tags)
        Destroy(gameObject);
    }

}
