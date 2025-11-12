using UnityEngine;
using System.Collections;

public class FadinhaController : MonoBehaviour
{
    [Header("Movimento")]
    public float hoverAmplitude = 0.25f;
    public float hoverSpeed = 2f;

    [Header("Ataque")]
    public Transform attackPoint;
    public GameObject projectilePrefab;
    public float attackCooldown = 1.2f;
    public LayerMask targetLayer;
    public float detectRange = 6f;

    [Header("Opções")]
    public bool faceTarget = true;

    private Animator animator;
    private Vector3 startPos;
    private float cooldownTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    void Update()
    {
        HoverMotion();
        cooldownTimer -= Time.deltaTime;

        bool hasTarget = CheckForTarget();
        animator.SetBool("isFlying", true);

        if (hasTarget && cooldownTimer <= 0f)
        {
            cooldownTimer = attackCooldown;
            animator.SetTrigger("Attack");
            // spawn via Animation Event -> SpawnAttackProjectile()
        }
    }

    void HoverMotion()
    {
        float y = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = startPos + new Vector3(0, y, 0);
    }

    bool CheckForTarget()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, targetLayer);
        if (hit != null && faceTarget)
        {
            Vector3 dir = hit.transform.position - transform.position;
            FaceDirection(dir.x);
        }
        return hit != null;
    }

    void FaceDirection(float x)
    {
        if (x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        else if (x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
    }

    // Chamado por Animation Event durante o clip de ataque
    public void SpawnAttackProjectile()
    {
        if (projectilePrefab == null || attackPoint == null) return;

        GameObject p = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        IceProjectile proj = p.GetComponent<IceProjectile>();
        if (proj != null)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, targetLayer);
            Vector2 dir;
            if (hit != null)
                dir = (hit.transform.position - attackPoint.position).normalized;
            else
                dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            proj.Launch(dir);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
