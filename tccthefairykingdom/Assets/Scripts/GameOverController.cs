using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverPanel;     // arraste aqui o GameOverPanel (o painel que contém título + botões)
    public CanvasGroup fadeCanvasGroup;  // opcional: um CanvasGroup para fazer fade; pode deixar nulo
    public float fadeDuration = 0.5f;

    [Header("Config")]
    public bool pauseTime = true;        // pausa o jogo quando o Game Over aparece
    public float delayBeforeShow = 0.05f; // pequeno delay opcional

    [Header("Menu")]
    public string menuSceneName = "MainMenu"; // nome da cena do menu (troque conforme seu projeto)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
    }

    // Chame GameOverController.Instance.ShowGameOver() quando o jogador morrer
    public void ShowGameOver()
    {
        StartCoroutine(DoShowGameOver());
    }

    IEnumerator DoShowGameOver()
    {
        // Use WaitForSecondsRealtime pois Time.timeScale pode ser 0
        yield return new WaitForSecondsRealtime(delayBeforeShow);

        if (pauseTime) Time.timeScale = 0f;

        // Pausar a música (mantendo posição) — ideal para Game Over
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.PauseMusic();
        // Se preferir fade out: ManterAMusica.instance.FadeOutAndStop(0.4f);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (fadeCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
    }

    // BOTÕES: ligar no Inspector -> OnClick()
    public void OnRetryButton()
    {
        // Antes de recarregar, resetar timeScale e decidir o que fazer com a música
        Time.timeScale = 1f;

        // Se quiser que a música recomece do início:
        if (ManterAMusica.instance != null)
        {
            ManterAMusica.instance.StopMusic();   // zera e para
            ManterAMusica.instance.FadeIn(0.25f, 1f); // opcional: iniciar com fade (se desejar)
            // Ou usar ResumeMusic() se preferir voltar do ponto pausado
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitToMenuButton()
    {
        Time.timeScale = 1f;

        // Normalmente queremos parar a música ao ir para o menu (caso o menu tenha sua própria música)
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.StopMusic();

        SceneManager.LoadScene(menuSceneName);
    }

    // caso queira esconder o painel e voltar ao jogo (útil em alguns fluxos)
    public void HideGameOverImmediate()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;

        Time.timeScale = 1f;

        // Retomar música (se preferir retomar)
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.ResumeMusic();
    }
}
