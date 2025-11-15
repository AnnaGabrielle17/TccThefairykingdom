using UnityEngine;
using UnityEngine.SceneManagement; // só se for usar Quit->Menu
using UnityEngine.EventSystems;   // para selecionar botão quando pausar

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

        // Pausa dos áudios (opcional)
        AudioListener.pause = true;

        // Mostrar UI
        if (pausePanel != null) pausePanel.SetActive(true);

        // Selecionar botão (útil para controle por teclado/controle)
        if (firstSelectedOnPause != null)
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

        // Retoma dos áudios
        AudioListener.pause = false;

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
        AudioListener.pause = false;

        // Carrega a cena de menu (pode usar um nome ou build index)
        SceneManager.LoadScene(menuSceneName);
    }

    // Método que você pode ligar ao botão "Sair do Jogo" para fechar o jogo
    public void QuitGame()
    {
        // Em build:
        Application.Quit();

        // No editor:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}