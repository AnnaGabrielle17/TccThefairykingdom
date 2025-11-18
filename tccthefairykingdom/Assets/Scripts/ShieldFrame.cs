using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(Collider2D))]
public class ShieldFrame : MonoBehaviour
{
    [Tooltip("Referência ao FadaDano no pai (ou outro objeto). Se vazio, tentará GetComponentInParent.")]
    public FadaDano fadaDano;

    [Tooltip("Fallback: referência a PlayerHealth caso esteja usando esse script em vez de FadaDano.")]
    public PlayerHealth playerHealth;

    [Tooltip("Prefab opcional de efeito quando o escudo bloqueia um projétil.")]
    public GameObject blockEffectPrefab;

    void Awake()
    {
        // tenta preencher automaticamente se o usuário não arrastou a referência
        if (fadaDano == null)
        {
            fadaDano = GetComponentInParent<FadaDano>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        // garante que o collider seja trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 1) Detecção por componente (mais robusta)
        EnemyProjectile proj = other.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            // destruir projétil
            Destroy(proj.gameObject);

            // notificar o componente correto (FadaDano tem prioridade)
            if (fadaDano != null)
            {
                fadaDano.ShieldHit();
            }
            else if (playerHealth != null)
            {
                // Usa reflexão para invocar ShieldHit() somente se existir (evita erro de compilação
                // caso PlayerHealth não declare esse método).
                MethodInfo mi = playerHealth.GetType().GetMethod("ShieldHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null)
                {
                    mi.Invoke(playerHealth, null);
                }
                else
                {
                    Debug.LogWarning("[ShieldFrame] PlayerHealth não possui método ShieldHit(). Nada foi notificado. Se quiser comportamento de escudo para PlayerHealth, adicione um método ShieldHit() ou use FadaDano.");
                }
            }
            else
            {
                // tentativa extra: buscar dinamicamente nos pais
                var fd = GetComponentInParent<FadaDano>();
                if (fd != null) fd.ShieldHit();
                else
                {
                    var ph = GetComponentInParent<PlayerHealth>();
                    if (ph != null)
                    {
                        MethodInfo mi2 = ph.GetType().GetMethod("ShieldHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (mi2 != null) mi2.Invoke(ph, null);
                        else Debug.LogWarning("[ShieldFrame] Encontrou PlayerHealth nos pais, mas não há ShieldHit().");
                    }
                }
            }

            // spawn de efeito opcional
            if (blockEffectPrefab != null)
            {
                Instantiate(blockEffectPrefab, transform.position, Quaternion.identity);
            }

            return;
        }

        // 2) Detecção por tag (caso projéteis usem tag em vez de componente)
        if (other.CompareTag("EnemyProjectile"))
        {
            Destroy(other.gameObject);

            if (fadaDano != null)
            {
                fadaDano.ShieldHit();
            }
            else if (playerHealth != null)
            {
                MethodInfo mi = playerHealth.GetType().GetMethod("ShieldHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null) mi.Invoke(playerHealth, null);
                else Debug.LogWarning("[ShieldFrame] PlayerHealth não possui método ShieldHit().");
            }
            else
            {
                var fd = GetComponentInParent<FadaDano>();
                if (fd != null) fd.ShieldHit();
                else
                {
                    var ph = GetComponentInParent<PlayerHealth>();
                    if (ph != null)
                    {
                        MethodInfo mi2 = ph.GetType().GetMethod("ShieldHit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (mi2 != null) mi2.Invoke(ph, null);
                        else Debug.LogWarning("[ShieldFrame] Encontrou PlayerHealth nos pais, mas não há ShieldHit().");
                    }
                }
            }

            if (blockEffectPrefab != null)
            {
                Instantiate(blockEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
