using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CrystalPickup : MonoBehaviour
{
    public string nextSceneName = "";
    public float delayBeforeLoad = 1.2f;
    public float speed = 2f;
    public bool homingToPlayer = true;
    public float maxLifetime = 10f;

    [Header("Victory UI (auto-bind)")]
    public VictoryScreen victoryScreen; // será auto-encontrado se vazio

    private Transform player;
    private bool picked = false;

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;

        Destroy(gameObject, maxLifetime);

        if (victoryScreen == null)
        {
            victoryScreen = FindObjectOfType<VictoryScreen>();
            Debug.Log("CrystalPickup: auto-bind VictoryScreen -> " + (victoryScreen != null));
        }
        else
        {
            Debug.Log("CrystalPickup: VictoryScreen já atribuído no Inspector.");
        }
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
        Debug.Log("CrystalPickup: OnTriggerEnter2D with " + other.name + " tag=" + other.tag);

        if (picked) { Debug.Log("CrystalPickup: já pego, ignorando."); return; }
        if (!other.CompareTag("Player")) { Debug.Log("CrystalPickup: colisão não é Player, ignorando."); return; }

        picked = true;
        Debug.Log("CrystalPickup: player coletou cristal!");

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (victoryScreen != null)
        {
            Debug.Log("CrystalPickup: chamando VictoryScreen.ShowVictory()");
            victoryScreen.ShowVictory("Você pegou o cristal!");
            StartCoroutine(LoadSceneAfterRealtimeDelay());
        }
        else
        {
            Debug.LogWarning("CrystalPickup: victoryScreen é null, carregando cena direto.");
            if (!string.IsNullOrEmpty(nextSceneName))
                StartCoroutine(LoadSceneAfterDelay(nextSceneName));
            else
                StartCoroutine(LoadSceneAfterDelay(SceneManager.GetActiveScene().buildIndex + 1));
        }
    }

    IEnumerator LoadSceneAfterRealtimeDelay()
    {
        Debug.Log("CrystalPickup: esperando " + delayBeforeLoad + "s (Realtime) antes de carregar próxima cena.");
        yield return new WaitForSecondsRealtime(delayBeforeLoad);
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
