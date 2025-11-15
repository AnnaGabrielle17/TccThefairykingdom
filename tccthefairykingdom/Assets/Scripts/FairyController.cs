using UnityEngine;
using UnityEngine.Events;

public class FairyController : MonoBehaviour
{
    [Header("Projéteis")]
    [Tooltip("Prefab do projétil normal")]
    public GameObject powerPrefab;
    [Tooltip("Prefab do projétil SUPER (opcional). Se não setado usa powerPrefab mesmo com power ativo.")]
    public GameObject superProjectilePrefab;
    public Transform firePoint;
    public float powerSpeed = 8f;
    [Tooltip("Se true, trata powerPrefab como já sendo super (fallback)")]
    public bool prefabIsSuper = false;

    [Header("Ataque / Input")]
    public KeyCode attackKey = KeyCode.K;
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

    private Animator anim;
    private int facing = 1; // 1 = direita, -1 = esquerda

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null) anim.Play("Fly");
        if (onAttackEvent == null) onAttackEvent = new UnityEvent();
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= lastAttackTime + attackCooldown)
        {
            if (anim != null) anim.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }

        facing = transform.localScale.x >= 0f ? 1 : -1;
    }

    public void OnAttackEvent()
    {
        SpawnPower();

        if (attackSfx != null)
            AudioSource.PlayClipAtPoint(attackSfx, transform.position, attackSfxVolume);

        if (onAttackEvent != null)
            onAttackEvent.Invoke();
    }

    public void SpawnPower()
    {
        if (powerPrefab == null || firePoint == null)
        {
            Debug.LogWarning("SpawnPower: powerPrefab ou firePoint não atribuído.");
            return;
        }

        var pc = GetComponent<PlayerCombat>();
        // adapta para a versão simplificada do PlayerCombat (usa hasSuperPower)
        bool hasSuperFlag = (pc != null) ? pc.hasSuperPower : false;

        // decide se deve ser super
        bool shouldBeSuper;
        if (requirePlayerActiveSuper)
        {
            shouldBeSuper = (pc != null) && pc.hasSuperPower;
        }
        else
        {
            // fallback antigo mantido: considerar prefabIsSuper ou flag do player
            shouldBeSuper = hasSuperFlag || prefabIsSuper;
        }

        GameObject prefabToUse = powerPrefab;
        if (shouldBeSuper && superProjectilePrefab != null) prefabToUse = superProjectilePrefab;

        Debug.Log($"SpawnPower: escolhendo prefab. hasSuperFlag={hasSuperFlag}, prefabIsSuper={prefabIsSuper}, shouldBeSuper={shouldBeSuper}, superPrefab={(superProjectilePrefab!=null?superProjectilePrefab.name:"NULL")}, normalPrefab={(powerPrefab!=null?powerPrefab.name:"NULL")}");

        GameObject p = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);

        var allProj = p.GetComponents<Projectile>();
        Debug.Log($"SpawnPower: instanciado prefab '{prefabToUse.name}' -> Projectile components found: {allProj.Length}");
        for (int i = 0; i < allProj.Length; i++)
        {
            Debug.Log($"  proj[{i}] on gameObject '{allProj[i].gameObject.name}' initial isSuper={allProj[i].isSuper}");
        }

        Vector3 s = p.transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        p.transform.localScale = s;

        Debug.Log($"SpawnPower: instanciado prefab '{prefabToUse.name}' | shouldBeSuper={shouldBeSuper}");

        SpriteRenderer sr = p.GetComponent<SpriteRenderer>() ?? p.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            Color c = sr.color; c.a = 1f; sr.color = c;
            if (debugForceSortingOrder >= 0) sr.sortingOrder = debugForceSortingOrder;
            Debug.Log($"SpawnPower: SpriteRenderer encontrado em '{sr.gameObject.name}', sprite='{sr.sprite?.name ?? "null"}'");
        }
        else
        {
            Debug.LogWarning("SpawnPower: SpriteRenderer NÃO encontrado no prefab instanciado.");
        }

        var proj = p.GetComponent<Projectile>() ?? p.GetComponentInChildren<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(facing);

            // calcula dano baseado no PlayerCombat simplificado (normalDamage / superDamage)
            int damageToSet = proj.damage;
            if (pc != null)
            {
                damageToSet = shouldBeSuper ? pc.superDamage : pc.normalDamage;
            }

            Debug.Log($"SpawnPower: antes SetAsSuper -> proj.gameObject='{proj.gameObject.name}', proj.isSuper={proj.isSuper}, shouldBeSuper={shouldBeSuper}");
            proj.SetAsSuper(shouldBeSuper);
            proj.SetDamage(damageToSet);
            proj.Launch();
            Debug.Log($"SpawnPower: depois SetAsSuper -> proj.isSuper={proj.isSuper}, damage={damageToSet}");

            Debug.Log($"SpawnPower: Projectile script encontrado. isSuper={proj.isSuper}, damage={damageToSet}");
        }
        else
        {
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = new Vector2(facing * powerSpeed, 0f);
        }
    }

    public void ForceSpawnPower()
    {
        SpawnPower();
    }
}
