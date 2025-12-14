// GameManager_Cutscene_Simple.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Cutscene_Simple : MonoBehaviour
{
    public static GameManager_Cutscene_Simple Instance;
    public GameObject cutscenePanel;        // painel que mostra sua imagem de cutscene
    public bool loadVictoryScene = true;
    public string victorySceneName = "VictoryScene";
    public GameObject victoryPanel;         // opcional se loadVictoryScene = false
    public float cutsceneDuration = 4f;
    public KeyCode skipKey = KeyCode.Space;
    public GameObject playerToDisable;
    public AudioSource cutsceneAudio;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnEnemyDeathWithDelay(float delay)
    {
        StartCoroutine(DelayedStart(delay));
    }

    IEnumerator DelayedStart(float d)
    {
        if (d > 0f) yield return new WaitForSeconds(d);
        yield return StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        if (cutscenePanel == null)
        {
            Debug.LogWarning("[GameManager] cutscenePanel não atribuído -> indo direto para vitória.");
            GoToVictory();
            yield break;
        }

        // garante que fique por cima
        cutscenePanel.transform.SetAsLastSibling();

        if (playerToDisable != null) playerToDisable.SetActive(false);
        cutscenePanel.SetActive(true);

        if (cutsceneAudio != null) cutsceneAudio.Play();

        float timer = 0f;
        while (timer < cutsceneDuration)
        {
            if (Input.GetKeyDown(skipKey)) break;
            timer += Time.deltaTime;
            yield return null;
        }

        if (cutsceneAudio != null && cutsceneAudio.isPlaying) cutsceneAudio.Stop();
        cutscenePanel.SetActive(false);
        if (playerToDisable != null) playerToDisable.SetActive(true);

        GoToVictory();
    }

    void GoToVictory()
    {
        if (loadVictoryScene)
        {
            if (!string.IsNullOrEmpty(victorySceneName))
                SceneManager.LoadScene(victorySceneName);
            else
                Debug.LogWarning("[GameManager] victorySceneName vazio.");
        }
        else
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            else Debug.LogWarning("[GameManager] victoryPanel não atribuído.");
        }
    }
}
