using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject prefab;
    public SimpleObjectPool originPool;

    public void ReturnToPool()
    {
        if (originPool != null) originPool.Return(this.gameObject);
        else Destroy(this.gameObject);
    }
}
