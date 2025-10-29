using UnityEngine;
using System.Collections;


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
    public float particleSpeed = 8f;
    public float spawnOffset = 0.6f;
    public float minSpawnDistance = 1f;
    public float fireDistance = 1.0f;
    public float instanceAutoDestroy = 2f;
    public int projectileDamage = 1; // dano aplicado ao jogador pelo projétil

    [Header("Animator (opcional)")]
    public Animator animator;
    private const string ANIM_ATTACK_BOOL = "isAttacking";
    public string attackStateName = "Sunfairy_Attack"; // altere se necessário

    // estado interno
    private float startY;
    private bool canMove = true;
    private bool arrivedAndStopped = false;
    private Transform currentTarget = null;
    private Vector3 attackPosition;
    private Collider2D[] enemyColliders;

    void Awake()
    {
        enemyColliders = GetComponentsInChildren<Collider2D>();
    }

    void Start()
    {
        startY = transform.position.y;
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (useStopPoint && stopPoint == null && stopByX && Mathf.Approximately(stopX, 0f))
            Debug.LogWarning("EnemyFairy: useStopPoint habilitado, mas stopPoint não atribuído e stopX=0. Verifique inspector.");
        if (useStopPoint && stopPoint == null && !stopByX && Mathf.Approximately(stopY, 0f))
            Debug.LogWarning("EnemyFairy: useStopPoint habilitado, mas stopPoint não atribuído e stopY=0. Verifique inspector.");
    }

    void Update()
    {
        // Se já chegou e parou: travamos SOMENTE no eixo X e mantemos oscilação vertical
        if (arrivedAndStopped)
        {
            float y = attackPosition.y + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
            transform.position = new Vector3(attackPosition.x, y, transform.position.z);
            return;
        }

        // Movimento padrão: sempre se move para a esquerda enquanto não chegou ao stop
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

        bool reached = false;
        if (stopPoint != null)
            reached = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                                       new Vector2(targetPos.x, targetPos.y)) <= stopTolerance;
        else
        {
            if (stopByX) reached = Mathf.Abs(transform.position.x - targetPos.x) <= stopTolerance;
            else reached = Mathf.Abs(transform.position.y - targetPos.y) <= stopTolerance;
        }

        if (reached)
        {
            // trava X e guarda posição para manter a oscilação vertical a partir daqui
            arrivedAndStopped = true;
            attackPosition = transform.position;
            canMove = false;

            // valida presença do player se necessário
            bool playerOk = true;
            if (requirePlayerToAttack)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
                playerOk = false;
                foreach (var c in hits) if (c != null && c.CompareTag(playerTag)) { playerOk = true; break; }
            }

            if (playerOk) EnterAttackState();
            else EnterAttackState(); // se quiser esperar até o player chegar altere aqui
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
        ForceStopMovement(); // zera apenas a velocidade X

        // animação: seta bool e tenta forçar o state Attack (apenas para garantir)
        if (animator != null)
        {
            animator.SetBool(ANIM_ATTACK_BOOL, true);
            Debug.Log("EnterAttackState: isAttacking=true");

            int hash = Animator.StringToHash(attackStateName);
            if (animator.HasState(0, hash))
            {
                Debug.Log("EnterAttackState: animator.HasState -> forçando Play(" + attackStateName + ")");
                animator.Play(attackStateName, 0, 0f);
            }
            else
            {
                Debug.LogWarning("EnterAttackState: state '" + attackStateName + "' não encontrado na layer 0 do Animator.");
            }
        }
        else
        {
            Debug.LogWarning("EnterAttackState: animator não atribuído.");
        }

        IgnoreCollisionsWithTarget(currentTarget, true);
        // FireFromPrefab deve ser chamado por Animation Event no clip de ataque
    }

    void ExitAttackState()
    {
        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, false);
        IgnoreCollisionsWithTarget(currentTarget, false);
        canMove = true;
        arrivedAndStopped = false;
    }

    // ------------------------
    // Animation Event / Fire (FORÇA O TIRO PARA A ESQUERDA)
    // ------------------------
    
        public void FireFromPrefab()
{
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

    // FORÇAR direção para a ESQUERDA
    Vector2 dir = Vector2.left;

    // calculos de offset (coloque spawnOffset pequeno para não nascer muito longe)
    float distanceToPlayer = currentTarget != null ? Vector2.Distance(muzzle.position, currentTarget.position) : Mathf.Infinity;
    float extraOffset = 0f;
    if (distanceToPlayer < minSpawnDistance)
        extraOffset = (minSpawnDistance - distanceToPlayer) + 0.05f;

    // desiredOffset controla quão longe do muzzle o projétil nasce
    float desiredOffset = Mathf.Max(spawnOffset + extraOffset, fireDistance);
    // se quiser nascer praticamente na frente, use spawnOffset = 0.12 no Inspector
    Vector3 spawnPos = muzzle.position + (Vector3)(dir * desiredOffset);

    // instancia o prefab DO PROJÉTIL (não um ParticleSystem isolado)
    GameObject inst = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
    inst.transform.right = dir; // orienta a visualização

    // tenta configurar o script do projétil caso exista
    EnemyProjectile proj = inst.GetComponent<EnemyProjectile>() ?? inst.GetComponentInChildren<EnemyProjectile>();
    Collider2D projCol = inst.GetComponent<Collider2D>() ?? inst.GetComponentInChildren<Collider2D>();

    if (proj != null)
    {
        proj.direction = dir;
        proj.speed = particleSpeed;       // ajuste no Inspector do inimigo
        proj.lifeTime = Mathf.Max(proj.lifeTime, instanceAutoDestroy); // garante tempo
        // proj.damage já pode ser ajustado no prefab ou aqui:
        // proj.damage = 1;
    }
    else
    {
        Debug.LogWarning("FireFromPrefab: prefab instanciado NÃO contém EnemyProjectile (adicione o script ao prefab).");
    }

    // evita colisão com a própria inimiga (ignora entre todos os colliders do inimigo e do projétil)
    if (projCol != null && enemyColliders != null)
    {
        foreach (var ec in enemyColliders)
        {
            if (ec == null) continue;
            Physics2D.IgnoreCollision(ec, projCol, true);
        }
    }

    // se o prefab tiver um ParticleSystem para visuals, força Play
    ParticleSystem ps = inst.GetComponent<ParticleSystem>() ?? inst.GetComponentInChildren<ParticleSystem>();
    if (ps != null)
    {
        var main = ps.main;
        if (main.simulationSpace != ParticleSystemSimulationSpace.World)
            Debug.LogWarning("ParticleSystem.simulationSpace != World. Recomendo definir como World no prefab.");
        ps.Play();
    }

    // destruição segura
    if (Application.isPlaying)
        Destroy(inst, instanceAutoDestroy);
    else
        DestroyImmediate(inst);
}
    

    [ContextMenu("Test FireFromPrefab")]
    public void TestFireFromPrefab()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test FireFromPrefab: execute este teste apenas em Play Mode. Entre no Play e tente de novo.");
            return;
        }
        FireFromPrefab();
    }

    // utility: zera apenas componente X da velocidade se houver Rigidbody2D
    void ForceStopMovement()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.angularVelocity = 0f;
            // NÃO colocamos rb.isKinematic = true para preservar oscilações verticais feitas por física
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