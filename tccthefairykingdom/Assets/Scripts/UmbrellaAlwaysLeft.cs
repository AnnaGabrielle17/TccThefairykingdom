using UnityEngine;

public class UmbrellaAlwaysLeft : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.5f;             // velocidade para a esquerda (units/seg)
    public bool useKinematicMove = false;  // se true usa MovePosition (Rigidbody2D kinematic), senão seta velocity

    [Header("Visual")]
    public SpriteRenderer spriteRendererToFlip; // opcional, para garantir que esteja "olhando" para a esquerda
    public bool flipToLeft = true;               // se true força visual para esquerda

    void Awake()
    {
        // tenta pegar o SpriteRenderer automaticamente se não atribuído
        if (spriteRendererToFlip == null)
            spriteRendererToFlip = GetComponent<SpriteRenderer>();

        if (flipToLeft && spriteRendererToFlip != null)
        {
            // garante que sprite esteja apontando para a esquerda visualmente
            spriteRendererToFlip.flipX = true; // ajuste se seu sprite "frente" for flipX = false
        }
    }

    void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (useKinematicMove)
        {
            // recomenda: Rigidbody2D.BodyType = Kinematic
            Vector2 next = rb.position + Vector2.left * speed * Time.fixedDeltaTime;
            rb.MovePosition(next);
        }
        else
        {
            // recomenda: Rigidbody2D.BodyType = Dynamic, Freeze Rotation Z no Rigidbody2D (constraints)
            rb.linearVelocity = new Vector2(-Mathf.Abs(speed), rb.linearVelocity.y);
        }
    }

    // alternativa simples sem física (se preferir usar Transform diretamente):
    // void Update() { transform.Translate(Vector2.left * speed * Time.deltaTime); }
}
