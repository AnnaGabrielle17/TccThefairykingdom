using UnityEngine;



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
    [Tooltip("Prefab do projétil: pode ser um GameObject com EnemyProjectile e/ou um ParticleSystem")]
    public GameObject particlePrefab;
    public Transform muzzle;
    public float particleSpeed = 8f;
    public float spawnOffset = 0.6f;
    public float minSpawnDistance = 1f;
    public float fireDistance = 1.0f;
    public float instanceAutoDestroy = 2f;
    public int projectileDamage = 1;

    [Header("Animator (opcional)")]
    public Animator animator;
    private const string ANIM_ATTACK_BOOL = "isAttacking";
    public string attackStateName = "Sunfairy_Attack";

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

            if (playerOk) EnterAttackState();
            else EnterAttackState();
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
        if (currentTarget == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) currentTarget = p.transform;
        }

        canMove = false;
        ForceStopMovement();

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
        // FireFromPrefab será chamado por Animation Event (ou pode chamar diretamente para teste)
    }

    void ExitAttackState()
    {
        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, false);
        IgnoreCollisionsWithTarget(currentTarget, false);
        canMove = true;
        arrivedAndStopped = false;
    }

    // ------------------------
    // Animation Event / Fire
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

        // força direção para esquerda
        Vector2 dir = Vector2.left;

        // offset de spawn
        float distanceToPlayer = currentTarget != null ? Vector2.Distance(muzzle.position, currentTarget.position) : Mathf.Infinity;
        float extraOffset = 0f;
        if (distanceToPlayer < minSpawnDistance)
            extraOffset = (minSpawnDistance - distanceToPlayer) + 0.05f;

        float desiredOffset = Mathf.Max(spawnOffset + extraOffset, fireDistance);
        Vector3 spawnPos = muzzle.position + (Vector3)(dir * desiredOffset);

        // instancia prefab
        GameObject inst = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
        inst.transform.right = dir;

        // LOG pra debug (ajuda muito)
        bool rootPS = inst.GetComponent<ParticleSystem>() != null;
        bool childPS = inst.GetComponentInChildren<ParticleSystem>() != null;
        Debug.Log($"Instanciado {inst.name} | rootPS={rootPS} | childPS={childPS} | pos={inst.transform.position} rot={inst.transform.eulerAngles} scale={inst.transform.lossyScale}");

        // tenta configurar script do projétil
        EnemyProjectile proj = inst.GetComponent<EnemyProjectile>() ?? inst.GetComponentInChildren<EnemyProjectile>();
        Collider2D projCol = inst.GetComponent<Collider2D>() ?? inst.GetComponentInChildren<Collider2D>();

        if (proj != null)
        {
            // usa o Init para evitar problemas de proteção/overload
            proj.Init(dir, particleSpeed, Mathf.Max(proj.lifeTime, instanceAutoDestroy), projectileDamage);
        }
        else
        {
            Debug.LogWarning("FireFromPrefab: prefab instanciado NÃO contém EnemyProjectile (adicione o script ao prefab).");
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

        // configura visual do ParticleSystem (se houver)
        ParticleSystem ps = inst.GetComponent<ParticleSystem>() ?? inst.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            // tenta forçar parâmetros mínimos seguros
            if (main.simulationSpace != ParticleSystemSimulationSpace.World)
            {
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                Debug.Log("FireFromPrefab: definiu simulationSpace = World no ParticleSystem (por segurança).");
            }

            // ajustes mínimos se estiver muito pequeno
            if (main.startLifetime.mode == ParticleSystemCurveMode.Constant && main.startLifetime.constant < 0.05f)
                main.startLifetime = 0.6f;
            if (main.startSize.mode == ParticleSystemCurveMode.Constant && main.startSize.constant < 0.02f)
                main.startSize = 0.25f;

            // Renderer: material / sorting
            var rend = ps.GetComponent<ParticleSystemRenderer>();
            if (rend != null)
            {
                if (rend.sharedMaterial == null)
                {
                    // cria um material simples para sprites (apenas em runtime)
                    rend.material = new Material(Shader.Find("Sprites/Default"));
                    Debug.Log("FireFromPrefab: ParticleSystemRenderer não tinha material -> atribuído Sprites/Default.");
                }

                // força order alto para ficar na frente
                try
                {
                    rend.sortingOrder = Mathf.Max(rend.sortingOrder, 1000);
                }
                catch { }
            }

            // tenta tocar / emitir
            ps.Play();
            ps.Emit(8); // emite um pequeno burst para testar visibilidade
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
            Debug.LogWarning("Test FireFrom Prefab: execute este teste apenas em Play Mode.");
            return;
        }
        FireFromPrefab();
    }

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