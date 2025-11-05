using UnityEngine;

public class MushroomEmitter : MonoBehaviour
{
     [Header("Movement")]
    public float walkSpeed = 1.5f;      // velocidade para a esquerda
    public bool flipSpriteWhenLeft = true;
    public Rigidbody2D rb;              // opcional: deixe vazio para usar transform

    [Header("Emit")]
    public GameObject bubblePrefab;     // prefab com BubbleOrbiter + Collider (Is Trigger)
    public Transform emitPoint;         // local onde as bolhas nascem (geralmente filho)
    public int bubbleCount = 16;        // quantidade de bolhas na "esfera"
    public float sphereRadius = 0.7f;   // raio da esfera de bolhas (unidades)
    public float bubbleBaseScale = 1f;
    public float globalSpinSpeed = 40f; // graus/s - velocidade de rotação da esfera

    [Header("Fallback (animação)")]
    public Animator animator;
    public float pulseInterval = 2f;    // só se não usar AnimationEvent

    float timer;
    float currentSpin;
    
    
     // ângulo de rotação acumulado

    void Start()
    {
        if (emitPoint == null) emitPoint = transform;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // andar para a esquerda continuamente
        Vector2 vel = new Vector2(-walkSpeed, rb != null ? rb.linearVelocity.y : 0f);
        if (rb != null) rb.linearVelocity = vel;
        else transform.Translate(Vector2.left * walkSpeed * Time.deltaTime);

        // flip (opcional)
        if (flipSpriteWhenLeft)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = true;
        }

        // spin acumulado usado pelas bolhas (pode ser lido por elas no Start/Update)
        currentSpin += globalSpinSpeed * Time.deltaTime;

        // fallback emissão periódica (se não usar AnimationEvent)
        timer += Time.deltaTime;
        if (timer >= pulseInterval)
        {
            timer = 0f;
            if (animator) animator.SetTrigger("Pulse");
            else EmitBubbles();
        }
    }

    // Chamar por Animation Event (pico da animação Pulse) ou manualmente
    public void EmitBubbles()
    {
        if (bubblePrefab == null) return;

        // instancia N bolhas em posições distribuídas na esfera
        // usamos uma distribuição simples: golden-section-like para espalhar uniformemente
        for (int i = 0; i < bubbleCount; i++)
        {
            Vector3 spherePoint = RandomPointOnSphere(i, bubbleCount) * sphereRadius;
            GameObject b = Instantiate(bubblePrefab, emitPoint.position, Quaternion.identity);
            // parent nas bolhas sob o emitPoint para seguir o movimento do cogumelo
            b.transform.SetParent(emitPoint, worldPositionStays: true);

            // inicializa o orbiter (se existir)
            var orb = b.GetComponent<BubbleOrbiter>();
            if (orb != null)
            {
                orb.Init(spherePoint, bubbleBaseScale, currentSpin);
            }

            // opção: ajusta rotação inicial do sprite para apontar radialmente
            b.transform.up = (b.transform.position - emitPoint.position).normalized;
        }
    }

    // Gera um ponto aproximadamente uniformemente distribuído na esfera unitária
    Vector3 RandomPointOnSphere(int index, int total)
    {
        // técnica de Fibonacci/spherical distribution (determinística por index)
        float phi = Mathf.Acos(1f - 2f * ((index + 0.5f) / total)); // polar
        float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * (index + 0.5f); // azimuth
        float x = Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = Mathf.Cos(phi);
        float z = Mathf.Sin(phi) * Mathf.Sin(theta);
        return new Vector3(x, y, z).normalized;
    }
}