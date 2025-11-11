using UnityEngine;

public class DamageCollider : MonoBehaviour
{
   [Header("Configuração de dano")]
    public int damage = 1;

    [Tooltip("Se true: causa dano continuamente (OnTriggerStay2D). Se false: aplica dano uma vez (OnTriggerEnter2D).")]
    public bool continuousDamage = false;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private void Reset()
    {
        // Garante que o collider esteja como trigger (ajuda na configuração)
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (continuousDamage) return; // se estamos em modo contínuo, ignoramos enter

        if (enableDebugLogs)
            Debug.Log($"[DamageCollider] OnTriggerEnter2D com {other.name} (tag: {other.tag})");

        TryApplyDamageToCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!continuousDamage) return; // só processa se modo contínuo

        if (enableDebugLogs)
            Debug.Log($"[DamageCollider] OnTriggerStay2D com {other.name} (tag: {other.tag})");

        TryApplyDamageToCollider(other);
    }

    private void TryApplyDamageToCollider(Collider2D other)
    {
        if (other == null) return;

        // normalmente o jogador tem a tag "Player" — se você usa outra tag, pode remover esta checagem
        // mas mantive para evitar aplicar dano a objetos indesejados.
        // Se preferir, comente a linha abaixo.
        // if (!other.CompareTag("Player")) return;

        // tenta achar o componente FadaDano no próprio collider ou em algum parent (caso o collider esteja em child)
        var fada = other.GetComponent<FadaDano>();
        if (fada == null)
        {
            fada = other.GetComponentInParent<FadaDano>();
        }

        if (fada != null)
        {
            // Usamos TryTakeDamageFromExternal que respeita intervaloDano do FadaDano
            fada.TryTakeDamageFromExternal(damage);

            if (enableDebugLogs)
                Debug.Log($"[DamageCollider] Aplicou {damage} de dano usando FadaDano em {other.name}");
            return;
        }

        // fallback: envia mensagem para cima (caso você tenha outro nome de script)
        other.SendMessageUpwards("TryTakeDamageFromExternal", damage, SendMessageOptions.DontRequireReceiver);

        if (enableDebugLogs)
            Debug.Log($"[DamageCollider] FadaDano não encontrado. Enviado SendMessageUpwards TryTakeDamageFromExternal para {other.name}");
    }
}
