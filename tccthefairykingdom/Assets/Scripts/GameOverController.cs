using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public void ShowGameOver()
    {
        StartCoroutine(DoShowGameOver());
    }

    IEnumerator DoShowGameOver()
    {
        yield return new WaitForSecondsRealtime(delayBeforeShow);

        if (pauseTime) Time.timeScale = 0f;

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitToMenuButton()
    {
        Time.timeScale = 1f;
        // Certifique-se de ter a cena do menu no Build Settings com o mesmo nome
        SceneManager.LoadScene(menuSceneName);
    }

    // caso queira esconder o painel e voltar ao jogo
    public void HideGameOverImmediate()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
        Time.timeScale = 1f;
    }
}

