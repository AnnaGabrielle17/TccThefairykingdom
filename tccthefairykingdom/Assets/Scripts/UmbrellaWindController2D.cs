using UnityEngine;

public class UmbrellaWindController2D : MonoBehaviour
{
    [Header("Refs")]
    public ParticleSystem windParticles;    // atribuir no inspector
    public Collider2D windDamageZone;       // trigger collider
    public SpriteRenderer umbrellaSprite;   // SpriteRenderer da sombrinha (assign)

    [Header("Timing")]
    public float windDuration = 1f;

    private Coroutine stopRoutine;

    void Awake()
    {
        if (windParticles == null) windParticles = GetComponent<ParticleSystem>();
        if (windDamageZone != null) windDamageZone.enabled = false;
    }

    // chamada por Animation Event: PlayWind
    public void PlayWind()
    {
        // 1) tenta sincronizar o frame
        ApplyUmbrellaFrameToParticles();

        // 2) ativa PS + zona de dano
        if (stopRoutine != null) StopCoroutine(stopRoutine);
        windParticles.Play();
        if (windDamageZone != null) windDamageZone.enabled = true;

        if (windDuration > 0f)
            stopRoutine = StartCoroutine(StopAfter(windDuration));
    }

    public void StopWind()
    {
        if (stopRoutine != null) { StopCoroutine(stopRoutine); stopRoutine = null; }
        windParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (windDamageZone != null) windDamageZone.enabled = false;
    }

    private System.Collections.IEnumerator StopAfter(float t)
    {
        yield return new WaitForSeconds(t);
        StopWind();
    }

    private void ApplyUmbrellaFrameToParticles()
    {
        if (windParticles == null || umbrellaSprite == null) return;

        var tsa = windParticles.textureSheetAnimation;
        tsa.enabled = true;
        tsa.mode = ParticleSystemAnimationMode.Sprites;

        // 1) trava a animação para não progredir frames durante a vida da partícula
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f); // sempre usa startFrame

        // 2) procura índice do sprite atual entre os sprites do TSAM
        int foundIndex = -1;
        int spriteCount = tsa.spriteCount; // total de sprites adicionados no module
        for (int i = 0; i < spriteCount; i++)
        {
            Sprite s = tsa.GetSprite(i);
            if (s == umbrellaSprite.sprite)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex >= 0)
        {
            // define Start Frame como esse índice (StartFrame aceita MinMaxCurve)
            tsa.startFrame = new ParticleSystem.MinMaxCurve((float)foundIndex);
            // opcional: se quiser garantir que não tenha aleatoriedade:
            // tsa.startFrameMultiplier = 1f;
        }
        else
        {
            // fallback: substitui o slot 0 pelasprite atual (ou usa index 0)
            Debug.LogWarning("Sprite da sombrinha não encontrado no TextureSheetAnimation. Substituindo slot 0.");
            if (spriteCount > 0)
            {
                tsa.SetSprite(0, umbrellaSprite.sprite);
                tsa.startFrame = new ParticleSystem.MinMaxCurve(0f);
            }
        }
    }
}
