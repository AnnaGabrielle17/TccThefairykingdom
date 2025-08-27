using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoScreen : MonoBehaviour
{
    public float delay = 3f; // tempo em segundos
    public string nextScene = "Menu"; // nome da próxima cena

    void Start()
    {
        Invoke("LoadNextScene", delay);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
