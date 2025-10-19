using UnityEngine;
using System.Collections;

public class ColetavelAumentaVelocidade : MonoBehaviour
{
    public float multiplicador = 1.5f;
    public float duracao = 5f; // segundos, 0 = permanente
    public string playerTag = "Player";
    public AudioClip somColeta;
    public GameObject vfxColeta;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerSpeedBuff buff = other.GetComponent<PlayerSpeedBuff>();
        if (buff == null) buff = other.gameObject.AddComponent<PlayerSpeedBuff>();

        buff.AddMultiplier(multiplicador, duracao);

        if (somColeta != null) AudioSource.PlayClipAtPoint(somColeta, transform.position);
        if (vfxColeta != null) Instantiate(vfxColeta, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}


