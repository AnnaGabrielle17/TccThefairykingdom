using UnityEngine;

public class FairyMovement : MonoBehaviour
{
    public float speed = 3f; // velocidade da fada

    [Header("Limites da cena")]
    public bool useCameraBounds = true; // se true: calcula limites pela câmera; se false: usa os valores abaixo
    public float minX = -5f;
    public float maxX = 5f;
    public float minY = -3f;
    public float maxY = 3f;
    public Vector2 padding = new Vector2(0.5f, 0.5f); // folga para não encostar na borda da câmera

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (useCameraBounds)
        {
            CalculateCameraBounds();
        }
    }

    void Update()
    {
        // Leitura dos eixos Horizontal (A/D, ←/→) e Vertical (W/S, ↑/↓)
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f);

        // Aplica a movimentação
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        // Inverte o sprite quando movimenta para a esquerda
        if (spriteRenderer != null)
        {
            if (moveX < -0.01f) spriteRenderer.flipX = true;
            else if (moveX > 0.01f) spriteRenderer.flipX = false;
        }

        // Garante que a fada fique dentro dos limites definidos
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    // Calcula limites do mundo a partir da viewport da câmera principal (funciona bem com camera ortográfica)
    void CalculateCameraBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Distância entre câmera e o plano da fada (necessário para ViewportToWorldPoint)
        float zDistance = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance)); // bottom-left do viewport
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance)); // top-right do viewport

        minX = bl.x + padding.x;
        minY = bl.y + padding.y;
        maxX = tr.x - padding.x;
        maxY = tr.y - padding.y;
    }
}
