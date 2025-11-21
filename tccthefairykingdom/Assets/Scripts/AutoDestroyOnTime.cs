using UnityEngine;

public class AutoDestroyOnTime : MonoBehaviour
{
    public float lifetime = 3f;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
