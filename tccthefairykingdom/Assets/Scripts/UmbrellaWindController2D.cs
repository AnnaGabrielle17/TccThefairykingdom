using System.Collections;
using UnityEngine;

/// <summary>
/// UmbrellaWindController2D - versão completa e corrigida
/// - Sincroniza startFrame do ParticleSystem quando o sprite da sombrinha está entre os sprites do PS (comparação por texture+rect).
/// - NÃO sobrescreve sprites do ParticleSystem.
/// - Reposiciona o emitter e a zona de dano à frente da sombrinha (emitterLocalOffset).
/// - Ajusta startSpeed do PS e o tamanho do BoxCollider2D do WindZone (se aplicável).
/// - Logs para ajudar debug.
/// </summary>

public class UmbrellaWindController2D : MonoBehaviour
{
    [Header("Refs")]
    public ParticleSystem windParticles;    // atribuir no inspector (child WindParticles)
    public Collider2D windDamageZone;       // trigger collider (child WindZone)
    public SpriteRenderer umbrellaSprite;   // SpriteRenderer da sombrinha (assign)

    [Header("Timing")]
    [Tooltip("Duração do vento em segundos (0 ou negativo = indefinido)")]
    public float windDuration = 1f;

    [Header("Fallback behavior (quando o sprite da sombrinha NÃO for encontrado nos frames do PS)")]
    public bool useRandomIfNotFound = true;
    [Tooltip("Se useRandomIfNotFound = false e esse valor for válido, usará esse índice como fallback (senão não altera startFrame).")]
    public int fallbackFrameIndex = 0;

    [Header("Auto-positioning")]
    [Tooltip("Offset local do emissor/zone quando tocar o vento (x negativo = esquerda)")]
    public Vector2 emitterLocalOffset = new Vector2(-0.8f, 0f);
    [Tooltip("Tamanho alvo do BoxCollider2D do WindZone (width, height)")]
    public Vector2 colliderSize = new Vector2(1.2f, 0.8f);
    [Tooltip("Velocidade inicial sugerida das partículas (aumente se quiser vento mais longe)")]
    public float particleStartSpeed = 3.5f;

    private Coroutine stopRoutine;
    private BoxCollider2D cachedBoxCollider;

    void Awake()
    {
        if (windParticles == null) windParticles = GetComponent<ParticleSystem>();
        if (windDamageZone != null) windDamageZone.enabled = false;
    }

    void Start()
    {
        // cache do BoxCollider2D (se for BoxCollider2D)
        if (windDamageZone != null)
            cachedBoxCollider = windDamageZone as BoxCollider2D;

        // aplica startSpeed inicial no ParticleSystem (se configurável)
        if (windParticles != null)
        {
            var main = windParticles.main;
            main.startSpeed = particleStartSpeed;
        }
    }

    // chamada por Animation Event: PlayWind
    public void PlayWind()
    {
        Debug.Log("[UmbrellaWind] PlayWind() chamado.");

        // reposiciona emitter e zona de dano
        if (windParticles != null)
            windParticles.transform.localPosition = emitterLocalOffset;

        if (windDamageZone != null)
        {
            windDamageZone.transform.localPosition = emitterLocalOffset;
            windDamageZone.enabled = true;

            if (cachedBoxCollider != null)
            {
                cachedBoxCollider.size = colliderSize;
                cachedBoxCollider.offset = Vector2.zero;
            }
        }

        // sincroniza frame (se possível) — não sobrescreve sprites do PS
        ApplyUmbrellaFrameToParticles();

        // play e stop coroutine
        if (stopRoutine != null) StopCoroutine(stopRoutine);
        if (windParticles != null) windParticles.Play();

        if (windDuration > 0f)
            stopRoutine = StartCoroutine(StopAfter(windDuration));
    }

    public void StopWind()
    {
        if (stopRoutine != null) { StopCoroutine(stopRoutine); stopRoutine = null; }
        if (windParticles != null) windParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (windDamageZone != null) windDamageZone.enabled = false;
    }

    private IEnumerator StopAfter(float t)
    {
        yield return new WaitForSeconds(t);
        StopWind();
    }

    private void ApplyUmbrellaFrameToParticles()
    {
        if (windParticles == null)
        {
            Debug.LogWarning("[UmbrellaWind] windParticles == null");
            return;
        }

        if (umbrellaSprite == null || umbrellaSprite.sprite == null)
        {
            Debug.LogWarning("[UmbrellaWind] umbrellaSprite não atribuído ou sem sprite.");
            return;
        }

        var tsa = windParticles.textureSheetAnimation;
        tsa.enabled = true;
        tsa.mode = ParticleSystemAnimationMode.Sprites;

        // trava a animação para não progredir frames durante a vida da partícula
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f); // sempre usa startFrame

        int spriteCount = tsa.spriteCount;
        if (spriteCount <= 0)
        {
            Debug.LogWarning("[UmbrellaWind] TextureSheetAnimation não possui sprites configurados.");
            return;
        }

        // procura índice do sprite atual entre os sprites do TSAM (comparação robusta)
        int foundIndex = -1;
        Sprite target = umbrellaSprite.sprite;

        for (int i = 0; i < spriteCount; i++)
        {
            Sprite s = tsa.GetSprite(i);
            if (s == null) continue;

            if (s.texture == target.texture && s.rect == target.rect)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex >= 0)
        {
            tsa.startFrame = new ParticleSystem.MinMaxCurve((float)foundIndex);
            Debug.Log($"[UmbrellaWind] Encontrou sprite da sombrinha nos frames do PS. Usando índice {foundIndex}.");
        }
        else
        {
            // NÃO sobrescrever sprites do Particle System! apenas aplicar fallback seguro
            if (useRandomIfNotFound)
            {
                int rand = Random.Range(0, spriteCount);
                tsa.startFrame = new ParticleSystem.MinMaxCurve((float)rand);
                Debug.LogWarning($"[UmbrellaWind] Sprite da sombrinha NÃO encontrado. Usando frame aleatório {rand} dos frames do PS.");
            }
            else
            {
                if (fallbackFrameIndex >= 0 && fallbackFrameIndex < spriteCount)
                {
                    tsa.startFrame = new ParticleSystem.MinMaxCurve((float)fallbackFrameIndex);
                    Debug.LogWarning($"[UmbrellaWind] Sprite da sombrinha NÃO encontrado. Usando fallbackFrameIndex {fallbackFrameIndex}.");
                }
                else
                {
                    Debug.LogWarning("[UmbrellaWind] Sprite da sombrinha NÃO encontrado. Mantendo frames originais do ParticleSystem.");
                }
            }
        }
    }
}