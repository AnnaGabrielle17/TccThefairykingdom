using UnityEngine;
using System;
using System.Reflection;

public class ShieldCollectible : MonoBehaviour
{
    [Tooltip("Duração do escudo em segundos. 0 = nenhuma expiração por tempo (use hits).")]
    public float shieldDuration = 5f;

    [Tooltip("Quantos hits o escudo aguenta. Use int.MaxValue para ignorar hits (ou 0 para infinito).")]
    public int shieldHits = int.MaxValue;

    [Tooltip("Efeito opcional ao pegar (partícula, som etc).")]
    public GameObject pickupEffectPrefab;

    [Header("Áudio")]
    [Tooltip("Som tocado quando o jogador pega a coletável.")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    [Tooltip("Volume do som de pickup.")]
    public float pickupSoundVolume = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        bool applied = false;

        // 1) Preferência: FadaDano (se existir no objeto colidido)
        var fada = other.GetComponent<FadaDano>();
        if (fada != null)
        {
            fada.AddShield(shieldDuration, shieldHits);
            applied = true;
        }
        else
        {
            // 2) Fallback: PlayerHealth (pode não existir; usamos reflexão para invocar AddShield se houver)
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                applied = TryInvokeAddShieldViaReflection(ph, shieldDuration, shieldHits);
            }
            else
            {
                // 3) Por segurança, procurar nos pais também
                var fadaPai = other.GetComponentInParent<FadaDano>();
                if (fadaPai != null)
                {
                    fadaPai.AddShield(shieldDuration, shieldHits);
                    applied = true;
                }
                else
                {
                    var phPai = other.GetComponentInParent<PlayerHealth>();
                    if (phPai != null)
                    {
                        applied = TryInvokeAddShieldViaReflection(phPai, shieldDuration, shieldHits);
                    }
                }
            }
        }

        if (!applied)
        {
            Debug.LogWarning("[ShieldCollectible] Não encontrou FadaDano nem PlayerHealth com método compatível AddShield no objeto 'Player'.");
        }

        // Efeito visual
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // Tocar som: prefere AudioSource no Player, senão PlayClipAtPoint
        if (pickupSound != null)
        {
            AudioSource playerAudio = other.GetComponent<AudioSource>() ?? other.GetComponentInParent<AudioSource>();
            if (playerAudio != null)
            {
                playerAudio.PlayOneShot(pickupSound, pickupSoundVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
            }
        }

        Destroy(gameObject);
    }

    // Tenta várias assinaturas comuns de AddShield via reflexão.
    private bool TryInvokeAddShieldViaReflection(object target, float duration, int hits)
    {
        if (target == null) return false;

        Type t = target.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        try
        {
            // 1) AddShield(float, int)
            MethodInfo mi = t.GetMethod("AddShield", flags, null, new Type[] { typeof(float), typeof(int) }, null);
            if (mi != null)
            {
                mi.Invoke(target, new object[] { duration, hits });
                return true;
            }

            // 2) AddShield(float)
            mi = t.GetMethod("AddShield", flags, null, new Type[] { typeof(float) }, null);
            if (mi != null)
            {
                mi.Invoke(target, new object[] { duration });
                return true;
            }

            // 3) AddShield(int)
            mi = t.GetMethod("AddShield", flags, null, new Type[] { typeof(int) }, null);
            if (mi != null)
            {
                mi.Invoke(target, new object[] { hits });
                return true;
            }

            // 4) AddShield() sem parâmetros
            mi = t.GetMethod("AddShield", flags, null, Type.EmptyTypes, null);
            if (mi != null)
            {
                mi.Invoke(target, null);
                return true;
            }

            // 5) Procurar qualquer método nomeado 'AddShield' (último recurso) e tentar invocar com até 2 args convertidos
            mi = t.GetMethod("AddShield", flags);
            if (mi != null)
            {
                var parameters = mi.GetParameters();
                object[] args = null;
                if (parameters.Length == 2)
                    args = new object[] { Convert.ChangeType(duration, parameters[0].ParameterType), Convert.ChangeType(hits, parameters[1].ParameterType) };
                else if (parameters.Length == 1)
                {
                    if (parameters[0].ParameterType == typeof(float) || parameters[0].ParameterType == typeof(double))
                        args = new object[] { Convert.ChangeType(duration, parameters[0].ParameterType) };
                    else
                        args = new object[] { Convert.ChangeType(hits, parameters[0].ParameterType) };
                }
                else
                    args = new object[] { };

                mi.Invoke(target, args);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ShieldCollectible] Falha ao invocar AddShield via reflexão: " + ex.Message);
            return false;
        }

        return false;
    }
}
