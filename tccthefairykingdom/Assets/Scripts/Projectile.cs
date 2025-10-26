using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 3f;
    int direction = 1;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Se rb existir, define velocidade inicial
        if (rb != null) rb.linearVelocity = new Vector2(direction * speed, 0f);
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(int dir)
    {
        direction = dir;
        if (rb != null) rb.linearVelocity = new Vector2(direction * speed, 0f);

        // ajusta escala do sprite para apontar na direção certa
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;
    }

    // Exemplo de colisão:
    void OnTriggerEnter2D(Collider2D other)
    {
        // se colidir em inimigo, aplicar dano...
        // Destroy ao colidir (se quiser)
        Destroy(gameObject);
    }

}
