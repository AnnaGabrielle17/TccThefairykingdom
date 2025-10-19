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

    private void Awake()
    {
        if (vida < 0) vida = maxVida;
        vida = Mathf.Clamp(vida, 0, maxVida);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // sincroniza barra no início
        AtualizarBarraVisualInstantanea();
    }

    // agora usa o método centralizado TryTakeDamageFromExternal
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
            if (usarStepDecreaseVisivel && frameCycler != null)
            {
                StartCoroutine(DecreaseStepsVisiveis(passos));
            }
            else
            {
                AtualizarBarraVisualInstantanea();
            }
        }

        Debug.Log("Fada levou dano! Vida: " + vida);

        if (vida <= 0) Morrer();
    }

    private IEnumerator DecreaseStepsVisiveis(int passos)
    {
        for (int i = 0; i < passos; i++)
        {
            if (frameCycler != null) frameCycler.DecreaseOne();
            yield return new WaitForSeconds(delayEntrePassos);
        }
        AtualizarBarraVisualInstantanea();
    }

    private void AtualizarBarraVisualInstantanea()
    {
        if (frameCycler != null && frameCycler.frames != null && frameCycler.frames.Length > 0)
        {
            int last = frameCycler.frames.Length - 1;
            float fraction = (float)vida / Mathf.Max(1, maxVida); // 0..1
            int index = Mathf.RoundToInt(last * fraction);
            frameCycler.SetIndex(index);
        }
    }

    private void Morrer()
    {
        Debug.Log("Fada morreu!");
        // animação de morte, desativar, etc.
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
        vida = Mathf.Clamp(vida + quantidade, 0, maxVida);
        AtualizarBarraVisualInstantanea();
    }

    public void CurarVisivel(int quantidade, float delayEntrePassosLocal = 0.05f)
    {
        if (quantidade <= 0) return;
        StartCoroutine(CurarStepsVisiveis(quantidade, delayEntrePassosLocal));
    }

    private IEnumerator CurarStepsVisiveis(int quantidade, float delay)
    {
        for (int i = 0; i < quantidade; i++)
        {
            int novaVida = Mathf.Clamp(vida + 1, 0, maxVida);
            if (novaVida == vida) // já está cheio
                break;

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
    }
}
