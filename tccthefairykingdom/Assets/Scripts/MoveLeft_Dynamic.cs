using UnityEngine;

public class MoveLeft_Dynamic : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = Vector2.left * speed;
    }
}