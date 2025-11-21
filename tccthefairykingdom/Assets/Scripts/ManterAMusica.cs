using System.Collections;
using UnityEngine;

public class ManterAMusica : MonoBehaviour
{
    public static ManterAMusica instance;

    [Header("Audio")]
    public AudioSource audioSource;          // arraste o AudioSource do objeto aqui
    public bool playOnAwakeIfMissing = true; // se quiser que toque automaticamente na 1ª cena
    public float defaultVolume = 1f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("ManterAMusica: nenhum AudioSource encontrado no objeto.");
            }
        }

        if (audioSource != null)
        {
            audioSource.volume = defaultVolume;
            if (playOnAwakeIfMissing && !audioSource.isPlaying)
                audioSource.Play();
        }
    }

    // Pausa sem zerar posição (retoma do mesmo ponto)
    public void PauseMusic()
    {
        if (audioSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        audioSource.Pause();
    }

    // Retoma a reprodução (se estava pausada)
    public void ResumeMusic()
    {
        if (audioSource == null) return;
        if (!audioSource.isPlaying)
        {
            audioSource.UnPause(); // retoma do ponto pausado
        }
    }

    // Para e reseta para o início (pos 0)
    public void StopMusic()
    {
        if (audioSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        audioSource.Stop();
        audioSource.time = 0f;
    }

    // Fade out e para (opcional)
    public void FadeOutAndStop(float duration = 0.5f)
    {
        if (audioSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutCoroutine(duration));
    }

    // Fade in (opcional)
    public void FadeIn(float duration = 0.5f, float targetVolume = 1f)
    {
        if (audioSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeInCoroutine(duration, targetVolume));
    }

    IEnumerator FadeOutCoroutine(float duration)
    {
        float start = audioSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.volume = defaultVolume; // restaura volume interno (se quiser)
        fadeCoroutine = null;
    }

    IEnumerator FadeInCoroutine(float duration, float targetVolume)
    {
        audioSource.volume = 0f;
        if (!audioSource.isPlaying) audioSource.Play();
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
        fadeCoroutine = null;
    }
}
