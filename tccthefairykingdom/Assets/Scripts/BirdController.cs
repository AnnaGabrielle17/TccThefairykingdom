using UnityEngine;
using System.Collections;

public class BirdController : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;                       // arraste o Player (ou deixe vazio para buscar por tag "Player")
    private BirdShooter shooter;
    public Animator animator;                      // opcional: para triggers "Hit" e "Die"
    public Collider2D bodyCollider;                // opcional: para desabilitar colisões ao morrer

    [Header("Movimento")]
    public float leftSpeed = 2f;                   // velocidade no eixo X (sempre para a esquerda)
    public float verticalSpeed = 3f;               // velocidade de ajuste no Y
    public float stopX = 0f;                       // coordenada X onde o pássaro pára (world X)
    public float stopTolerance = 0.02f;            // margem ao comparar com stopX
    public bool followVerticalWhileMoving = true;  // se true, já segue vertical enquanto vai para a esquerda

    private bool reachedStopX = false;
    private bool isDead = false;

    [Header("Vida / Poderes")]
    public int maxPowers = 3;                      // ao chegar aqui, morre
    private int currentPowers = 0;

    [Header("Morte")]
    public float destroyDelay = 1.2f;              // tempo até destruir o GameObject após a morte
    public GameObject deathVFX;                    // efeito opcional ao morrer

    [Header("Disparo ao parar")]
    public bool shootWhenStopped = true;           // se true, começa a atirar ao chegar em stopX
    public float shootPollInterval = 0.12f;        // com que frequência a coroutine tenta atirar (segundos)
    public bool shootImmediatelyOnStop = true;     // dispara uma vez imediatamente ao alcançar stopX

    private Coroutine shootingCoroutine;

    private void Awake()
    {
        shooter = GetComponent<BirdShooter>();
        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (isDead) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 pos = transform.position;

        // movimento horizontal: vai para a esquerda até stopX
        if (!reachedStopX)
        {
            float newX = pos.x - leftSpeed * Time.deltaTime;
            if (newX <= stopX + stopTolerance)
            {
                newX = stopX;
                reachedStopX = true;

                // começa a rotina de disparo se configurado
                if (shootWhenStopped && shooter != null && shootingCoroutine == null)
                {
                    shootingCoroutine = StartCoroutine(ShootingRoutine());
                }
            }
            pos.x = newX;
        }

        // movimento vertical: segue o player dependendo das opções
        if (player != null && (reachedStopX || followVerticalWhileMoving))
        {
            float targetY = player.position.y;
            pos.y = Mathf.MoveTowards(pos.y, targetY, verticalSpeed * Time.deltaTime);
        }

        transform.position = pos;
    }

    private IEnumerator ShootingRoutine()
    {
        // dispara imediatamente se solicitado (ShootLeft fará a checagem de cooldown internamente)
        if (shootImmediatelyOnStop)
        {
            shooter.ShootLeft();
        }

        while (!isDead && shooter != null && reachedStopX)
        {
            shooter.ShootLeft();
            yield return new WaitForSeconds(Mathf.Max(0.01f, shootPollInterval));
        }

        shootingCoroutine = null;
    }

    /// <summary>
    /// Chame esse método quando o pássaro receber um "poder" do jogador.
    /// </summary>
    public void AddPower(int amount = 1)
    {
        if (isDead) return;

        currentPowers += amount;
        if (animator != null) animator.SetTrigger("Hit");

        if (currentPowers >= maxPowers)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // interrompe rotina de tiro
        if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
        shootingCoroutine = null;

        // impede que ele continue atirando
        if (shooter != null) shooter.enabled = false;

        // play animação e VFX
        if (animator != null) animator.SetTrigger("Die");
        if (deathVFX != null) Instantiate(deathVFX, transform.position, Quaternion.identity);

        // desativa colisões para não interagir mais
        if (bodyCollider != null) bodyCollider.enabled = false;

        // destrói após um tempo
        Destroy(gameObject, destroyDelay);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerPower"))
        {
            AddPower(1);
            Destroy(other.gameObject);
        }
    }

    private void OnDisable()
    {
        // garantir que coroutine seja parada ao desabilitar
        if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
        shootingCoroutine = null;
    }
}

