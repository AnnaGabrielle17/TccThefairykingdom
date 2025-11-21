using UnityEngine;
using UnityEngine.Events;

public class FairyController : MonoBehaviour
{
    [Header("Projéteis")]
    [Tooltip("Prefab do projétil normal (arraste o PREFAB, não um objeto da cena)")]
    public GameObject powerPrefab;
    [Tooltip("Prefab do projétil SUPER (opcional). Se não setado usa powerPrefab mesmo com power ativo.")]
    public GameObject superProjectilePrefab;
    public Transform firePoint;
    public float powerSpeed = 8f;
    [Tooltip("Se true, trata powerPrefab como já sendo super (fallback)")]
    public bool prefabIsSuper = false;

    [Header("Pool")]
    [Tooltip("Tamanho inicial do pool para projéteis")]
    public int projectilePoolSize = 8;
    [Tooltip("Tamanho inicial do pool para VFX (hit effects)")]
    public int vfxPoolSize = 6;

    [Header("Ataque / Input")]
    public KeyCode attackKey = KeyCode.K;
    [Tooltip("Cooldown entre ataques (aplicado no momento do SPAWN)")]
    public float attackCooldown = 0.45f;
    private float lastAttackTime = -999f;

    [Header("Optional Audio")]
    public AudioClip attackSfx;
    [Range(0f,1f)] public float attackSfxVolume = 1f;

    [Header("Animation Event Hooks")]
    public UnityEvent onAttackEvent;

    [Header("Debug / Helper")]
    public int debugForceSortingOrder = 50;

    [Header("Behavior")]
    [Tooltip("Se true, o projétil só será super se o PlayerCombat.hasSuperPower for true.")]
    public bool requirePlayerActiveSuper = true;

    // Guard para evitar chamadas quase simultâneas (animação com >1 AnimationEvent)
    [Header("Protection")]
    [Tooltip("Tempo mínimo entre dois spawns aceitos (segundos) — defesa extra contra double-spawn)")]
    public float duplicateGuardTime = 0.15f;

    // estado interno
    private Animator anim;
    private int facing = 1; // 1 = direita, -1 = esquerda

    // controle extra para identificar spawn recente
    private GameObject lastSpawnedProjectile = null;
    private float lastSpawnedTime = -999f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (onAttackEvent == null) onAttackEvent = new UnityEvent();
    }

    void Start()
    {
        if (anim != null) anim.Play("Fly");

        // criar pools iniciais se prefabs definidos (defensivo)
        if (powerPrefab != null) PoolManager.CreatePool(powerPrefab, Mathf.Max(1, projectilePoolSize));
        if (superProjectilePrefab != null) PoolManager.CreatePool(superProjectilePrefab, Mathf.Max(1, projectilePoolSize));

        // tentar criar pools para hitVfx contidos nos prefabs (se houver)
        TryCreateVFXPoolFromPrefab(powerPrefab);
        TryCreateVFXPoolFromPrefab(superProjectilePrefab);
    }

    void OnEnable()
    {
        // reset quando o objeto for reativado (útil em respawn)
        lastAttackTime = -999f;
        lastSpawnedProjectile = null;
        lastSpawnedTime = -999f;
    }

    void Update()
    {
        // input: apenas dispara a animação se cooldown já passou (o cooldown real é validado em SpawnPower)
        if (Input.GetKeyDown(attackKey) && Time.time >= lastAttackTime + attackCooldown)
        {
            if (anim != null) anim.SetTrigger("Attack");
        }

        // direção baseada na escala local
        facing = transform.localScale.x >= 0f ? 1 : -1;
    }

    /// <summary>
    /// Chamado por AnimationEvent ou manualmente para quando o ataque deve acontecer.
    /// </summary>
    public void OnAttackEvent()
    {
        SpawnPower();

        if (attackSfx != null)
            AudioSource.PlayClipAtPoint(attackSfx, transform.position, attackSfxVolume);

        if (onAttackEvent != null)
            onAttackEvent.Invoke();
    }

    /// <summary>
    /// Força o spawn imediato (pode ser usado por outros scripts).
    /// </summary>
    public void ForceSpawnPower()
    {
        SpawnPower();
    }

    /// <summary>
    /// Spawn do projétil com guards para evitar double-spawn e usa pool quando disponível.
    /// </summary>
    public void SpawnPower()
    {
        // principal cooldown check (aplicado no momento do spawn)
        if (Time.time < lastAttackTime + attackCooldown)
        {
            // Debug.Log("SpawnPower ignorado: cooldown ativo.");
            return;
        }

        // defesa adicional: se já spawnamos um projétil que ainda está ativo e foi spawnado há pouco, ignorar
        if (lastSpawnedProjectile != null && lastSpawnedProjectile.activeInHierarchy)
        {
            if (Time.time < lastSpawnedTime + duplicateGuardTime)
            {
                // Debug.Log("SpawnPower ignorado: duplicate guard");
                return;
            }
        }

        if (powerPrefab == null || firePoint == null)
        {
            Debug.LogWarning("SpawnPower: powerPrefab ou firePoint não atribuído.");
            return;
        }

        // decide se utilizar super prefab
        var pc = GetComponent<PlayerCombat>();
        bool hasSuperFlag = (pc != null) ? pc.hasSuperPower : false;

        bool shouldBeSuper;
        if (requirePlayerActiveSuper)
            shouldBeSuper = (pc != null) && pc.hasSuperPower;
        else
            shouldBeSuper = hasSuperFlag || prefabIsSuper;

        GameObject prefabToUse = powerPrefab;
        if (shouldBeSuper && superProjectilePrefab != null) prefabToUse = superProjectilePrefab;

        // pega do pool se disponível, senão instancia
        GameObject p = null;
        if (PoolManager.HasPoolFor(prefabToUse))
        {
            p = PoolManager.Get(prefabToUse, firePoint.position, Quaternion.identity);
        }
        else
        {
            p = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);
        }

        if (p == null)
        {
            Debug.LogWarning("SpawnPower: falha ao obter instancia do prefab.");
            return;
        }

        // registra momento do spawn (cooldown + duplicate guard)
        lastAttackTime = Time.time;
        lastSpawnedProjectile = p;
        lastSpawnedTime = Time.time;

        // ajustar escala/direção
        Vector3 s = p.transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        p.transform.localScale = s;

        // garantir sprite ativo e alpha = 1
        SpriteRenderer sr = p.GetComponent<SpriteRenderer>() ?? p.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            Color c = sr.color; c.a = 1f; sr.color = c;
            if (debugForceSortingOrder >= 0) sr.sortingOrder = debugForceSortingOrder;
        }

        // configurar Projectile se existir
        var proj = p.GetComponent<Projectile>() ?? p.GetComponentInChildren<Projectile>();
        if (proj != null)
        {
            // calcula dano baseado no PlayerCombat, se existir
            int damageToSet = proj.damage;
            if (pc != null)
            {
                damageToSet = shouldBeSuper ? pc.superDamage : pc.normalDamage;
            }

            proj.SetAsSuper(shouldBeSuper);
            proj.SetDamage(damageToSet);
            proj.SetDirection(facing);
            proj.Launch();
        }
        else
        {
            // fallback: só aplica velocidade no Rigidbody2D
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(facing * powerSpeed, rb.linearVelocity.y);
            }
        }
    }

    /// <summary>
    /// Tenta ler o prefab e criar pool para o hitVfx que o Projectile referencia (se existir).
    /// Chamado no Start.
    /// </summary>
    void TryCreateVFXPoolFromPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        // se o prefab for um asset prefab, GetComponent funciona; se for null, sai.
        var proj = prefab.GetComponent<Projectile>() ?? prefab.GetComponentInChildren<Projectile>();
        if (proj != null && proj.hitVfx != null)
        {
            PoolManager.CreatePool(proj.hitVfx, Mathf.Max(1, vfxPoolSize));
        }
    }
}
