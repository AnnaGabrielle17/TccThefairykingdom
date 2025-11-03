using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class CrystalPickup : MonoBehaviour
{
    [Tooltip("Nome da cena a carregar. Se vazio, carrega buildIndex + 1")]
    public string nextSceneName = "";
    public float delayBeforeLoad = 0.2f;

    [Header("Movimento")]
    [Tooltip("Velocidade de movimento (unidades por segundo)")]
    public float speed = 2f;
    [Tooltip("Se verdadeiro, o cristal persegue o Player. Se falso, anda só para a esquerda.")]
    public bool homingToPlayer = true;
    [Tooltip("Tempo máximo em segundos antes do cristal se autodestruir (fallback)")]
    public float maxLifetime = 10f;

    private Transform player;

    void Start()
    {
        // tenta achar o Player pela Tag (garanta que o Player tem Tag = \"Player\")
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;

        // fallback: destrói sozinho pra não acumular objetos
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        // Se não tem player e homing está ligado, apenas anda para a esquerda
        if (player == null)
        {
            if (!homingToPlayer)
                transform.position += Vector3.left * speed * Time.deltaTime;
            return;
        }

        if (homingToPlayer)
        {
            // movimento suave em direção à posição atual do player
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else
        {
            // movimento fixo para a esquerda
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // desativa visual e collider para evitar múltiplas ativações
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // load scene
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