using UnityEngine;
using System.Collections;

public class ColetavelVidaVisivel : MonoBehaviour
{
    public int curaAmount = 1;
    public string playerTag = "Player";
    public float delayEntrePassos = 0.06f;

    public AudioClip somColeta;
    public GameObject vfxColeta;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        FadaDano fada = other.GetComponent<FadaDano>();
        if (fada == null) return;

        // chama a versão visível (passo-a-passo)
        fada.CurarVisivel(curaAmount, delayEntrePassos);

        if (somColeta != null)
            AudioSource.PlayClipAtPoint(somColeta, transform.position);

        if (vfxColeta != null)
            Instantiate(vfxColeta, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}

