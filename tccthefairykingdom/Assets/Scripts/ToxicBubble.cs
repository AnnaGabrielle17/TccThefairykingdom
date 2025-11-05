using UnityEngine;

public class ToxicBubble : MonoBehaviour
{
    [Header("Dano")]
    public int hitDamage = 1;
    public bool destroyOnHit = false; // se true, bolha some ao acertar; se false, orbita sempre
    public bool applyDOTInstead = false;
    public int dotDamage = 1;
    public float dotDuration = 3f;
    public float dotTick = 1f;

    BubbleOrbiter orbiter;

    void Awake()
    {
        orbiter = GetComponent<BubbleOrbiter>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<FadaDano>(out var fada))
        {
            if (applyDOTInstead)
            {
                // tenta aplicar DOT (recomendo ter ApplyDOT no seu FadaDano, veja acima)
                var method = fada.GetType().GetMethod("ApplyDOT");
                if (method != null)
                    method.Invoke(fada, new object[] { dotDamage, dotDuration, dotTick });
                else
                    fada.TomarDano(dotDamage); // fallback aplica só um tick
            }
            else
            {
                // usa TryTakeDamageFromExternal para respeitar intervalos
                fada.TryTakeDamageFromExternal(hitDamage);
            }

            if (destroyOnHit) Destroy(gameObject);
        }

        // colisão com environment -> destruir (opcional)
        int envLayer = LayerMask.NameToLayer("Environment");
        if (other.gameObject.layer == envLayer)
        {
            Destroy(gameObject);
        }
    }
}