using UnityEngine;

public class BirdShooter : MonoBehaviour
{
    [Header("Prefab do projétil")]
    public GameObject projectilePrefab;

    [Header("Ajustes do tiro")]
    public Transform spawnPoint; // onde o projétil nasce (pode ser um filho vazio)
    public Vector2 spawnOffset = new Vector2(-0.3f, 0f); // deslocamento relativo para spawn
    public float projectileSpeed = 6f;
    public int projectileDamage = 1;
    public float fireCooldown = 1.5f;
    [HideInInspector] public float lastFireTime = -999f;

    private void Reset()
    {
        spawnPoint = transform;
    }

    public void ShootLeft()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[BirdShooter:{name}] Projectile prefab NÃO atribuído.");
            return;
        }

        if (Time.time < lastFireTime + fireCooldown)
        {
            return;
        }

        lastFireTime = Time.time;

        Vector3 pos = (spawnPoint != null ? spawnPoint.position : transform.position) + (Vector3)spawnOffset;

        // small safety offset in Y to avoid spawning inside the bird collider
        pos.y += 0.05f;

        GameObject go = Instantiate(projectilePrefab, pos, Quaternion.identity);

        if (go == null)
        {
            Debug.LogError($"[BirdShooter:{name}] Instantiate do projectile retornou null.");
            return;
        }

        Debug.Log($"[BirdShooter:{name}] disparou projétil em {Time.time:F2} pos={pos}");

        // prevent projectile from colliding with the emitter
        Collider2D emitterCol = GetComponent<Collider2D>();
        Collider2D projCol = go.GetComponent<Collider2D>();
        if (emitterCol != null && projCol != null)
        {
            Physics2D.IgnoreCollision(emitterCol, projCol, true);
        }

        var proj = go.GetComponent<PoderPassaroProjectile>();
        if (proj != null)
        {
            proj.Initialize(Vector2.left, projectileSpeed, projectileDamage);
        }
        else
        {
            proj = go.GetComponentInChildren<PoderPassaroProjectile>();
            if (proj != null) proj.Initialize(Vector2.left, projectileSpeed, projectileDamage);
            else
            {
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.left * projectileSpeed;
                }
                else
                {
                    Debug.LogWarning($"[BirdShooter:{name}] prefab instanciado não tem PoderPassaroProjectile nem Rigidbody2D.");
                }
            }
        }
    }
}
