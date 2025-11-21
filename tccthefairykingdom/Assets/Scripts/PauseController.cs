using UnityEngine;
using UnityEngine.EventSystems;   // para selecionar botão quando pausar
using UnityEngine.SceneManagement; // caso use Quit -> Menu

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;            // arrastar PausePanel aqui
    public GameObject firstSelectedOnPause;  // arrastar ResumeButton (opcional)

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;

        // Pausa do tempo do jogo
        Time.timeScale = 0f;

        // Pausa apenas da música controlada por ManterAMusica (recomendado)
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.PauseMusic();

        // Se quiser pausar todos os áudios (SFX também), poderia usar:
        // AudioListener.pause = true;
        // (mas normalmente preferimos apenas pausar a música)

        // Mostrar UI
        if (pausePanel != null) pausePanel.SetActive(true);

        // Selecionar botão (útil para teclado/controle)
        if (firstSelectedOnPause != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedOnPause);
        }

        // Mostrar cursor (opcional, para desktop)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        isPaused = false;

        // Retoma do tempo
        Time.timeScale = 1f;

        // Retoma apenas da música controlada por ManterAMusica
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.ResumeMusic();

        // Se você usou AudioListener.pause = true; então precisa:
        // AudioListener.pause = false;

        // Esconder UI
        if (pausePanel != null) pausePanel.SetActive(false);

        // Remover seleção
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Ocultar cursor se quiser (ajuste conforme sua lógica)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }

    // Método que você pode ligar ao botão "Sair" (Quit -> Menu)
    public void QuitToMenu(string menuSceneName)
    {
        // Certifique-se de setar timeScale antes de trocar de cena
        Time.timeScale = 1f;

        // Garantir música adequada ao mudar para o menu
        if (ManterAMusica.instance != null)
            ManterAMusica.instance.StopMusic(); // ou ResumeMusic() se o menu usa a mesma música

        // Carrega a cena de menu (pode usar um nome ou build index)
        SceneManager.LoadScene(menuSceneName);
    }

    // Método que você pode ligar ao botão "Sair do Jogo" para fechar o jogo
    public void QuitGame()
    {
        Time.timeScale = 1f;

        // Em build:
        Application.Quit();

        // No editor:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
