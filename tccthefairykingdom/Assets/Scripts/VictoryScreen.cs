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

    public void ShowVictory(string message = "Você venceu!")
    {
        Debug.Log("VictoryScreen: ShowVictory called with message: " + message);

        if (panel != null) panel.SetActive(true);

        if (titleText != null) titleText.text = message;

        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        int lastIndex = SceneManager.sceneCountInBuildSettings - 1;
        if (btnNext != null) btnNext.interactable = (buildIndex < lastIndex);

        Time.timeScale = 0f; // pausa o jogo
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnNext()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.LogWarning("VictoryScreen: não há próxima cena no Build Settings.");
    }

    void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
