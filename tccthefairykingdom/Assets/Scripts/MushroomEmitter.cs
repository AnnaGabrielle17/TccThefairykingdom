
using UnityEngine;

public class MushroomEmitter : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;      // velocidade para a esquerda
    public bool flipSpriteWhenLeft = true;
    public Rigidbody2D rb;              // opcional: deixe vazio para usar transform

    [Header("Emit (Smoke-style)")]
    [Tooltip("Prefab da bolha (prefab, não instancia da cena)")]
    public GameObject bubblePrefab;
    public Transform emitPoint;         // onde as bolhas nascem
    public int bubbleCount = 4;         // quantas bolhas soltar por pulso (sugestão 3-6)
    public float burstRadius = 0.12f;   // quão compactas são as bolhas no burst (menor = mais juntas)
    public float emitSpreadX = 0.12f;    // dispersão horizontal inicial
    public float baseRiseSpeed = -0.9f;  // velocidade vertical inicial (negativo = cair)
    public float riseSpeedVariance = 0.15f;
    public float bubbleLifetime = 3.0f;
    public float bubbleBaseScale = 0.4f;  // escala base aplicada ao prefab
    [Tooltip("Multiplicador extra para aumentar/reduzir bolhas")]
    public float bubbleScaleMultiplier = 0.6f;

    [Header("Animation / Fallback")]
    public Animator animator;
    public float pulseInterval = 2f;    // fallback periódico se não usar AnimationEvent

    float timer;

    void Start()
    {
        if (emitPoint == null) emitPoint = transform;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // mover para a esquerda
        Vector2 vel = new Vector2(-walkSpeed, rb != null ? rb.linearVelocity.y : 0f);
        if (rb != null) rb.linearVelocity = vel;
        else transform.Translate(Vector2.left * walkSpeed * Time.deltaTime);

        if (flipSpriteWhenLeft)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = true;
        }

        // fallback de emissão periódica
        timer += Time.deltaTime;
        if (timer >= pulseInterval)
        {
            timer = 0f;
            if (animator) animator.SetTrigger("Pulse");
            else EmitBubbles();
        }
    }

    // Chame por AnimationEvent no pico do pulso, ou diretamente
    public void EmitBubbles()
    {
        if (bubblePrefab == null || emitPoint == null) return;

        for (int i = 0; i < bubbleCount; i++)
        {
            // spawn em disco (cluster compacto)
            float r = Mathf.Sqrt(Random.value) * burstRadius;
            float ang = Random.Range(0f, Mathf.PI * 2f);
            Vector2 offset = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
            Vector3 spawnPos = emitPoint.position + (Vector3)offset;

            GameObject b = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);

            // NÃO parentar — queremos que as bolhas "se soltem"
            // ajustar escala com pequena variação (para parecer conjunto de bolhas diferentes)
            float s = bubbleBaseScale * bubbleScaleMultiplier * Random.Range(0.85f, 1.05f);
            s = Mathf.Clamp(s, 0.08f, 1.2f); // evita escalas enormes por acidente
            b.transform.localScale = Vector3.one * s;

            // ajusta collider radius proporcionalmente (se houver)
            var circle = b.GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                circle.radius = Mathf.Abs(circle.radius) * s;
                circle.isTrigger = true;
            }

            // velocidade inicial: forçar Y negativo para cair (quando baseRiseSpeed negativo)
            float rise = baseRiseSpeed * (1f + Random.Range(-riseSpeedVariance, riseSpeedVariance));
            float driftX = Random.Range(-emitSpreadX * 0.5f, emitSpreadX * 0.5f);

            var rbBubble = b.GetComponent<Rigidbody2D>();
            if (rbBubble != null)
            {
                rbBubble.linearVelocity = new Vector2(driftX, rise);
                rbBubble.linearDamping = 0.8f;
                // se quiser usar gravidade, configure no prefab; aqui deixamos gravityScale como está
            }
            else
            {
                // fallback: adiciona mover simples
                var mover = b.GetComponent<AutoBubbleMover>();
                if (mover == null) mover = b.AddComponent<AutoBubbleMover>();
                mover.Initialize(new Vector2(driftX, rise));
            }

            // schedule destruction se necessário
            var destroyer = b.GetComponent<AutoDestroyOnTime>();
            if (destroyer == null)
            {
                destroyer = b.AddComponent<AutoDestroyOnTime>();
                destroyer.lifetime = bubbleLifetime;
            }

            // remove/disable orbiter se por acaso existir no prefab
            var orb = b.GetComponent<MonoBehaviour>();
            if (orb != null && orb.GetType().Name == "BubbleOrbiter") orb.enabled = false;

            // opcional: ajuste do sprite renderer para transparência inicial
            var sr = b.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Clamp01(c.a * 0.85f);
                sr.color = c;
            }
        }
    }
}
