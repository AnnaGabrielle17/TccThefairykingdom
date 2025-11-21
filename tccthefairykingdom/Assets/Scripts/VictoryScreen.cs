using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel; // atribua o painel que contém UI (pode começar desativado)
    public Text titleText;
    public Button btnNext;
    public Button btnRetry;
    public Button btnMenu;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);

        if (btnNext != null) btnNext.onClick.AddListener(OnNext);
        if (btnRetry != null) btnRetry.onClick.AddListener(OnRetry);
        if (btnMenu != null) btnMenu.onClick.AddListener(OnMenu);
    }

    void Start()
    {
        Debug.Log("VictoryScreen: Start() - panel assigned? " + (panel != null));
    }

    // Chame ShowVictory() ao completar o nível
    public void ShowVictory(string message = "Você venceu!")
    {
        Debug.Log("VictoryScreen: ShowVictory called with message: " + message);

        if (panel != null) panel.SetActive(true);

        if (titleText != null) titleText.text = message;

        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        int lastIndex = SceneManager.sceneCountInBuildSettings - 1;
        if (btnNext != null) btnNext.interactable = (buildIndex < lastIndex);

        // Pausar o jogo
        Time.timeScale = 0f;

        // Pausar (ou dar fade) na música
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.PauseMusic();
        // ou: ManterAMusica.instance.FadeOutAndStop(0.4f);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;

        // Retomar música se desejar
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.ResumeMusic();
    }

    void OnNext()
    {
        Time.timeScale = 1f;

        // Decida se quer manter a música (resume) ou reiniciar (stop then start)
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.ResumeMusic();

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.LogWarning("VictoryScreen: não há próxima cena no Build Settings.");
    }

    void OnRetry()
    {
        Time.timeScale = 1f;

        if (ManterAMusica.instance != null)
        {
            // reinicia a música ao tentar novamente
            ManterAMusica.instance.StopMusic();
            ManterAMusica.instance.FadeIn(0.25f, 1f); // opcional
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnMenu()
    {
        Time.timeScale = 1f;

        // Parar a música ao voltar para o menu (normalmente o menu usa outra trilha)
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.StopMusic();

        SceneManager.LoadScene("Menu");
    }
}
