using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class CrystalPickup : MonoBehaviour
{[Tooltip("Nome da cena a carregar. Se vazio, carrega buildIndex + 1")]
    public string nextSceneName = "";
    public float delayBeforeLoad = 0.2f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // desativa visual e collider para evitar múltiplas ativações
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // aqui pode tocar som/efeito
        if (!string.IsNullOrEmpty(nextSceneName))
            StartCoroutine(LoadSceneAfterDelay(nextSceneName));
        else
            StartCoroutine(LoadSceneAfterDelay(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator LoadSceneAfterDelay(int buildIndex)
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        if (buildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(buildIndex);
        else
            Debug.Log("CrystalPickup: próxima cena não encontrada no Build Settings.");
    }
}
