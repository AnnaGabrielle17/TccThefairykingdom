using UnityEngine;

public class Mecanicas : MonoBehaviour
{
    public float velocidade = 2f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Evita qualquer rotação causada pela física ou colisões
        // Método compatível com Rigidbody2D antigo e novo:
        rb.freezeRotation = true; // opção simples
        // rb.constraints = RigidbodyConstraints2D.FreezeRotation; // alternativa
    }

    void FixedUpdate()
    {
        // Use velocity (nome correto) para controlar o movimento do Rigidbody2D
        rb.linearVelocity = new Vector2(-velocidade, rb.linearVelocity.y);
    }
}


