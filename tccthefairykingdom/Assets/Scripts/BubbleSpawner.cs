using UnityEngine;
using System.Collections;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public float spawnInterval = 0.4f;
    public float radius = 0.25f;
    public float speedMin = 0.6f, speedMax = 1.2f;
    public float lifeMin = 1.2f, lifeMax = 2.4f;
    public bool spawnOnStart = true;

    void Start() {
        if (spawnOnStart) StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop() {
        while(true) {
            SpawnBubble();
            yield return new WaitForSeconds(spawnInterval * Random.Range(0.7f, 1.4f));
        }
    }

    public void SpawnBubble() {
        Vector2 offset = Random.insideUnitCircle * radius;
        Vector3 pos = transform.position + (Vector3)offset;
        var go = Instantiate(bubblePrefab, pos, Quaternion.identity);
        // configura velocidade para subir com variação
        float spd = Random.Range(speedMin, speedMax);
        float life = Random.Range(lifeMin, lifeMax);
        Vector2 vel = new Vector2(Random.Range(-0.15f, 0.15f), spd);
        var b = go.GetComponent<Bubble>();
        if (b != null) b.Initialize(vel, life);
    }
}

