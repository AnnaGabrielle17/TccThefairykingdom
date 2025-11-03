using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class EnemyFairy : MonoBehaviour
{
   [Header("Movimento")]
    public float horizontalSpeed = 2f;
    public float verticalSpeed = 2f;
    public float verticalRange = 1f;

    [Header("Parar em um ponto")]
    public bool useStopPoint = false;
    public bool stopByX = true;
    public float stopX = -5f;
    public float stopY = 0f;
    public Transform stopPoint;
    public float stopTolerance = 0.05f;

    [Header("Detecção (opcional)")]
    public float detectionRadius = 3f;
    public LayerMask detectionLayer = ~0;
    public string playerTag = "Player";
    public bool requirePlayerToAttack = true;

    [Header("Projétil (Prefab)")]
    public GameObject particlePrefab;
    public Transform muzzle;
    public float particleSpeed = 12f;
    public float spawnOffset = 0.12f;
    public float instanceAutoDestroy = 4f;
    public int projectileDamage = 1;

    [Header("Ajuste de seguimento vertical")]
    [Tooltip("Multiplicador aplicado à diferença de altura (player.y - muzzle.y) para virar velocidade Y")]
    public float verticalFollowMultiplier = 1f;
    [Tooltip("Velocidade máxima vertical do projétil (evita movimentos muito bruscos)")]
    public float maxVerticalSpeed = 4f;

    [Header("Animator (opcional)")]
    public Animator animator;
    private const string ANIM_ATTACK_BOOL = "isAttacking";

    // estado
    private float startY;
    private bool canMove = true;
    private bool arrivedAndStopped = false;
    private Transform currentTarget = null;
    private Vector3 attackPosition;
    private Collider2D[] enemyColliders;

    private bool firedThisAttack = false;

    void Awake()
    {
        enemyColliders = GetComponentsInChildren<Collider2D>();
    }

    void Start()
    {
        startY = transform.position.y;
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (arrivedAndStopped)
        {
            float y = attackPosition.y + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
            transform.position = new Vector3(attackPosition.x, y, transform.position.z);
            return;
        }

        if (useStopPoint) MoveTowardsStopPoint();
        else if (canMove)
        {
            transform.Translate(Vector2.left * horizontalSpeed * Time.deltaTime);
            float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        if (!useStopPoint && requirePlayerToAttack) DetectPlayer();
    }

    void MoveTowardsStopPoint()
    {
        Vector3 targetPos = stopPoint != null ? stopPoint.position :
                            stopByX ? new Vector3(stopX, transform.position.y, transform.position.z) :
                                      new Vector3(transform.position.x, stopY, transform.position.z);

        float step = horizontalSpeed * Time.deltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, targetPos.x, step);
        float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
        transform.position = new Vector3(newX, newY, transform.position.z);

        bool reached = stopPoint != null
            ? Vector2.Distance(transform.position, targetPos) <= stopTolerance
            : (stopByX ? Mathf.Abs(transform.position.x - targetPos.x) <= stopTolerance
                       : Mathf.Abs(transform.position.y - targetPos.y) <= stopTolerance);

        if (reached)
        {
            arrivedAndStopped = true;
            attackPosition = transform.position;
            canMove = false;
            EnterAttackState();
        }
    }

    void DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
        foreach (var c in hits)
            if (c != null && c.CompareTag(playerTag))
            {
                currentTarget = c.transform;
                EnterAttackState();
                break;
            }
    }

    void EnterAttackState()
    {
        canMove = false;
        ForceStopMovement();

        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, true);
        firedThisAttack = false;
    }

    void ExitAttackState()
    {
        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, false);
        canMove = true;
        arrivedAndStopped = false;
    }

    // Animation Event: chamado no frame do tiro dentro do clip de ataque
    public void FireFromPrefab()
    {
        if (firedThisAttack) return;
        firedThisAttack = true;

        if (particlePrefab == null)
        {
            Debug.LogWarning("FireFromPrefab: particlePrefab NÃO atribuído no Inspector.");
            return;
        }

        Vector3 muzzleWorldPos = muzzle != null ? muzzle.position : transform.position;

        // direção horizontal fixa: sempre para a esquerda (x negativo)
        float xSpeed = -particleSpeed;

        // calcular ySpeed baseado na diferença de altura (player.y - muzzle.y)
        float ySpeed = 0f;
        if (currentTarget != null)
        {
            float dy = currentTarget.position.y - muzzleWorldPos.y;
            ySpeed = dy * verticalFollowMultiplier;
            ySpeed = Mathf.Clamp(ySpeed, -maxVerticalSpeed, maxVerticalSpeed);
        }

        Vector2 velocity = new Vector2(xSpeed, ySpeed);

        // spawn do projétil um pouco à frente do muzzle (apenas deslocamento horizontal)
        Vector3 spawnPos = muzzleWorldPos + Vector3.left * spawnOffset;

        GameObject inst = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

        // garantir escala positiva (evita flips por herança)
        inst.transform.localScale = new Vector3(
            Mathf.Abs(inst.transform.localScale.x),
            Mathf.Abs(inst.transform.localScale.y),
            Mathf.Abs(inst.transform.localScale.z)
        );

        // configurar o script do projétil — Launch aplica a velocidade sem rotacionar o transform
        var proj = inst.GetComponent<EnemyProjectile>() ?? inst.GetComponentInChildren<EnemyProjectile>();
        if (proj != null)
        {
            proj.Launch(velocity, projectileDamage, instanceAutoDestroy);
        }
        else
        {
            var rb = inst.GetComponent<Rigidbody2D>() ?? inst.GetComponentInChildren<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = velocity;
        }

        // ignorar colisão com o inimigo
        var projCol = inst.GetComponent<Collider2D>() ?? inst.GetComponentInChildren<Collider2D>();
        if (projCol != null && enemyColliders != null)
        {
            foreach (var ec in enemyColliders)
                if (ec != null) Physics2D.IgnoreCollision(ec, projCol, true);
        }

        // opcional: ParticleSystem
        var ps = inst.GetComponent<ParticleSystem>() ?? inst.GetComponentInChildren<ParticleSystem>();
        if (ps != null) ps.Play();

        // destruição automática (fallback)
        if (Application.isPlaying) Destroy(inst, instanceAutoDestroy);
        else DestroyImmediate(inst);
    }

    public void ResetFireFlag()
    {
        firedThisAttack = false;
    }

    [ContextMenu("Test FireFromPrefab")]
    public void TestFireFromPrefab()
    {
        if (!Application.isPlaying) return;
        FireFromPrefab();
    }

    void ForceStopMovement()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 v = rb.linearVelocity;
            v.x = 0f;
            rb.linearVelocity = v;
            rb.angularVelocity = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (useStopPoint)
        {
            Gizmos.color = Color.red;
            if (stopPoint != null) Gizmos.DrawWireSphere(stopPoint.position, stopTolerance);
            else if (stopByX) Gizmos.DrawLine(new Vector3(stopX, transform.position.y - 5f, 0f), new Vector3(stopX, transform.position.y + 5f, 0f));
            else Gizmos.DrawLine(new Vector3(transform.position.x - 5f, stopY, 0f), new Vector3(transform.position.x + 5f, stopY, 0f));
        }

        if (muzzle != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(muzzle.position, 0.08f);
        }
    }
}