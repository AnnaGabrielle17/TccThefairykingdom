using UnityEngine;

public class Mecanicas : MonoBehaviour
{
    public float velocidade = 2f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // sempre anda para a esquerda
        rb.linearVelocity = new Vector2(-velocidade, rb.linearVelocity.y);
    }
    }


