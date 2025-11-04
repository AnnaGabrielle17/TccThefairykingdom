using UnityEngine;

public class MushroomEmitter : MonoBehaviour
{
    [Header("Prefabs & Points")]
    public GameObject bubblePrefab;
    public Transform emitPoint; // onde as bolhas nascem (filho do cogumelo)

    [Header("Emissão")]
    public int bubbleCount = 6;
    public float spreadAngle = 60f; // ângulo total em graus
    public float bubbleSpeed = 3f; // units/s
    [Range(0f, 0.5f)] public float speedVariance = 0.12f;

    [Header("Fallback (se não usar AnimationEvent)")]
    public Animator animator;
    public float pulseInterval = 2f; // fallback por Update que aciona trigger "Pulse"

    float timer;

    void Start()
    {
        if (emitPoint == null) emitPoint = transform;
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // fallback simples que dispara a animação/emit se quiser que seja periódico
        timer += Time.deltaTime;
        if (timer >= pulseInterval)
        {
            timer = 0f;
            if (animator) animator.SetTrigger("Pulse");
            else EmitBubbles();
        }
    }

    // Chame este método por Animation Event no pico do pulso (ou via código)
    public void EmitBubbles()
    {
        if (bubblePrefab == null) return;

        for (int i = 0; i < bubbleCount; i++)
        {
            float t = (bubbleCount == 1) ? 0f : (i / (float)(bubbleCount - 1) - 0.5f);
            float angle = t * spreadAngle;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;

            GameObject b = Instantiate(bubblePrefab, emitPoint.position, Quaternion.identity);
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            float speedFactor = 1f + Random.Range(-speedVariance, speedVariance);
            if (rb != null)
            {
                rb.linearVelocity = dir * bubbleSpeed * speedFactor;
            }

            // rotação opcional para o sprite
            b.transform.up = dir;
        }
    }

}
