using UnityEngine;

public class DashEffect : MonoBehaviour
{
    [Header("Ajustes do efeito")]
    public float lifetime = 0.35f;        // tempo total até destruir
    public float startScale = 0.6f;       // escala inicial
    public float endScale = 1.4f;         // escala final
    public bool fadeOut = true;           // faz fade da sprite
    public float rotationSpeed = 0f;      // rotação opcional durante a vida

    SpriteRenderer sr;
    float t = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / lifetime);

        // scale
        float s = Mathf.Lerp(startScale, endScale, p);
        transform.localScale = new Vector3(s, s, s);

        // optional rotation
        if (rotationSpeed != 0f)
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // fade
        if (fadeOut && sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, p);
            sr.color = c;
        }

        if (t >= lifetime)
            Destroy(gameObject);
    }

}
