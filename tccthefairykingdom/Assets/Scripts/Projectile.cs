using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float lifeTime = 3f;

    [Header("Damage")]
    public int damage = 1;

    [Header("Super settings (optional)")]
    public bool isSuper = false;
    public float superSpeedMultiplier = 1.25f;
    public float areaRadius = 0f;
    public GameObject hitVfx;
    public ParticleSystem superParticles;

    [Header("Enemy detection (layer)")]
    public LayerMask enemyLayer;

    Rigidbody2D rb;
    Collider2D col;
    int direction = 1; // 1 direita, -1 esquerda

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        Debug.Log($"Projectile.Awake on '{gameObject.name}': isSuper initial = {isSuper}");
    }

    void Start()
    {
        Debug.Log($"Projectile.Start on '{gameObject.name}': isSuper at Start = {isSuper}");
        ApplyInitialVelocity();
        Destroy(gameObject, lifeTime);
    }

    void ApplyInitialVelocity()
    {
        float finalSpeed = speed * (isSuper ? superSpeedMultiplier : 1f);
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * finalSpeed, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Inicializa o projétil em runtime: define direção, velocidade (atualiza campo speed)
    /// e configura o dano. Compatível com chamadas como Initialize(Vector2.right, 8f, 10f).
    /// </summary>
    public void Initialize(Vector2 directionVec, float spd, float dmg)
    {
        // define velocidade base
        this.speed = spd;

        // define dano (usa o método existente para arredondar)
        SetDamage(dmg);

        // define direção a partir do vetor (x >= 0 -> direita, else esquerda)
        int dir = directionVec.x >= 0f ? 1 : -1;
        SetDirection(dir);

        // aplica a velocidade imediatamente
        ApplyInitialVelocity();
    }

    public void SetDamage(float d)
    {
        damage = Mathf.RoundToInt(d);
    }

    public void SetDirection(int dir)
    {
        direction = dir >= 0 ? 1 : -1;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;

        ApplyInitialVelocity();
    }

    public void SetAsSuper(bool superMode)
    {
        isSuper = superMode;
        Debug.Log($"Projectile.SetAsSuper() on '{gameObject.name}': set isSuper={isSuper} (superMode={superMode})");
        if (superParticles != null)
        {
            if (isSuper) superParticles.Play();
            else superParticles.Stop();
        }
        ApplyInitialVelocity();
    }

    public void Launch()
    {
        ApplyInitialVelocity();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (other.CompareTag("Player")) return;

        bool isEnemy = ((1 << other.gameObject.layer) & enemyLayer.value) != 0;

        if (isEnemy)
        {
            var eh = other.GetComponent<EnemyHealth>() ?? other.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                ApplyDamageToComponent(eh, damage);
                HandleAOEAndDestroy();
                return;
            }

            var ef = other.GetComponent<EnemyFairy>() ?? other.GetComponentInParent<EnemyFairy>();
            if (ef != null)
            {
                ApplyDamageToComponent(ef, damage);
                HandleAOEAndDestroy();
                return;
            }

            other.gameObject.SendMessage("TakeDamage", (float)damage, SendMessageOptions.DontRequireReceiver);
            HandleAOEAndDestroy();
            return;
        }

        if (!other.isTrigger)
        {
            SpawnHitVFX();
            Destroy(gameObject);
        }
    }

    void HandleAOEAndDestroy()
    {
        if (isSuper && areaRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (h.gameObject == this.gameObject) continue;

                bool hIsEnemy = ((1 << h.gameObject.layer) & enemyLayer.value) != 0;
                if (!hIsEnemy) continue;

                var eh2 = h.GetComponent<EnemyHealth>() ?? h.GetComponentInParent<EnemyHealth>();
                if (eh2 != null)
                {
                    ApplyDamageToComponent(eh2, damage * 0.6f);
                    continue;
                }

                var ef2 = h.GetComponent<EnemyFairy>() ?? h.GetComponentInParent<EnemyFairy>();
                if (ef2 != null)
                {
                    ApplyDamageToComponent(ef2, damage * 0.6f);
                    continue;
                }

                h.gameObject.SendMessage("TakeDamage", damage * 0.6f, SendMessageOptions.DontRequireReceiver);
            }
        }

        SpawnHitVFX();
        Destroy(gameObject);
    }

    void SpawnHitVFX()
    {
        if (hitVfx != null) Instantiate(hitVfx, transform.position, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        if (isSuper && areaRadius > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, areaRadius);
        }
    }

    void ApplyDamageToComponent(Component comp, float amount)
    {
        if (comp == null) return;

        System.Type t = comp.GetType();

        MethodInfo mFloat = t.GetMethod("TakeDamage", new System.Type[] { typeof(float) });
        if (mFloat != null)
        {
            mFloat.Invoke(comp, new object[] { (float)amount });
            return;
        }

        MethodInfo mInt = t.GetMethod("TakeDamage", new System.Type[] { typeof(int) });
        if (mInt != null)
        {
            mInt.Invoke(comp, new object[] { Mathf.RoundToInt(amount) });
            return;
        }

        MethodInfo mDouble = t.GetMethod("TakeDamage", new System.Type[] { typeof(double) });
        if (mDouble != null)
        {
            mDouble.Invoke(comp, new object[] { (double)amount });
            return;
        }

        MethodInfo[] methods = t.GetMethods();
        foreach (var mi in methods)
        {
            if (mi.Name != "TakeDamage") continue;
            var ps = mi.GetParameters();
            if (ps.Length != 1) continue;
            var pType = ps[0].ParameterType;
            if (pType == typeof(float) || pType == typeof(int) || pType == typeof(double))
            {
                object arg = amount;
                if (pType == typeof(int)) arg = Mathf.RoundToInt(amount);
                else if (pType == typeof(double)) arg = (double)amount;
                mi.Invoke(comp, new object[] { arg });
                return;
            }
        }

        comp.gameObject.SendMessage("TakeDamage", (float)amount, SendMessageOptions.DontRequireReceiver);
    }
}
