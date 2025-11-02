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
[Tooltip("Prefab que contém EnemyProjectile (script) e sprite — NÃO precisa ser ParticleSystem.")]
public GameObject particlePrefab; 
[Tooltip("Transform de onde o projétil deve nascer (muzzle) — um child vazio posicionado na frente da varinha")]
public Transform muzzle;
[Tooltip("Velocidade linear do projétil (vai para EnemyProjectile.speed)")]
public float particleSpeed = 12f;
[Tooltip("Quanto o projétil nasce à frente do muzzle")]
public float spawnOffset = 0.12f;
public float minSpawnDistance = 1f;
public float fireDistance = 0.6f;
[Tooltip("Tempo antes de destruir automaticamente a instância do projétil (usado como fallback)")]
public float instanceAutoDestroy = 4f;
public int projectileDamage = 1;

[Header("Animator (opcional)")]
public Animator animator;
private const string ANIM_ATTACK_BOOL = "isAttacking";
public string attackStateName = "Sunfairy_Attack";

// estado
private float startY;
private bool canMove = true;
private bool arrivedAndStopped = false;
private Transform currentTarget = null;
private Vector3 attackPosition;
private Collider2D[] enemyColliders;

// controla um tiro por ciclo/loop de ataque
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

    if (useStopPoint)
        MoveTowardsStopPoint();
    else
    {
        if (canMove)
        {
            transform.Translate(Vector2.left * horizontalSpeed * Time.deltaTime);
            float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    if (!useStopPoint && requirePlayerToAttack)
        DetectPlayer();
}

void MoveTowardsStopPoint()
{
    Vector3 targetPos = Vector3.zero;
    bool haveTarget = false;
    if (stopPoint != null) { targetPos = stopPoint.position; haveTarget = true; }
    else
    {
        if (stopByX) { targetPos = new Vector3(stopX, transform.position.y, transform.position.z); haveTarget = true; }
        else { targetPos = new Vector3(transform.position.x, stopY, transform.position.z); haveTarget = true; }
    }

    if (!haveTarget) { Debug.LogWarning("EnemyFairy: useStopPoint ativo mas sem alvo válido."); return; }

    float step = horizontalSpeed * Time.deltaTime;
    float newX = Mathf.MoveTowards(transform.position.x, targetPos.x, step);
    float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
    transform.position = new Vector3(newX, newY, transform.position.z);

    bool reached = stopPoint != null
        ? Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(targetPos.x, targetPos.y)) <= stopTolerance
        : (stopByX ? Mathf.Abs(transform.position.x - targetPos.x) <= stopTolerance : Mathf.Abs(transform.position.y - targetPos.y) <= stopTolerance);

    if (reached)
    {
        arrivedAndStopped = true;
        attackPosition = transform.position;
        canMove = false;

        bool playerOk = true;
        if (requirePlayerToAttack)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
            playerOk = false;
            foreach (var c in hits) if (c != null && c.CompareTag(playerTag)) { playerOk = true; break; }
        }

        EnterAttackState();
    }
}

void DetectPlayer()
{
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
    Transform detected = null;
    foreach (var c in hits) if (c != null && c.CompareTag(playerTag)) { detected = c.transform; break; }

    if (detected != null && currentTarget == null)
    {
        currentTarget = detected;
        EnterAttackState();
    }
}

void EnterAttackState()
{
    canMove = false;
    ForceStopMovement();

    if (animator != null)
        animator.SetBool(ANIM_ATTACK_BOOL, true);

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

    // direção fixa da inimiga: sempre para a esquerda
    Vector2 dir = Vector2.left;

    // spawn do projétil um pouco à frente do muzzle
    Vector3 spawnPos = muzzleWorldPos + (Vector3)(dir * spawnOffset);

    GameObject inst = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

    // garantir que o sprite do projétil não inverta, mantendo a orientação original do prefab
    inst.transform.right = Vector3.right;

    // configurar o script do projétil
    var proj = inst.GetComponent<EnemyProjectile>() ?? inst.GetComponentInChildren<EnemyProjectile>();
    if (proj != null)
        proj.Init(dir, particleSpeed, instanceAutoDestroy, projectileDamage);

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

    // destruição automática
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