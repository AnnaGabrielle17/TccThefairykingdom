using UnityEngine;
using System.Collections;

public class OwlController : MonoBehaviour
{
   public enum State { MovingToStop, Hovering }

    [Header("Movimento horizontal (original)")]
    public float speed = 1.5f;
    public bool walkLeft = true;

    [Header("Ponto de parada (para entrar em Hover)")]
    public Transform stopPoint;    // arraste um Empty na cena ou deixe null para usar stopX
    public float stopX = 0f;       // usado se stopPoint == null
    public float stopTolerance = 0.05f; // margem para considerar que chegou

    [Header("Hover (movimento só vertical)")]
    public float verticalSpeed = 3f;      // velocidade máxima vertical (por segundo)
    public float maxVerticalOffset = 4f;  // limite de deslocamento a partir do stopY
    public bool invertVerticalWithPlayer = true; // se true, quando player sobe, coruja desce
    [Range(0f, 2f)] public float invertMultiplier = 1f;
    public float verticalSmooth = 8f;    // suavização (maior = menos lag)

    [Header("Tiro / Fire")]
    public GameObject projectilePrefab;  // prefab do projétil da coruja (EnergyBolt/Prefab)
    public Transform firePoint;
    public float fireCooldown = 2f;
    public bool useAutoFireLoop = false;  // se true dispara automaticamente
    [Header("Multiplicadores quando em Hover (aumentam alcance/velocidade)")]
   // public float projectileSpeedMultiplierHover = 1.6f;
   // public float projectileLifeMultiplierHover = 1.6f;

    [Header("Player")]
    public Transform playerTransform;    // se vazio, tenta encontrar tag "Player"

    [Header("Animator (opcional)")]
    public Animator animator; // se quiser disparar triggers/parametros como antes

    // estado interno
    State currentState = State.MovingToStop;
    float targetX;
    float stopY;
    float nextFireTime = 0f;
    Coroutine autoFireRoutine;

    void Awake()
    {
        // decide targetX
        targetX = (stopPoint != null) ? stopPoint.position.x : stopX;
        stopY = (stopPoint != null) ? stopPoint.position.y : transform.position.y;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (animator == null) animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (useAutoFireLoop) autoFireRoutine = StartCoroutine(AutoFireLoop());
    }

    void OnDisable()
    {
        if (autoFireRoutine != null) StopCoroutine(autoFireRoutine);
    }

    void Update()
    {
        if (currentState == State.MovingToStop)
        {
            MoveHorizontally();
        }
        else if (currentState == State.Hovering)
        {
            HoverVertical();
        }

        // manter parâmetro de animação parecido com o seu original
        if (animator != null)
        {
            animator.SetBool("isWalking", currentState == State.MovingToStop);
        }
    }

    void MoveHorizontally()
    {
        float dir = walkLeft ? -1f : 1f;
        transform.Translate(Vector2.right * dir * speed * Time.deltaTime);

        // detecta chegada
        float currentX = transform.position.x;
        if ((walkLeft && currentX <= targetX + stopTolerance) || (!walkLeft && currentX >= targetX - stopTolerance))
        {
            // tranca X exatamente e entra em hover
            Vector3 p = transform.position;
            p.x = targetX;
            transform.position = p;
            EnterHover();
        }
    }

    void EnterHover()
    {
        currentState = State.Hovering;
        stopY = (stopPoint != null) ? stopPoint.position.y : transform.position.y;

        // se estiver em auto-fire e não estava rodando, iniciar
        if (useAutoFireLoop && autoFireRoutine == null)
            autoFireRoutine = StartCoroutine(AutoFireLoop());
    }

    void HoverVertical()
    {
        if (playerTransform == null) return;

        float playerY = playerTransform.position.y;
        float delta = playerY - stopY;
        float desiredOffset = invertVerticalWithPlayer ? -delta * invertMultiplier : delta * invertMultiplier;
        desiredOffset = Mathf.Clamp(desiredOffset, -maxVerticalOffset, maxVerticalOffset);
        float desiredY = stopY + desiredOffset;

        // suaviza movimento vertical
        float currentY = transform.position.y;
        float lerpT = 1f - Mathf.Exp(-verticalSmooth * Time.deltaTime);
        float smoothedY = Mathf.Lerp(currentY, desiredY, lerpT);

        // limita deslocamento máximo por frame (velocidade)
        float maxDelta = verticalSpeed * Time.deltaTime;
        float finalY = Mathf.MoveTowards(currentY, smoothedY, maxDelta);

        transform.position = new Vector3(targetX, finalY, transform.position.z);
    }

    /// <summary>
    /// Método público que pode ser chamado por Animation Event (ex: "Fire").
    /// Também usado pela coroutine AutoFireLoop.
    /// </summary>
    public void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireCooldown;

        GameObject go = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // tenta encontrar um componente EnergyBolt e ajustar alcance/vida se estiver em hover
        var energyBolt = go.GetComponent<EnergyBolt>() ?? go.GetComponentInChildren<EnergyBolt>();
        if (energyBolt != null && currentState == State.Hovering)
        {
            //energyBolt.speed *= projectileSpeedMultiplierHover;
            //energyBolt.lifeTime *= projectileLifeMultiplierHover;
        }

        // definir direção: mira no player se tiver, senão usa facing
        Vector2 dir;
        if (playerTransform != null)
        {
            dir = (Vector2)(playerTransform.position - firePoint.position);
            if (dir.sqrMagnitude < 0.001f) dir = (walkLeft ? Vector2.left : Vector2.right);
        }
        else
        {
            dir = (walkLeft ? Vector2.left : Vector2.right);
        }

        // tenta setar direção em diferentes scripts possíveis (EnergyBolt ou Projectile)
        var settable = go.GetComponent<EnergyBolt>() as object;
        if (settable != null)
        {
            (settable as EnergyBolt).SetDirection(dir.normalized);
        }
        else
        {
            // se o prefab usar outro script (ex: Projectile com SetDirection(int)), tenta alguns fallbacks
            var projScript = go.GetComponent<Projectile>();
            if (projScript != null)
            {
                // se Projectile tiver SetDirection(int) com +/-1:
                try
                {
                    int side = (dir.x >= 0f) ? 1 : -1;
                    projScript.SetDirection(side);
                }
                catch { /* se não tiver SetDirection ou assinatura diferente, ignora */ }
            }
        }

        // opcional: trigger de animação do ataque (se tiver)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    IEnumerator AutoFireLoop()
    {
        while (true)
        {
            Fire();
            yield return new WaitForSeconds(fireCooldown);
        }
    }

    // utilitário para forçar hover manualmente (debug)
    public void ForceEnterHover()
    {
        targetX = transform.position.x;
        EnterHover();
    }
}