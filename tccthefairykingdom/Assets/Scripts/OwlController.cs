using UnityEngine;

public class OwlController : MonoBehaviour
{
    public float speed = 1.5f;
    public bool walkLeft = true;
    public GameObject projectilePrefab;
    public Transform firePoint; // onde o projétil nasce
    public float fireCooldown = 2f;

    Animator animator;
    float nextFireTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Movimento simples para a esquerda
        if (walkLeft)
            transform.Translate(Vector2.left * speed * Time.deltaTime);

        animator.SetBool("isWalking", walkLeft);

        // Disparo por tempo (opcional) — a animação deve ser disparada para ter sincronização correta
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            animator.SetTrigger("Attack");
            // O projectile deve ser instanciado por um Animation Event chamando Fire()
        }
    }

    // Chame este método via Animation Event no clip Attack (ou pelo script se preferir)
    public void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
