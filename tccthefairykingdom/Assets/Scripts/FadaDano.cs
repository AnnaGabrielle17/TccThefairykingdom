using UnityEngine;
using System.Collections;

public class FadaDano : MonoBehaviour
{
    [Header("Vida")]
    public int maxVida = 6;
    [Tooltip("-1 = inicializar automaticamente com maxVida")]
    public int vida = -1;

    [Header("Dano")]
    public float intervaloDano = 1f; // 1 segundo entre cada dano
    private float tempoUltimoDano;

    [Header("Referências")]
    private SpriteRenderer spriteRenderer;

    [Tooltip("Arraste o componente DebugFrameCycler que controla a Image da barra")]
    public DebugFrameCycler frameCycler;

    [Header("Comportamento visual")]
    [Tooltip("Se true: chama DecreaseOne() passo-a-passo com pequeno delay (efeito visual)")]
    public bool usarStepDecreaseVisivel = true;
    [Tooltip("Delay entre cada DecreaseOne quando usarStepDecreaseVisivel=true")]
    public float delayEntrePassos = 0.05f;

    [Header("Debug / Teste")]
    public bool enableDebugKeys = true; // pressionar K/L para testar em runtime

    // controle de corrotina/estado para evitar race conditions
    private Coroutine currentVisualCoroutine = null;
    private bool isAnimating = false;

    // ----------------- ESCUDO -----------------
    [Header("Escudo (opcional)")]
    [Tooltip("GameObject filho que representa a 'frame' visual do escudo. Se vazio, o escudo ainda funciona sem visual.")]
    public GameObject shieldFrame;

    [Tooltip("Duração padrão do escudo em segundos. Use 0 para não expirar por tempo.")]
    public float defaultShieldDuration = 5f;

    [Tooltip("Número de hits que o escudo aguenta. Use int.MaxValue para 'infinito' (apenas duração).")]
    public int defaultShieldHits = int.MaxValue;

    [Tooltip("Prefab de efeito quando o escudo bloqueia (opcional).")]
    public GameObject shieldBlockEffectPrefab;

    private bool shieldActive = false;
    private int shieldHitsRemaining = 0;
    private Coroutine shieldCoroutine = null;
    // -------------------------------------------

    // coroutine de piscar (guardamos para poder parar se necessário)
    private Coroutine piscarCoroutine = null;

    public void ApplyDOT(int damagePerTick, float duration, float tickInterval = 1f)
    {
        StartCoroutine(ApplyDOTCoroutine(damagePerTick, duration, tickInterval));
    }

    private IEnumerator ApplyDOTCoroutine(int dmg, float duration, float tickInterval)
    {
        float elapsed = 0f;
        while (elapsed < duration && vida > 0)
        {
            TomarDano(dmg); // TomarDano ignora o intervaloDano, então DOT sempre aplica
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }

    private void Awake()
    {
        if (vida < 0) vida = maxVida;
        vida = Mathf.Clamp(vida, 0, maxVida);
        spriteRenderer = GetComponent<SpriteRenderer>();

        // se houver shieldFrame, garanta que esteja inativo no início
        if (shieldFrame != null) shieldFrame.SetActive(false);
    }

    private void Start()
    {
        // tentativa automática de encontrar frameCycler se não atribuído
        if (frameCycler == null)
        {
            frameCycler = FindObjectOfType<DebugFrameCycler>();
        }

        AtualizarBarraVisualInstantanea();
    }

    private void Update()
    {
        if (!enableDebugKeys) return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            TomarDano(1);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Curar(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (frameCycler != null) frameCycler.DecreaseOne();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (frameCycler != null) frameCycler.IncreaseOne();
        }

        // debug: pressionando P ativa um escudo de teste
        if (enableDebugKeys && Input.GetKeyDown(KeyCode.P))
        {
            AddShield(4f, 3); // exemplo: 4s ou 3 hits, o que ocorrer primeiro (aqui ambos setados)
            Debug.Log("[DEBUG] Escudo adicionado via tecla P");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Mecanica"))
        {
            TryTakeDamageFromExternal(1);
        }
    }

    // Método público centralizado para aplicar dano de fontes externas (mecânicas, partículas, triggers)
    // Agora respeita o escudo: se houver escudo, consome o hit e retorna sem aplicar dano.
    public void TryTakeDamageFromExternal(int quantidade)
    {
        // se estiver com escudo, bloqueia e consome hit (independente do intervalo)
        if (shieldActive)
        {
            ShieldHit();
            return;
        }

        // Sem escudo: respeitar intervalo de dano
        if (Time.time < tempoUltimoDano + intervaloDano) return;

        tempoUltimoDano = Time.time;
        TomarDano(quantidade);

        // inicia piscar salvando a coroutine para poder parar depois
        if (piscarCoroutine != null)
        {
            StopCoroutine(piscarCoroutine);
            piscarCoroutine = null;
        }
        piscarCoroutine = StartCoroutine(PiscarDano());
    }

    public void TomarDano(int quantidade)
    {
        if (quantidade <= 0) return;

        int vidaAntiga = vida;
        vida = Mathf.Clamp(vida - quantidade, 0, maxVida);

        int passos = Mathf.Abs(vidaAntiga - vida);

        if (passos > 0)
        {
            // cancela qualquer animação visual em progresso (cura/dano)
            if (currentVisualCoroutine != null)
            {
                StopCoroutine(currentVisualCoroutine);
                currentVisualCoroutine = null;
                isAnimating = false;
            }

            if (usarStepDecreaseVisivel && frameCycler != null)
            {
                currentVisualCoroutine = StartCoroutine(DecreaseStepsVisiveis(passos));
            }
            else
            {
                AtualizarBarraVisualInstantanea();
            }
        }

        Debug.Log("Fada levou dano! Vida: " + vida);

        if (vida <= 0) Morrer();
    }

    // --- Substituída: versão que caminha até o índice alvo (resolve problemas de sincronização) ---
    private IEnumerator DecreaseStepsVisiveis(int passos)
    {
        isAnimating = true;

        if (frameCycler == null || frameCycler.frames == null || frameCycler.frames.Length == 0)
        {
            isAnimating = false;
            yield break;
        }

        int last = frameCycler.frames.Length - 1;

        // índice alvo baseado no valor atual de 'vida'
        int targetIndex = Mathf.RoundToInt(last * ((float)vida / Mathf.Max(1, maxVida)));

        // índice atual do frameCycler (fonte da verdade visual)
        int currentIndex = frameCycler.GetIndex();

        // mover passo-a-passo até o índice alvo de forma determinística
        if (frameCycler.framesAreInverted)
        {
            // invertido: full = 0, empty = last
            while (currentIndex < targetIndex)
            {
                currentIndex++;
                frameCycler.SetIndex(currentIndex);
                yield return new WaitForSeconds(delayEntrePassos);
            }
            while (currentIndex > targetIndex)
            {
                currentIndex--;
                frameCycler.SetIndex(currentIndex);
                yield return new WaitForSeconds(delayEntrePassos);
            }
        }
        else
        {
            // padrão: 0 = vazio ... last = cheio
            while (currentIndex > targetIndex)
            {
                currentIndex--;
                frameCycler.SetIndex(currentIndex);
                yield return new WaitForSeconds(delayEntrePassos);
            }
            while (currentIndex < targetIndex)
            {
                currentIndex++;
                frameCycler.SetIndex(currentIndex);
                yield return new WaitForSeconds(delayEntrePassos);
            }
        }

        // garante que termine exatamente no alvo
        frameCycler.SetIndex(targetIndex);

        isAnimating = false;
        currentVisualCoroutine = null;
    }
    // --- fim DecreaseStepsVisiveis ---

    private void AtualizarBarraVisualInstantanea()
    {
        if (isAnimating) return;

        if (frameCycler == null)
        {
            Debug.LogWarning("[FadaDano] frameCycler == null em AtualizarBarraVisualInstantanea");
            return;
        }

        if (frameCycler.frames == null || frameCycler.frames.Length == 0)
        {
            Debug.LogWarning("[FadaDano] frameCycler.frames inválido em AtualizarBarraVisualInstantanea");
            return;
        }

        int last = frameCycler.frames.Length - 1;
        float fraction = (float)vida / Mathf.Max(1, maxVida);
        int index = Mathf.RoundToInt(last * fraction);
        frameCycler.SetIndex(index);
    }

    private void Morrer()
    {
        Debug.Log("Fada morreu!");

        // cancelar qualquer animação visual em andamento
        if (currentVisualCoroutine != null)
        {
            StopCoroutine(currentVisualCoroutine);
            currentVisualCoroutine = null;
        }

        // Desabilita componentes que controlam o jogador (se existirem)
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // desativa scripts de controle do jogador para evitar inputs pós-morte
        var behaviors = GetComponents<MonoBehaviour>();
        foreach (var b in behaviors)
        {
            // não desativa este script (FadaDano) para manter lógica, só desativa outros controles
            if (b != this) b.enabled = false;
        }

        // opcional: toca trigger de animação de morte
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            // crie um trigger "Die" no Animator se quiser animação de morte
            if (anim.HasState(0, Animator.StringToHash("Die")))
                anim.SetTrigger("Die");
        }

        // chama GameOverController (se existir)
        if (GameOverController.Instance != null)
        {
            GameOverController.Instance.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverController não encontrado na cena. Crie um GameObject com o script e aponte o painel.");
            // fallback: destrói o jogador (se realmente desejar)
            Destroy(gameObject);
        }
    }

    private IEnumerator PiscarDano()
    {
        // se já tiver escudo, não piscar
        if (shieldActive) 
        {
            piscarCoroutine = null;
            yield break;
        }
        if (spriteRenderer == null)
        {
            piscarCoroutine = null;
            yield break;
        }

        // executo o piscar: dois ciclos (como você tinha)
        for (int i = 0; i < 2; i++)
        {
            // se entre um ciclo e outro o escudo for ativado, para imediatamente
            if (shieldActive)
            {
                spriteRenderer.enabled = true;
                piscarCoroutine = null;
                yield break;
            }

            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);

            if (shieldActive)
            {
                spriteRenderer.enabled = true;
                piscarCoroutine = null;
                yield break;
            }

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        piscarCoroutine = null;
    }

    // Para garantir que possamos forçar o fim do piscar (p.ex. quando aplica escudo)
    private void StopPiscar()
    {
        if (piscarCoroutine != null)
        {
            StopCoroutine(piscarCoroutine);
            piscarCoroutine = null;
        }
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    // ----------------- MÉTODOS DO ESCUDO -----------------
    /// <summary>
    /// Aplica um escudo ao jogador.
    /// duration = 0 => não expira por tempo (usa somente hits)
    /// maxHits = int.MaxValue => não decrementa por hits (usa só duração)
    /// </summary>
    public void AddShield(float duration = -1f, int maxHits = -1)
    {
        float useDuration = (duration < 0f) ? defaultShieldDuration : duration;
        int useHits = (maxHits < 0) ? defaultShieldHits : maxHits;

        shieldActive = true;
        // interrompe qualquer piscar em andamento assim que escudo é aplicado
        StopPiscar();
        shieldHitsRemaining = useHits;

        if (shieldFrame != null)
        {
            shieldFrame.SetActive(true);
            Collider2D c = shieldFrame.GetComponent<Collider2D>();
            if (c != null) c.enabled = true;
        }

        // Reinicia coroutine de duração se houver
        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
        if (useDuration > 0f)
        {
            shieldCoroutine = StartCoroutine(ShieldTimer(useDuration));
        }
    }

    private IEnumerator ShieldTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RemoveShield();
    }

    /// <summary>
    /// Consumir um hit do escudo (chamado quando um projétil bate)
    /// </summary>
    public void ShieldHit()
    {
        if (!shieldActive) return;

        // spawn de efeito de bloqueio
        if (shieldBlockEffectPrefab != null)
        {
            Instantiate(shieldBlockEffectPrefab, transform.position, Quaternion.identity);
        }

        // NÃO iniciar piscar quando o escudo bloqueia (removido)
        // Se quiser feedback visual no shield, faça no shieldFrame/Animator ou prefab de efeito.

        if (shieldHitsRemaining > 0 && shieldHitsRemaining < int.MaxValue)
        {
            shieldHitsRemaining--;
            Debug.Log($"Escudo bloqueou um projétil. Hits restantes: {shieldHitsRemaining}");
            if (shieldHitsRemaining <= 0)
            {
                RemoveShield();
            }
        }
        else
        {
            // int.MaxValue ou 0 trata como 'não decrementar'
            Debug.Log("Escudo bloqueou um projétil (hits infinitos).");
        }
    }

    public void RemoveShield()
    {
        shieldActive = false;
        shieldHitsRemaining = 0;

        if (shieldFrame != null)
        {
            Collider2D c = shieldFrame.GetComponent<Collider2D>();
            if (c != null) c.enabled = false;
            shieldFrame.SetActive(false);
        }

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
    }

    public bool IsShielded()
    {
        return shieldActive;
    }
    // ----------------- FIM ESCUDO -----------------

    // Métodos públicos úteis (p.ex. curar)
    public void Curar(int quantidade)
    {
        if (quantidade <= 0) return;

        if (currentVisualCoroutine != null)
        {
            StopCoroutine(currentVisualCoroutine);
            currentVisualCoroutine = null;
            isAnimating = false;
        }

        int vidaAntiga = vida;
        vida = Mathf.Clamp(vida + quantidade, 0, maxVida);
        Debug.Log($"[FadaDano] Curar: vidaAntiga={vidaAntiga} => vida={vida}");
        AtualizarBarraVisualInstantanea();
    }

    // cura visível por passos (um-por-um)
    public void CurarVisivel(int quantidade, float delayEntrePassosLocal = 0.05f)
    {
        if (quantidade <= 0) return;

        if (currentVisualCoroutine != null)
        {
            StopCoroutine(currentVisualCoroutine);
            currentVisualCoroutine = null;
            isAnimating = false;
        }

        currentVisualCoroutine = StartCoroutine(CurarStepsVisiveis(quantidade, delayEntrePassosLocal));
    }

    private IEnumerator CurarStepsVisiveis(int quantidade, float delay)
    {
        isAnimating = true;

        for (int i = 0; i < quantidade; i++)
        {
            int novaVida = Mathf.Clamp(vida + 1, 0, maxVida);
            if (novaVida == vida) break;
            vida = novaVida;

            if (usarStepDecreaseVisivel && frameCycler != null)
            {
                frameCycler.IncreaseOne();
            }
            else
            {
                AtualizarBarraVisualInstantanea();
            }

            yield return new WaitForSeconds(delay);
        }

        AtualizarBarraVisualInstantanea();
        isAnimating = false;
        currentVisualCoroutine = null;
    }
}
