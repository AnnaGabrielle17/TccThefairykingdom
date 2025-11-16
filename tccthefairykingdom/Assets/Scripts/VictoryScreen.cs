using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if TXT_MESH_PRO_EXISTS
using TMPro;
#endif

public class VictoryScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel; // VictoryPanel (desativado no Start/Awake)
    public Text titleText;   // usar se não usar TMP
    public Button btnNext;
    public Button btnRetry;
    public Button btnMenu;

#if TMP_PRESENT
    public TMPro.TextMeshProUGUI titleTMP; // opcional se usar TextMeshPro
#endif

    void Awake()
    {
        if (panel != null) panel.SetActive(false);

        if (btnNext != null) btnNext.onClick.AddListener(OnNext);
        if (btnRetry != null) btnRetry.onClick.AddListener(OnRetry);
        if (btnMenu != null) btnMenu.onClick.AddListener(OnMenu);
    }

    /// <summary>
    /// Mostra a tela de vitória com a mensagem e pausa o jogo.
    /// </summary>
    public void ShowVictory(string message = "Você venceu!")
    {
        if (panel != null) panel.SetActive(true);

        if (!string.IsNullOrEmpty(message))
        {
#if TMP_PRESENT
            if (titleTMP != null) titleTMP.text = message;
            else
#endif
            if (titleText != null) titleText.text = message;
        }

        // desativa o botão Next se estiver na última cena do Build Settings
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        int lastIndex = SceneManager.sceneCountInBuildSettings - 1;
        if (btnNext != null)
        {
            btnNext.interactable = (buildIndex < lastIndex);
            if (!btnNext.interactable)
            {
                // Se desejar, troque o texto para "Menu" manualmente no Inspector
            }
        }

        // Pausa o jogo; usaremos WaitForSecondsRealtime ao aguardar avanço automático.
        Time.timeScale = 0f;
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
        // substitua "MainMenu" pelo nome exato da sua cena de menu
        SceneManager.LoadScene("MainMenu");
    }
}
