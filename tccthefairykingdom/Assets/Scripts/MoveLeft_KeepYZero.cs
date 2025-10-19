using UnityEngine;

public class MoveLeft_KeepYZero : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true; // evita rotação indesejada
        rb.gravityScale = 0f;     // garante que gravidade não o puxe (opcional)
    }

    void FixedUpdate()
    {
        // força a componente Y a 0 (zera qualquer vertical)
        rb.linearVelocity = new Vector2(-speed, 0f);
    }
}

