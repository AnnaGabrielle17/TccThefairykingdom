using UnityEngine;

public class FadaDano : MonoBehaviour
{
    public int vida = 10;
    public float intervaloDano = 1f; // 1 segundo entre cada dano

    private float tempoUltimoDano;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Mecanica"))
        {
            if (Time.time >= tempoUltimoDano + intervaloDano)
            {
                TomarDano(1);
                tempoUltimoDano = Time.time;
                StartCoroutine(PiscarDano());
            }
        }
    }

    void TomarDano(int quantidade)
    {
        vida -= quantidade;
        Debug.Log("Fada levou dano! Vida: " + vida);

        if (vida <= 0)
        {
            Debug.Log("Fada morreu!");
            // aqui você pode colocar animação de morte ou game over
        }
    }

    System.Collections.IEnumerator PiscarDano()
    {
        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
