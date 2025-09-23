using UnityEngine;

public class EnemyFairy : MonoBehaviour
{
   public float speed = 2f;         // Velocidade de movimento
    public float moveRange = 2f;     // Quanto ela sobe e desce
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Movimento vertical automático (sobe e desce em loop)
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * moveRange;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
