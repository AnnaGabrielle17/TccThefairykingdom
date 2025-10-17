using UnityEngine;

public class Bubble : MonoBehaviour
{
    
    public float lifetime = 2f;
    public Vector2 velocity = new Vector2(0f, 0.6f);
    public float drift = 0.2f;
    public float rotateSpeed = 20f;

    SpriteRenderer sr;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2 customVelocity, float customLifetime = -1f)
    {
        if (customLifetime > 0f) lifetime = customLifetime;
        velocity = customVelocity;
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.position += (Vector3)(Mathf.Sin(Time.time * 2f) * drift * Time.deltaTime * Vector3.right);
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);

        if (sr != null)
        {
            float t = Mathf.Clamp01(timer / lifetime);
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;
        }

        if (timer >= lifetime) Destroy(gameObject);
    }
}

