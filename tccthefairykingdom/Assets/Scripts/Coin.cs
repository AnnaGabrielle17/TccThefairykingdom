using UnityEngine;

public class Coin : MonoBehaviour
{
    [Tooltip("Valor desta moeda (padrão 1)")]
    public int value = 1;

    [Tooltip("Som opcional ao coletar")]
    public AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Chama o gerenciador de moedas
            CoinManager.Instance.AddCoins(value);

            // Toca som se tiver
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position);

            Destroy(gameObject);
        }
    }
}
