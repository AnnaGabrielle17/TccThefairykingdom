using UnityEngine;
using System.Collections.Generic;

public class WindDamageZone2D : MonoBehaviour
{
    [Header("Dano (modo ativo)")]
    [Tooltip("Quantidade inteira enviada para TryTakeDamageFromExternal(int)")]
    public int damageAmount = 1;

    [Tooltip("Tempo mínimo (s) entre aplicações de dano por cada collider individual")]
    public float damageInterval = 1f;

    [Header("Empurrão (opcional)")]
    public bool usePush = true;
    [Tooltip("Força do empurrão aplicada ao Rigidbody2D do alvo (modo impulso)")]
    public float pushForce = 4f;
    [Tooltip("Direção local do push. Ex: (1,0) empurra para o right local do objeto.")]
    public Vector2 pushDirection = Vector2.right;

    [Header("Compatibilidade / tags")]
    [Tooltip("Se true: não aplica dano diretamente. Em vez disso marca este objeto com a tag 'Mecanica'")]
    public bool relyOnPlayerTrigger = false;
    [Tooltip("Quando relyOnPlayerTrigger = true, qual tag será usada (se quiser outra).")]
    public string mecanicaTag = "Mecanica";

    // controle de cooldown por collider
    private readonly Dictionary<Collider2D, float> lastDamageTimes = new Dictionary<Collider2D, float>();

    private void Reset()
    {
        // garante que o collider seja trigger por padrão
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void Start()
    {
        if (relyOnPlayerTrigger)
        {
            // torna o próprio GameObject marcado como "Mecanica" para que FadaDano detecte automaticamente
            // (atenção: a tag deve existir no projeto)
            if (!string.IsNullOrEmpty(mecanicaTag))
                gameObject.tag = mecanicaTag;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // limpa estado caso precise (mantemos no OnTriggerExit)
        if (lastDamageTimes.ContainsKey(other)) lastDamageTimes.Remove(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (relyOnPlayerTrigger)
        {
            // neste modo o player (FadaDano) já fará o OnTriggerStay e chamará TryTakeDamageFromExternal
            // então não precisamos fazer nada aqui.
            return;
        }

        // aplica dano ativo: procura FadaDano no collider
        if (other == null) return;

        float last;
        lastDamageTimes.TryGetValue(other, out last);

        if (Time.time < last + damageInterval)
            return; // ainda no cooldown para este collider

        // tenta encontrar o componente FadaDano no objeto atingido
        var fada = other.GetComponent<FadaDano>();
        if (fada != null)
        {
            // chama método público do seu script
            fada.TryTakeDamageFromExternal(damageAmount);
            lastDamageTimes[other] = Time.time;
        }
        else
        {
            // fallback: se o objeto não tiver FadaDano, você pode:
            // - checar por uma interface IDamageable (se existir no seu projeto),
            // - ou checar por tag e tratar de forma diferente.
            // Por simplicidade não fazemos nada aqui.
        }

        // aplica empurrão se houver Rigidbody2D
        if (usePush)
        {
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                // pushDirection é local ao WindDamageZone, transformamos pra world
                Vector2 worldPush = (Vector2)transform.TransformDirection(pushDirection.normalized) * pushForce;
                // usar impulso para um empurrão instantâneo (pode ajustar para Force se preferir gradual)
                rb.AddForce(worldPush, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // limpa cooldown do collider que saiu
        if (lastDamageTimes.ContainsKey(other)) lastDamageTimes.Remove(other);
    }

#if UNITY_EDITOR
    // visualização no editor: mostra direção do push
    private void OnDrawGizmosSelected()
    {
        if (usePush)
        {
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Vector3 end = transform.TransformPoint((Vector3)pushDirection.normalized * 0.5f);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.03f);
        }
    }
#endif
}

