using UnityEngine;

public class ToxicBubble : MonoBehaviour
{
    [Header("Propriedades")]
    public float lifetime = 4f;
    public int hitDamage = 1;            // dano instantâneo ao tocar
    public bool destroyOnHit = true;
    public bool popVfx = true;          // coloque um prefab de efeitos se quiser
    public GameObject popEffectPrefab;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // tenta achar o script FadaDano no collider atingido
        if (other.TryGetComponent<FadaDano>(out var fada))
        {
            // usa TryTakeDamageFromExternal para respeitar o intervaloDano do jogador
            fada.TryTakeDamageFromExternal(hitDamage);

            if (popVfx && popEffectPrefab != null)
                Instantiate(popEffectPrefab, transform.position, Quaternion.identity);

            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        // opcional: destruir ao colidir com ambiente (por camada)
        int envLayer = LayerMask.NameToLayer("Environment"); // ajuste o nome da layer se diferente
        if (other.gameObject.layer == envLayer)
        {
            if (popVfx && popEffectPrefab != null)
                Instantiate(popEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

}
