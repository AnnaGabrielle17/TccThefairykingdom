using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 8;
    Queue<GameObject> queue = new Queue<GameObject>();

    public void Initialize(GameObject prefab, int size)
    {
        this.prefab = prefab;
        this.initialSize = size;
        for (int i = 0; i < size; i++)
        {
            GameObject go = Instantiate(prefab);
            go.SetActive(false);
            var pooled = go.GetComponent<PooledObject>();
            if (pooled == null) pooled = go.AddComponent<PooledObject>();
            pooled.prefab = prefab;
            pooled.originPool = this;
            go.transform.SetParent(this.transform);
            queue.Enqueue(go);
        }
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject go;
        if (queue.Count > 0)
        {
            go = queue.Dequeue();
        }
        else
        {
            go = Instantiate(prefab);
            var pooled = go.GetComponent<PooledObject>() ?? go.AddComponent<PooledObject>();
            pooled.prefab = prefab;
            pooled.originPool = this;
        }

        go.transform.SetParent(null);
        go.transform.position = pos;
        go.transform.rotation = rot;

        // reset básico
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var col = go.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sr = go.GetComponent<SpriteRenderer>() ?? go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;

        // reset antes de desativar
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var col = go.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        // parar partículas (se existirem)
        var ps = go.GetComponentInChildren<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        go.SetActive(false);
        go.transform.SetParent(this.transform);
        queue.Enqueue(go);
    }
    // dentro de SimpleObjectPool
void OnDestroy()
{
    // avisa PoolManager para limpar entradas associadas
    PoolManager.NotifyPoolDestroyed(prefab);
}

}
