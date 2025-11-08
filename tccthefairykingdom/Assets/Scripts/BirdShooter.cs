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
    private float lastFireTime = -999f;

    private void Reset()
    {
        // tenta facilitar a configuração no inspector
        spawnPoint = transform;
    }

    public void ShootLeft()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab não atribuído em BirdShooter.");
            return;
        }

        if (Time.time < lastFireTime + fireCooldown) return;
        lastFireTime = Time.time;

        Vector3 pos = (spawnPoint != null ? spawnPoint.position : transform.position) + (Vector3)spawnOffset;
        GameObject go = Instantiate(projectilePrefab, pos, Quaternion.identity);

        // inicializa direção/velocidade/dano
        var proj = go.GetComponent<PoderPassaroProjectile>();
        if (proj != null)
        {
            proj.Initialize(Vector2.left, projectileSpeed, projectileDamage);
        }
        else
        {
            proj = go.GetComponentInChildren<PoderPassaroProjectile>();
            if (proj != null) proj.Initialize(Vector2.left, projectileSpeed, projectileDamage);
            // ...fallbacks...
        }

        // Exemplo de uso automático (descomente se desejar que o pássaro dispare periodicamente sem animação)
        /*
        private void Update()
        {
            if (Time.time > lastFireTime + fireCooldown)
            {
                ShootLeft();
            }
        }
        */

    }
}
