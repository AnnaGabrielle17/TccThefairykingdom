using UnityEngine;

[RequireComponent(typeof(Transform))]
public class MushroomPulseParticles : MonoBehaviour
{
   [Header("Referências")]
    public Transform spriteTransform;     // arraste o objeto do sprite (filho)
    public ParticleSystem ps;             // arraste o Particle System (filho)

    [Header("Pulso (escala)")]
    public float pulseSpeed = 1.8f;
    public float pulseMagnitude = 0.10f;  // exemplo: 0.10 = 10%

    [Header("Parâmetros base das partículas")]
    public float baseEmission = 30f;
    public float baseParticleSize = 0.06f;
    public float baseRadius = 0.14f;
    public float baseOrbital = 35f;

    // armazenar estados
    private Vector3 originalSpriteScale;
    private bool triedAutoFind = false;

    void Awake()
    {
        // tenta auto referenciar (caso não arrastou no inspector)
        if (spriteTransform == null && transform.childCount > 0)
            spriteTransform = transform.GetChild(0);

        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>();

        if (spriteTransform != null)
            originalSpriteScale = spriteTransform.localScale;
        else
            originalSpriteScale = Vector3.one;
    }

    void Start()
    {
        if (ps == null)
        {
            Debug.LogWarning("[MushroomPulseParticles] ParticleSystem não atribuído ou não encontrado no filho!");
            return;
        }

        // garante que as partículas comecem a tocar
        if (!ps.isPlaying)
            ps.Play();

        // garantia extra: ative módulos que vamos controlar
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
    }

    void Update()
    {
        // segurança para tentar auto-find uma vez (se esquecer de arrastar)
        if (!triedAutoFind)
        {
            triedAutoFind = true;
            if (ps == null)
                ps = GetComponentInChildren<ParticleSystem>();
            if (spriteTransform == null && transform.childCount > 0)
                spriteTransform = transform.GetChild(0);
        }

        float t = Time.time * pulseSpeed;
        float pulse01 = (Mathf.Sin(t) + 1f) * 0.5f; // 0..1

        // --- Pulsar a escala (respeitando escala original) ---
        float localScaleFactor = 1f + pulseMagnitude * Mathf.Sin(t); // 1 + (-amp..+amp)
        if (spriteTransform != null)
            spriteTransform.localScale = Vector3.Scale(originalSpriteScale, Vector3.one * localScaleFactor);

        if (ps == null) return;

        // --- Emissão ---
        var em = ps.emission;
        float emissionNow = baseEmission * Mathf.Lerp(0.6f, 1.9f, pulse01);
        emissionNow = Mathf.Max(0.1f, emissionNow); // nunca deixe zero
        em.rateOverTime = new ParticleSystem.MinMaxCurve(emissionNow);

        // --- Main (tamanho) ---
        var main = ps.main;
        float sizeNow = baseParticleSize * Mathf.Lerp(0.7f, 1.6f, pulse01);
        sizeNow = Mathf.Max(0.001f, sizeNow);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeNow);

        // --- Shape (raio) ---
        var shape = ps.shape;
        shape.radius = baseRadius * Mathf.Lerp(0.85f, 1.25f, pulse01);

        // --- Velocity over Lifetime (orbital Z) ---
        var velModule = ps.velocityOverLifetime;
        velModule.enabled = true;
        float orbitalNow = baseOrbital * Mathf.Lerp(0.8f, 1.5f, pulse01);
        velModule.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalNow);
    }
}
