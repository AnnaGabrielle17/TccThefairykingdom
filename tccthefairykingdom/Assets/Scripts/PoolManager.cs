using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    static Dictionary<int, SimpleObjectPool> poolsById = new Dictionary<int, SimpleObjectPool>();
    static Dictionary<int, GameObject> prefabById = new Dictionary<int, GameObject>();

    public static void CreatePool(GameObject prefab, int initialSize = 8)
    {
        if (prefab == null) return;
        int id = prefab.GetInstanceID();

        // se já existe e o pool ainda é válido, não faz nada
        if (poolsById.TryGetValue(id, out var existingPool))
        {
            if (existingPool != null && existingPool.gameObject != null) return;
            // pool inválida -> remover para recriar
            poolsById.Remove(id);
            prefabById.Remove(id);
        }

        GameObject container = new GameObject($"Pool_{prefab.name}");
        var pool = container.AddComponent<SimpleObjectPool>();
        pool.Initialize(prefab, Mathf.Max(1, initialSize));

        poolsById[id] = pool;
        prefabById[id] = prefab;
    }

    public static GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;
        int id = prefab.GetInstanceID();

        if (!poolsById.TryGetValue(id, out var pool) || pool == null || pool.gameObject == null)
        {
            // pool ausente ou inválida -> recriar automaticamente (fallback)
            CreatePool(prefab, 6);
            poolsById.TryGetValue(id, out pool);
            if (pool == null) return null;
        }

        return pool.Get(pos, rot);
    }

    public static void Return(GameObject prefab, GameObject instance)
    {
        if (instance == null) return;
        if (prefab == null)
        {
            // tenta pegar prefab registrado no PooledObject
            var pooled = instance.GetComponent<PooledObject>();
            if (pooled != null && pooled.prefab != null)
            {
                prefab = pooled.prefab;
            }
        }

        if (prefab == null)
        {
            Object.Destroy(instance);
            return;
        }

        int id = prefab.GetInstanceID();
        if (poolsById.TryGetValue(id, out var pool) && pool != null && pool.gameObject != null)
        {
            pool.Return(instance);
        }
        else
        {
            // sem pool válida -> destruir para não manter referências inválidas
            Object.Destroy(instance);
        }
    }

    // chamado por SimpleObjectPool quando for destruído
    public static void NotifyPoolDestroyed(GameObject prefab)
    {
        if (prefab == null) return;
        int id = prefab.GetInstanceID();
        poolsById.Remove(id);
        prefabById.Remove(id);
    }

    public static bool HasPoolFor(GameObject prefab)
    {
        if (prefab == null) return false;
        int id = prefab.GetInstanceID();
        return poolsById.TryGetValue(id, out var pool) && pool != null && pool.gameObject != null;
    }
}
