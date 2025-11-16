using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CrystalPickup : MonoBehaviour
{
    [Tooltip("Nome da cena a carregar. Se vazio, carrega buildIndex + 1")]
    public string nextSceneName = "";
    public float delayBeforeLoad = 1.2f; // tempo em segundos mostrado antes de trocar (realtime)
    
    [Header("Movimento")]
    public float speed = 2f;
    public bool homingToPlayer = true;
    public float maxLifetime = 10f;

    [Header("Victory UI")]
    public VictoryScreen victoryScreen; // arraste aqui seu VictoryScreen (opcional)

    private Transform player;
    private bool picked = false;

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;

        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        if (player == null)
        {
            if (!homingToPlayer)
                transform.position += Vector3.left * speed * Time.deltaTime;
            return;
        }

        if (homingToPlayer)
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        else
            transform.position += Vector3.left * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;
        if (!other.CompareTag("Player")) return;

        picked = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Se VictoryScreen definido, mostra a tela e depois avança (usando tempo real)
        if (victoryScreen != null)
        {
            victoryScreen.ShowVictory("Você pegou o cristal!");
            StartCoroutine(LoadSceneAfterRealtimeDelay());
        }
        else
        {
            // fallback para o comportamento antigo (sem UI)
            if (!string.IsNullOrEmpty(nextSceneName))
                StartCoroutine(LoadSceneAfterDelay(nextSceneName));
            else
                StartCoroutine(LoadSceneAfterDelay(SceneManager.GetActiveScene().buildIndex + 1));
        }
    }

    IEnumerator LoadSceneAfterRealtimeDelay()
    {
        // espera em tempo real (ignora Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(delayBeforeLoad);

        // restaura tempo antes de carregar
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextIndex);
            else
                Debug.Log("CrystalPickup: próxima cena não encontrada no Build Settings.");
        }
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
