using UnityEngine;
using System.Collections;

public class ColetavelAumentaVelocidade : MonoBehaviour
{
    [Tooltip("Multiplicador aplicado à speed (ex: 1.5 = +50%)")]
    public float multiplicador = 1.5f;

    [Tooltip("Duração em segundos. 0 = permanente")]
    public float duracao = 5f;

    [Tooltip("Tag do jogador (defina 'Player' no GameObject da fada)")]
    public string playerTag = "Player";

    public AudioClip somColeta;
    public GameObject vfxColeta;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // tenta pegar o script de movimento que você mostrou
        FairyMovement fm = other.GetComponent<FairyMovement>();
        if (fm == null)
        {
            Debug.LogWarning("ColetavelAumentaVelocidade: FairyMovement não encontrado no objeto colidido.");
            return;
        }

        // aplica multiplicador
        if (multiplicador != 1f)
        {
            fm.speed *= multiplicador;
        }

        // efeitos
        if (somColeta != null) AudioSource.PlayClipAtPoint(somColeta, transform.position);
        if (vfxColeta != null) Instantiate(vfxColeta, transform.position, Quaternion.identity);

        // se for temporário, agendamos a reversão
        if (duracao > 0f && multiplicador != 1f)
        {
            StartCoroutine(ReverterDepois(fm, multiplicador, duracao));
        }

        // destrói o coletável (padrão: destrói imediatamente)
        Destroy(gameObject);
    }

    private IEnumerator ReverterDepois(FairyMovement fm, float mult, float wait)
    {
        yield return new WaitForSeconds(wait);

        // Se o componente ainda existir, tenta reverter dividindo pela multiplicador
        if (fm != null)
        {
            if (mult != 0f)
                fm.speed /= mult;
        }
    }
}

