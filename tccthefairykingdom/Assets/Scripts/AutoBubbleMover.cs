using UnityEngine;

public class AutoBubbleMover : MonoBehaviour
{
    Vector2 velocity;

    public void Initialize(Vector2 initialVelocity)
    {
        velocity = initialVelocity;
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        // pequena desaceleração vertical
        velocity.y = Mathf.Lerp(velocity.y, velocity.y * 0.6f, Time.deltaTime * 0.2f);
        // drift horizontal suave
        velocity.x += Mathf.Sin(Time.time * 1.6f + GetInstanceID()) * 0.0025f;
    }
}
