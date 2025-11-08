using UnityEngine;

public class OscillatingProjectile : MonoBehaviour
{
    public float speedX = 5f;           // velocidade para a esquerda (use valor positivo; o script aplica Vector2.left)
    public float amplitude = 0.5f;      // quanto sobe/desce
    public float frequency = 2f;        // quão rápido oscila (Hz)
    public bool useFixedUpdate = true;  // true se usar física (Rigidbody2D)

    Rigidbody2D rb;
    float startY;
    float timer;
    float phase;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
        phase = Random.Range(0f, Mathf.PI * 2f); // evita que todas as esferas oscilem em sincronia
    }

    void FixedUpdate()
    {
        if (!useFixedUpdate) return;
        timer += Time.fixedDeltaTime;
        Vector2 pos = rb.position;
        pos.x += -speedX * Time.fixedDeltaTime; // move para a esquerda
        pos.y = startY + amplitude * Mathf.Sin(2f * Mathf.PI * frequency * timer + phase);
        rb.MovePosition(pos);
    }

    void Update()
    {
        if (useFixedUpdate) return;
        timer += Time.deltaTime;
        Vector3 pos = transform.position;
        pos.x += -speedX * Time.deltaTime;
        pos.y = startY + amplitude * Mathf.Sin(2f * Mathf.PI * frequency * timer + phase);
        transform.position = pos;
    }

}
