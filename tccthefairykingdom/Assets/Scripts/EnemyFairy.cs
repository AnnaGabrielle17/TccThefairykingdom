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
    [Tooltip("Prefab que contém EnemyProjectile (script) e/o ParticleSystem visual")]
    public GameObject particlePrefab;
    [Tooltip("Transform de onde o projétil deve nascer (muzzle)")]
    public Transform muzzle;
    public float particleSpeed = 8f;
    [Tooltip("Quanto o projétil nasce à frente do muzzle")]
    public float spawnOffset = 0.6f;
    public float minSpawnDistance = 1f;
    public float fireDistance = 1.0f;
    public float instanceAutoDestroy = 2f;
    public int projectileDamage = 1; // dano aplicado ao jogador

    [Header("Animator (opcional)")]
    public Animator animator;
    private const string ANIM_ATTACK_BOOL = "isAttacking";
    public string attackStateName = "Sunfairy_Attack"; // nome do state (se for usar Play)

    // estado interno
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

        if (useStopPoint && stopPoint == null && stopByX && Mathf.Approximately(stopX, 0f))
            Debug.LogWarning("EnemyFairy: useStopPoint habilitado, mas stopPoint não atribuído e stopX=0.");
        if (useStopPoint && stopPoint == null && !stopByX && Mathf.Approximately(stopY, 0f))
            Debug.LogWarning("EnemyFairy: useStopPoint habilitado, mas stopPoint não atribuído e stopY=0.");
    }

    void Update()
    {
        // Se já chegou e parou: trava X e permite oscilação vertical
        if (arrivedAndStopped)
        {
            float y = attackPosition.y + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
            transform.position = new Vector3(attackPosition.x, y, transform.position.z);
            return;
        }

        // Movimento normal: vai para a esquerda enquanto não parou
        if (useStopPoint)
        {
            MoveTowardsStopPoint();
        }
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
        if (stopPoint != null)
        {
            targetPos = stopPoint.position;
            haveTarget = true;
        }
        else
        {
            if (stopByX)
            {
                targetPos = new Vector3(stopX, transform.position.y, transform.position.z);
                haveTarget = true;
            }
            else
            {
                targetPos = new Vector3(transform.position.x, stopY, transform.position.z);
                haveTarget = true;
            }
        }

        if (!haveTarget)
        {
            Debug.LogWarning("EnemyFairy: useStopPoint ativo mas sem alvo válido (stopPoint/stopX/stopY).");
            return;
        }

        float step = horizontalSpeed * Time.deltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, targetPos.x, step);
        float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
        transform.position = new Vector3(newX, newY, transform.position.z);

        bool reached;
        if (stopPoint != null)
            reached = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                                       new Vector2(targetPos.x, targetPos.y)) <= stopTolerance;
        else
            reached = stopByX ? Mathf.Abs(transform.position.x - targetPos.x) <= stopTolerance
                              : Mathf.Abs(transform.position.y - targetPos.y) <= stopTolerance;

        if (reached)
        {
            arrivedAndStopped = true;
            attackPosition = transform.position;
            canMove = false;

            // checa player se necessário
            bool playerOk = true;
            if (requirePlayerToAttack)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
                playerOk = false;
                foreach (var c in hits) if (c != null && c.CompareTag(playerTag)) { playerOk = true; break; }
            }

            if (playerOk) EnterAttackState();
            else EnterAttackState(); // mantemos ataque mesmo sem player — ajuste se desejar
        }
    }

    void DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
        Transform detected = null;
        foreach (var c in hits)
        {
            if (c != null && c.CompareTag(playerTag))
            {
                detected = c.transform;
                break;
            }
        }

        if (detected != null && currentTarget == null)
        {
            currentTarget = detected;
            EnterAttackState();
        }
    }

    void EnterAttackState()
    {
        // fallback target
        if (currentTarget == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) currentTarget = p.transform;
        }

        canMove = false;
        ForceStopMovement();

        // animações
        if (animator != null)
        {
            animator.SetBool(ANIM_ATTACK_BOOL, true);
            Debug.Log("EnterAttackState: isAttacking=true");
            // opcional: force play (recomendado usar transições baseadas no bool)
            // animator.Play(attackStateName, 0, 0f);
        }
        else
        {
            Debug.LogWarning("EnterAttackState: animator não atribuído.");
        }

        // permitir 1 disparo no início do ataque
        firedThisAttack = false;

        IgnoreCollisionsWithTarget(currentTarget, true);

        // Observação: FireFromPrefab() deve ser chamado por Animation Event
    }

    void ExitAttackState()
    {
        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, false);
        IgnoreCollisionsWithTarget(currentTarget, false);
        canMove = true;
        arrivedAndStopped = false;
    }

    // -------------------------
    // Animation Event / Fire
    // -------------------------

    // Função chamada por Animation Event no frame do tiro
    public void FireFromPrefab()
    {
        if (firedThisAttack)
        {
            Debug.Log("FireFromPrefab ignorado (já foi disparado neste ataque).");
            return;
        }

        firedThisAttack = true;
        Debug.Log($"FireFromPrefab called on {name}");

        if (particlePrefab == null)
        {
            Debug.LogWarning("FireFromPrefab: particlePrefab NÃO atribuído no Inspector.");
            return;
        }
        if (muzzle == null)
        {
            Debug.LogWarning("FireFromPrefab: muzzle NÃO atribuído no Inspector.");
            return;
        }

        // força direção para a esquerda (troque para calcular direção até o player se desejar)
        Vector2 dir = Vector2.left;

        // offset para não nascer dentro do muzzle
        float distanceToPlayer = currentTarget != null ? Vector2.Distance(muzzle.position, currentTarget.position) : Mathf.Infinity;
        float extraOffset = 0f;
        if (distanceToPlayer < minSpawnDistance)
            extraOffset = (minSpawnDistance - distanceToPlayer) + 0.05f;
        float desiredOffset = Mathf.Max(spawnOffset + extraOffset, fireDistance);
        Vector3 spawnPos = muzzle.position + (Vector3)(dir * desiredOffset);

        // instancia o prefab
        GameObject inst = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
        inst.transform.right = dir;

        // seta dados no script EnemyProjectile (se existir)
        var proj = inst.GetComponent<EnemyProjectile>() ?? inst.GetComponentInChildren<EnemyProjectile>();
        var projCol = inst.GetComponent<Collider2D>() ?? inst.GetComponentInChildren<Collider2D>();

        if (proj != null)
        {
            proj.direction = dir;
            proj.speed = particleSpeed;
            proj.lifeTime = Mathf.Max(proj.lifeTime, instanceAutoDestroy);
            proj.damage = projectileDamage;
        }
        else
        {
            Debug.LogWarning("FireFromPrefab: prefab instanciado NÃO contém EnemyProjectile (adicione ao prefab se quiser lógica).");
        }

        // ignora colisões com a própria inimiga
        if (projCol != null && enemyColliders != null)
        {
            foreach (var ec in enemyColliders)
            {
                if (ec == null) continue;
                Physics2D.IgnoreCollision(ec, projCol, true);
            }
        }

        // toca particle system visuals se houver
        var ps = inst.GetComponent<ParticleSystem>() ?? inst.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            if (main.simulationSpace != ParticleSystemSimulationSpace.World)
                Debug.LogWarning("ParticleSystem.simulationSpace != World. Recomendo definir como World no prefab.");
            ps.Play();
        }

        // destruição segura
        if (Application.isPlaying) Destroy(inst, instanceAutoDestroy);
        else DestroyImmediate(inst);
    }

    // Animation Event/Outro método para resetar a flag para o próximo loop
    // Chame ResetFireFlag no final do clip de ataque (ou após os frames de dano)
    public void ResetFireFlag()
    {
        firedThisAttack = false;
        Debug.Log("ResetFireFlag chamado: pode disparar no próximo loop de ataque.");
    }

    [ContextMenu("Test FireFromPrefab")]
    public void TestFireFromPrefab()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test FireFromPrefab: execute este teste apenas em Play Mode.");
            return;
        }
        FireFromPrefab();
    }

    // util
    void ForceStopMovement()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.angularVelocity = 0f;
        }
    }

    void IgnoreCollisionsWithTarget(Transform target, bool ignore)
    {
        if (target == null || enemyColliders == null) return;
        Collider2D[] targetCols = target.GetComponentsInChildren<Collider2D>();
        if (targetCols == null || targetCols.Length == 0) return;

        foreach (var ec in enemyColliders)
        {
            if (ec == null) continue;
            foreach (var tc in targetCols)
            {
                if (tc == null) continue;
                Physics2D.IgnoreCollision(ec, tc, ignore);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (useStopPoint)
        {
            Gizmos.color = Color.red;
            if (stopPoint != null)
                Gizmos.DrawWireSphere(stopPoint.position, stopTolerance);
            else if (stopByX)
                Gizmos.DrawLine(new Vector3(stopX, transform.position.y - 5f, 0f), new Vector3(stopX, transform.position.y + 5f, 0f));
            else
                Gizmos.DrawLine(new Vector3(transform.position.x - 5f, stopY, 0f), new Vector3(transform.position.x + 5f, stopY, 0f));
        }

        if (muzzle != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(muzzle.position, 0.08f);
        }
    }
}