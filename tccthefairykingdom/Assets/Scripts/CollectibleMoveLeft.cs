using UnityEngine;

public class CollectibleMoveLeft : MonoBehaviour
{
    public float speed = 2f;

    // Arraste aqui o som do coletável no Inspector
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombat pc = collision.GetComponent<PlayerCombat>();

            if (pc != null)
            {
                pc.AddOrRefreshPower();
            }

            // Toca o efeito sonoro (se atribuído) na posição do coletável
            if (pickupSfx != null)
            {
                AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);
            }

            Destroy(gameObject);
        }
    }
}
