using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BirdShooter: instancia projéteis e mantém lista dos projéteis ativos para poder destruí-los/retorná-los ao pool rapidamente.
/// </summary>
[DisallowMultipleComponent]
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

    // lista de projéteis ativos instanciados por este shooter
    private readonly List<PoderPassaroProjectile> activeProjectiles = new List<PoderPassaroProjectile>();

    private Collider2D emitterCollider;

    private void Reset()
    {
        spawnPoint = transform;
    }

    private void Awake()
    {
        emitterCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Atira para a esquerda.
    /// </summary>
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
        pos.y += 0.05f; // small safety offset

        GameObject go = Instantiate(projectilePrefab, pos, Quaternion.identity);
        if (go == null)
        {
            Debug.LogError($"[BirdShooter:{name}] Instantiate do projectile retornou null.");
            return;
        }

        // evita colisão com o emissor
        Collider2D projCol = go.GetComponent<Collider2D>();
        if (emitterCollider != null && projCol != null)
        {
            Physics2D.IgnoreCollision(emitterCollider, projCol, true);
        }

        // tenta configurar o componente do projétil
        var proj = go.GetComponent<PoderPassaroProjectile>();
        if (proj != null)
        {
            proj.Initialize(Vector2.left, projectileSpeed, projectileDamage,
                            owner: GetComponentInParent<BirdController>(),
                            shooter: this);

            RegisterProjectile(proj);
        }
        else
        {
            // fallback simples: usa velocity (não linearVelocity)
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.left * projectileSpeed;
                rb.gravityScale = 0f;
            }
            else
            {
                Debug.LogWarning($"[BirdShooter:{name}] prefab instanciado não tem PoderPassaroProjectile nem Rigidbody2D.");
            }
        }
    }

    /// <summary>
    /// Registra um projétil ativo criado por este shooter.
    /// </summary>
    public void RegisterProjectile(PoderPassaroProjectile p)
    {
        if (p == null) return;
        if (!activeProjectiles.Contains(p))
            activeProjectiles.Add(p);
    }

    /// <summary>
    /// Notificado pelo projétil quando ele é destruído/desativado (pool).
    /// </summary>
    public void NotifyProjectileDestroyed(PoderPassaroProjectile p)
    {
        if (p == null) return;
        activeProjectiles.Remove(p);
    }

    /// <summary>
    /// Destrói ou retorna ao pool todos os projéteis ativos.
    /// </summary>
    public void DestroyAllProjectiles()
    {
        if (activeProjectiles.Count == 0)
            return;

        // Cópia da lista para evitar remoção dupla via NotifyProjectileDestroyed()
        var copy = activeProjectiles.ToArray();

        foreach (var p in copy)
        {
            if (p == null) continue;

            var pooled = p.GetComponent<PooledObject>();

            if (pooled != null)
            {
                try
                {
                    pooled.ReturnToPool();
                }
                catch
                {
                    // fallback
                    p.gameObject.SetActive(false);
                }
            }
            else
            {
                try
                {
                    Destroy(p.gameObject);
                }
                catch
                {
                    if (p != null && p.gameObject != null)
                        p.gameObject.SetActive(false);
                }
            }
        }

        // Agora sim limpa tudo de forma segura
        activeProjectiles.Clear();
    }

    private void OnDestroy()
    {
        activeProjectiles.Clear();
    }
}
