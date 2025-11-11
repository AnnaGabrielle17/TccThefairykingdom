using UnityEngine;

public class FlocoRigidbodyMover : MonoBehaviour
{
    public float speed = 1.5f;
    public float rotationSpeed = 90f; // graus/seg
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // remover se quiser físico na rotação
    }

    void FixedUpdate()
    {
        // mover usando física
        Vector2 newPos = rb.position + Vector2.left * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // rotacionar via Transform (não usar torque se não quiser simulação física)
        transform.Rotate(Vector3.forward, rotationSpeed * Time.fixedDeltaTime);
    }
}

