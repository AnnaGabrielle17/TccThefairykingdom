using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    // registro de todas as instâncias para distribuir stopX/stopY
    private static List<BirdController> allInstances = new List<BirdController>();

    [Header("Refs")]
    public Transform player;
    private BirdShooter shooter;
    public Animator animator;
    public Collider2D bodyCollider;

    [Header("Movimento")]
    public float leftSpeed = 2f;
    public float verticalSpeed = 3f;
    public Transform stopPoint;           // se preenchido, usa stopPoint.position.x
    public float stopX = 0f;              // base stopX (quando stopPoint == null)
    public float stopTolerance = 0.02f;
    public bool followVerticalWhileMoving = true; // segue o player enquanto se move

    [Header("Auto-spacing (evitar empilhamento)")]
    public bool useAutoSpacing = true;    // se true, vai ajustar stopX automaticamente
    public float baseStopX = 0f;          // X base para distribuir (usado se stopPoint == null)
    public float stopSpacing = 0.8f;      // espaçamento entre pássaros em X
    public float baseStopY = 0f;          // Y base para distribuir stopY (quando stopPoint == null)
    public float stopYSpacing = 0.6f;     // espaçamento vertical entre pássaros ao parar
    public int myIndex = -1;              // índice para debug

    [Header("Desincronização inicial")]
    public float startXVariation = 0.25f;
    public float startYVariation = 0.6f;  // Aumentei para maior separação vertical inicial
    public float initialStartDelayMax = 0.25f; // delay antes de começar a se mover (desincroniza)

    [Header("Shooting")]
    public bool shootWhenStopped = true;
    public bool shootImmediatelyOnStop = true;
    public float initialShootingDelayMax = 0.3f; // delay randômico antes do primeiro tiro

    [Header("Vida")]
    public int maxPowers = 3; // numero de hits antes de morrer (ajuste no Inspector)

    // internal state
    private bool reachedStopX = false;
    private bool isDead = false;
    private Coroutine shootingCoroutine;
    private float startDelay = 0f;
    private bool movementEnabled = true;

    // target stop Y (quando reachedStopX == true we'll move to this Y)
    private float stopY;

    // vida atual (contador de "poderes" que recebeu)
    private int currentPowers = 0;

    void Awake()
    {
        shooter = GetComponent<BirdShooter>();
        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }
        if (animator == null) animator = GetComponent<Animator>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();

        // registro — atribui índice determinístico baseado em Count
        allInstances.Add(this);
        myIndex = allInstances.Count - 1;
    }

    void Start()
    {
        // aplica pequena variação inicial na posição para evitar spawn idêntico
        float rx = Random.Range(-startXVariation, startXVariation);
        float ry = Random.Range(-startYVariation, startYVariation);
        transform.position = new Vector3(transform.position.x + rx, transform.position.y + ry, transform.position.z);
        Debug.Log($"[Bird:{name}] Start pos offset rx={rx:F2}, ry={ry:F2}");

        // compute stopX if using auto spacing
        if (useAutoSpacing)
        {
            float baseX = (baseStopX != 0f) ? baseStopX : (stopPoint != null ? stopPoint.position.x : stopX);
            myIndex = allInstances.IndexOf(this);
            stopX = baseX + myIndex * stopSpacing;
            Debug.Log($"[Bird:{name}] Auto spaced stopX assigned index={myIndex} stopX={stopX:F2}");
        }
        else if (stopPoint != null)
        {
            // se não usar auto spacing, usa exatamente stopPoint.x
            stopX = stopPoint.position.x;
            Debug.Log($"[Bird:{name}] Using stopPoint {stopPoint.name} at x={stopX:F2}");
        }

        // compute stopY automatically: se baseStopY == 0 usamos a Y atual como baseline para manter alturas relativas
        {
            float baseY = (baseStopY != 0f) ? baseStopY : transform.position.y;
            myIndex = allInstances.IndexOf(this);
            stopY = baseY + myIndex * stopYSpacing;
            Debug.Log($"[Bird:{name}] Auto spaced stopY assigned index={myIndex} stopY={stopY:F2}");
        }

        // desincronização do start para evitar todos se moverem no mesmo frame
        startDelay = Random.Range(0f, initialStartDelayMax);
        if (startDelay > 0f)
        {
            movementEnabled = false;
            StartCoroutine(EnableMovementAfterDelay(startDelay));
            Debug.Log($"[Bird:{name}] startDelay={startDelay:F2}s");
        }
    }

    IEnumerator EnableMovementAfterDelay(float d)
    {
        yield return new WaitForSeconds(d);
        movementEnabled = true;
    }

    void OnDestroy()
    {
        // remove do registro
        allInstances.Remove(this);
        RecomputeAutoSpacingForAll();
    }

    void Update()
    {
        if (isDead) return;
        if (!movementEnabled) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 pos = transform.position;

        if (!reachedStopX)
        {
            float newX = pos.x - leftSpeed * Time.deltaTime;
            float targetX = stopPoint != null ? stopPoint.position.x : stopX;

            if (Mathf.Abs(newX - targetX) <= stopTolerance || newX <= targetX)
            {
                newX = targetX;
                reachedStopX = true;
                Debug.Log($"[Bird:{name}] reached stopX at {newX:F2}");

                // quando parou, começa a rotina de tiro com pequeno delay randômico
                if (shootWhenStopped && shooter != null && shootingCoroutine == null)
                {
                    float sd = Random.Range(0f, initialShootingDelayMax);
                    shootingCoroutine = StartCoroutine(ShootingRoutine(sd));
                }
            }

            pos.x = newX;
        }

        // Vertical behaviour:
        if (!reachedStopX)
        {
            if (player != null && followVerticalWhileMoving)
            {
                float targetY = player.position.y;
                pos.y = Mathf.MoveTowards(pos.y, targetY, verticalSpeed * Time.deltaTime);
            }
        }
        else
        {
            pos.y = Mathf.MoveTowards(pos.y, stopY, verticalSpeed * Time.deltaTime);
        }

        transform.position = pos;
    }

    IEnumerator ShootingRoutine(float initialDelay)
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        if (shootImmediatelyOnStop && shooter != null)
        {
            shooter.ShootLeft();
        }

        while (!isDead && shooter != null && reachedStopX)
        {
            float wait = 0.5f;
            if (shooter != null) wait = Mathf.Max(0.01f, shooter.fireCooldown);
            yield return new WaitForSeconds(wait);
            shooter.ShootLeft();
        }

        shootingCoroutine = null;
    }

    // Expose for debug/force
    public void ForceShootNow()
    {
        if (shooter != null) shooter.ShootLeft();
    }

    // AddPower / Die actual implementation
    public void AddPower(int amount = 1)
    {
        if (isDead) return;

        currentPowers += amount;
        Debug.Log($"[Bird:{name}] AddPower({amount}) => currentPowers = {currentPowers}/{maxPowers}");

        if (animator != null) animator.SetTrigger("Hit");

        if (currentPowers >= maxPowers)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[Bird:{name}] Die() called.");

        // 1) destruir/retornar projéteis que esse pássaro criou (muito importante)
        if (shooter != null)
        {
            shooter.DestroyAllProjectiles();
        }

        // 2) stop shooting coroutine
        if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
        shootingCoroutine = null;

        // 3) desabilitar shooter (impede futuros shoots enquanto o GO ainda existir)
        if (shooter != null) shooter.enabled = false;

        // play animation/vfx
        if (animator != null) animator.SetTrigger("Die");

        // disable collider so further hits are ignored
        if (bodyCollider != null) bodyCollider.enabled = false;

        // optional: destroy after delay (mantive 1s como antes)
        Destroy(gameObject, 1.0f);
    }

    // -------------------
    // Collision handling (trigger + collision) -> unified handler
    // -------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other == null) return;

        Debug.Log($"[Bird:{name}] OnTriggerEnter2D hit {other.name} tag={other.tag}");
        HandleHitByObject(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (isDead) return;
        if (coll == null || coll.gameObject == null) return;

        Debug.Log($"[Bird:{name}] OnCollisionEnter2D hit {coll.gameObject.name} tag={coll.gameObject.tag}");
        HandleHitByObject(coll.gameObject);
    }

    // central handler that accepts GameObject (works for trigger or collision)
    private void HandleHitByObject(GameObject otherObj)
    {
        if (otherObj == null) return;

        // 1) detecta o script Projectile diretamente (mais robusto)
        var proj = otherObj.GetComponent<PoderPassaroProjectile>();
        if (proj != null)
        {
            Debug.Log($"[Bird:{name}] Hit by Projectile component -> applying AddPower and removing projectile");

            // se o projétil possui PooledObject, use ReturnToPool() em vez de Destroy
            var pooled = otherObj.GetComponent<PooledObject>();
            if (pooled != null)
            {
                try
                {
                    pooled.ReturnToPool();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Bird:{name}] pooled.ReturnToPool falhou: {e}. Fazendo Destroy fallback.");
                    DestroySafe(otherObj);
                }
            }
            else
            {
                DestroySafe(otherObj);
            }

            AddPower(1);
            return;
        }

        // 2) fallback por tag
        if (otherObj.CompareTag("PlayerPower"))
        {
            Debug.Log($"[Bird:{name}] Hit by object tagged PlayerPower -> AddPower and destroy");
            DestroySafe(otherObj);
            AddPower(1);
            return;
        }

        // 3) fallback por componente via reflexão: procura componente chamado "Projectile"
        Component generic = otherObj.GetComponent("Projectile");
        if (generic != null)
        {
            Debug.Log($"[Bird:{name}] Hit by component named 'Projectile' (reflection) -> AddPower and destroy");
            DestroySafe(otherObj);
            AddPower(1);
            return;
        }

        // nada relevante encontrado
    }

    // Destroy helper that evita MissingReferenceException
    private void DestroySafe(GameObject go)
    {
        if (go == null) return;
        // tenta usar PooledObject if exists
        var pooled = go.GetComponent<PooledObject>();
        if (pooled != null)
        {
            try { pooled.ReturnToPool(); return; }
            catch { /* fallback */ }
        }
        try { Destroy(go); }
        catch (System.Exception e) { Debug.LogWarning($"[Bird:{name}] DestroySafe falhou: {e}"); }
    }

    // expõe se está morto (usado pelo projétil como checagem de segurança)
    public bool IsDead => isDead;

    // Recalcula índices e stop positions quando uma instância é removida
    private static void RecomputeAutoSpacingForAll()
    {
        for (int i = 0; i < allInstances.Count; i++)
        {
            var b = allInstances[i];
            b.myIndex = i;
            if (b.useAutoSpacing)
            {
                float baseX = (b.baseStopX != 0f) ? b.baseStopX : (b.stopPoint != null ? b.stopPoint.position.x : b.stopX);
                b.stopX = baseX + i * b.stopSpacing;

                float baseY = (b.baseStopY != 0f) ? b.baseStopY : b.transform.position.y;
                b.stopY = baseY + i * b.stopYSpacing;
            }
        }
    }
}
