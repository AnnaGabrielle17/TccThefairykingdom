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

    private void Awake()
    {
        if (vida < 0) vida = maxVida;
        vida = Mathf.Clamp(vida, 0, maxVida);
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (Input.GetKeyDown(KeyCode.K))
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
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Mecanica"))
        {
            TryTakeDamageFromExternal(1);
        }
    }

    // Método público centralizado para aplicar dano de fontes externas (mecânicas, partículas, triggers)
    public void TryTakeDamageFromExternal(int quantidade)
    {
        if (Time.time < tempoUltimoDano + intervaloDano) return;

        tempoUltimoDano = Time.time;
        TomarDano(quantidade);
        StartCoroutine(PiscarDano());
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
        // se quiser: GameOverController.Instance.ShowGameOver();
    }

    private IEnumerator PiscarDano()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

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
   