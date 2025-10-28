using UnityEngine;
using System.Collections;


public class EnemyFairy : MonoBehaviour
{
 [Header("Movimento")]
    public float horizontalSpeed = 2f;
    public float verticalSpeed = 2f;
    public float verticalRange = 1f;

    [Header("Parar em um ponto")]
    public bool useStopPoint = false;        // ativa o comportamento de "ir até um ponto e parar"
    public bool stopByX = true;              // compara X (true) ou Y (false) se usar stopByCoordinate
    public float stopX = -5f;                // se stopByX e useStopPoint: quando x <= stopX (inimigo vindo da direita)
    public float stopY = 0f;                 // se !stopByX: quando y <= stopY (dependendo do movimento)
    public Transform stopPoint;              // alternativa: arraste um Transform para parada exata (usa antes de stopX/Y se presente)
    public float stopTolerance = 0.05f;      // tolerância de distância para considerar "chegou"

    [Header("Detecção (opcional)")]
    public float detectionRadius = 3f;
    public LayerMask detectionLayer = ~0;
    public string playerTag = "Player";
    public bool requirePlayerToAttack = true; // se true, só ataca quando o player estiver na cena/na detecção

    [Header("Projétil (Prefab)")]
    public GameObject particlePrefab;
    public Transform muzzle;
    public float particleSpeed = 8f;
    public float spawnOffset = 0.6f;
    public float minSpawnDistance = 1f;
    public float fireDistance = 1.0f;
    public float instanceAutoDestroy = 2f;

    [Header("Animator (opcional)")]
    public Animator animator;
    private const string ANIM_ATTACK_BOOL = "isAttacking";

    // estado
    private float startY;
    private bool canMove = true;
    private bool arrivedAndStopped = false;   // flag: já chegou no ponto e parou
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
        if (useStopPoint && stopPoint == null)
        {
            // se usar stopPoint opcional não definido, usa stopX/Y
        }

        // debug warnings para facilitar configuração
        if (useStopPoint && stopPoint == null && !stopByX && Mathf.Approximately(stopY, 0f))
            Debug.LogWarning("EnemyFairy: useStopPoint habilitado, mas stopPoint não atribuído e stopY=0. Verifique inspector.");
        if (useStopPoint && stopPoint == null && stopByX && Mathf.Approximately(stopX, 0f))
            Debug.LogWarning("EnemyFairy: useStopPoint habilitado, mas stopPoint não atribuído e stopX=0. Verifique inspector.");
    }

    void Update()
    {
        // se já parou e está atacando, trava posição
        if (arrivedAndStopped)
        {
            transform.position = attackPosition;
            return;
        }

        // se usa o comportamento "ir até um ponto", prioriza isso
        if (useStopPoint)
        {
            MoveTowardsStopPoint();
        }
        else
        {
            // movimento padrão: caminha para a esquerda enquanto oscila verticalmente
            if (canMove)
            {
                transform.Translate(Vector2.left * horizontalSpeed * Time.deltaTime);
                float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        // opcional: se não usa stopPoint e requirePlayerToAttack==true, mantemos DetectPlayer pra iniciar ataque se player entrar
        if (!useStopPoint && requirePlayerToAttack)
            DetectPlayer();
    }

    // move o inimigo até o stoppoint/stopX e, ao chegar, chama EnterAttackState()
    void MoveTowardsStopPoint()
    {
        // Se um stopPoint Transform foi fornecido, mira nele (posição exata)
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
                // mantemos mesma y atual, só checamos X
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

        // move horizontalmente em direção ao target X (mantém oscilação vertical enquanto não chegou)
        float step = horizontalSpeed * Time.deltaTime;
        Vector3 newPos = transform.position;

        // move X em direção ao target X por step (sem pular)
        float newX = Mathf.MoveTowards(transform.position.x, targetPos.x, step);
        // mantém a oscilação vertical enquanto ainda não chegou
        float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
        newPos = new Vector3(newX, newY, transform.position.z);
        transform.position = newPos;

        // checa se chegou no ponto (usando tolerância)
        bool reached = false;
        if (stopPoint != null)
        {
            reached = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                                       new Vector2(targetPos.x, targetPos.y)) <= stopTolerance;
        }
        else
        {
            if (stopByX)
                reached = Mathf.Abs(transform.position.x - targetPos.x) <= stopTolerance;
            else
                reached = Mathf.Abs(transform.position.y - targetPos.y) <= stopTolerance;
        }

        if (reached)
        {
            // travar e iniciar ataque (só se não exigir player ou se player estiver detectado/na cena)
            bool playerOk = true;
            if (requirePlayerToAttack)
            {
                // checa se player está nas proximidades (OverlapCircleAll)
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
                playerOk = false;
                foreach (var c in hits)
                {
                    if (c != null && c.CompareTag(playerTag)) { playerOk = true; break; }
                }
            }

            if (playerOk)
            {
                // trava posição e entra em ataque
                arrivedAndStopped = true;
                attackPosition = transform.position;
                canMove = false;
                EnterAttackState();
            }
            else
            {
                // se não encontrou jogador no momento, pode ficar em espera (ou ainda travar se preferir)
                arrivedAndStopped = true;
                attackPosition = transform.position;
                canMove = false;
            }
        }
    }

    // Detecta player se quiser atacar sem stopPoint
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
        else if (detected == null && currentTarget != null)
        {
            // opcional: sair do estado (se quiser)
            // ExitAttackState();
            // currentTarget = null;
        }
    }

    // quando parar para atacar (mantém sua lógica existente)
    void EnterAttackState()
    {
        // garante target de fallback
        if (currentTarget == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) currentTarget = p.transform;
        }

        // força parada e grava posição (attackPosition já setada)
        canMove = false;
        ForceStopMovement();

        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, true);

        IgnoreCollisionsWithTarget(currentTarget, true);

        // FireFromPrefab deve ser chamado por Animation Event no clip de ataque
    }

    // saída de ataque (opcional)
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

    // Método principal (com logs) — chamado pela Animation Event
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

        // fallback target
        if (currentTarget == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) currentTarget = p.transform;
        }

        Vector2 dir = currentTarget != null ? (currentTarget.position - muzzle.position).normalized : transform.right;

        float distanceToPlayer = currentTarget != null ? Vector2.Distance(muzzle.position, currentTarget.position) : Mathf.Infinity;
        float extraOffset = 0f;
        if (distanceToPlayer < minSpawnDistance)
            extraOffset = (minSpawnDistance - distanceToPlayer) + 0.05f;

        float desiredOffset = Mathf.Max(spawnOffset + extraOffset, fireDistance);
        Vector3 spawnPos = muzzle.position + (Vector3)(dir * desiredOffset);

        GameObject inst = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
        inst.transform.right = dir;

        ParticleSystem ps = inst.GetComponent<ParticleSystem>();
        if (ps == null) ps = inst.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            if (main.simulationSpace != ParticleSystemSimulationSpace.World)
                Debug.LogWarning("ParticleSystem.simulationSpace != World. Recomendo definir como World no prefab.");

            // força sorting alto pra teste de visibilidade
            var rend = ps.GetComponent<ParticleSystemRenderer>();
            if (rend != null) rend.sortingOrder = Mathf.Max(rend.sortingOrder, 100);

            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            ep.position = spawnPos;
            ep.applyShapeToPosition = false;
            ep.velocity = dir * particleSpeed;
            ps.Emit(ep, 1);
            ps.Play();
        }
        else
        {
            Debug.LogWarning("FireFromPrefab: prefab instanciado NÃO contém ParticleSystem!");
        }

        // destruição correta dependendo do modo (Editor vs Play)
        if (Application.isPlaying)
            Destroy(inst, instanceAutoDestroy);
        else
            DestroyImmediate(inst);
    }

    // Teste seguro só em Play Mode
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

    // utility
    void ForceStopMovement()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // corrigido para propriedade correta
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
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