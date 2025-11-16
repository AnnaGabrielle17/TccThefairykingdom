using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Cutscene
{
    public string title;
    [TextArea(2, 8)]
    public string[] lines;
    public Sprite background;
    public AudioClip music;
    public AudioClip typingSfx;
    public float lineDelay = 1f;
    public float endDelay = 0.8f;
}

[RequireComponent(typeof(AudioSource))]
public class CutscenePlayerAuto : MonoBehaviour
{
    [Header("UI (one canvas/UI for all cutscenes)")]
    public GameObject uiRoot;         // painel pai com a UI da cutscene (Canvas root)
    public Image backgroundImage;     // Image full-screen que receberá os sprites
    public TMP_Text dialogueText;     // TextMeshProUGUI onde o texto aparece

    [Header("Cutscenes")]
    public List<Cutscene> cutscenes = new List<Cutscene>();

    [Header("Typing")]
    public float lettersPerSecond = 60f;
    [Tooltip("Se true, clique revela todo o texto. Se não quer entrada, deixe false.")]
    public bool allowClickToReveal = false;

    [Header("Flow")]
    public bool autoStart = true;
    public int startCutsceneIndex = 0;

    [Header("Load next scene (quando todas as cutscenes terminarem)")]
    public bool useSceneName = false;
    public string sceneNameToLoad = "Level1";
    public int buildIndexToLoad = 1;

    AudioSource audioSource;
    Coroutine runningCoroutine = null;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    void Start()
    {
        if (autoStart && cutscenes != null && cutscenes.Count > 0)
        {
            PlayAllFrom(startCutsceneIndex);
        }
    }

    public void PlayAllFrom(int index)
    {
        if (cutscenes == null || cutscenes.Count == 0) return;
        index = Mathf.Clamp(index, 0, cutscenes.Count - 1);
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(PlaySequenceFrom(index));
    }

    IEnumerator PlaySequenceFrom(int index)
    {
        // preserva este GameObject (manager + audio + uiRoot) durante o carregamento
        DontDestroyOnLoad(gameObject);

        // ativa UI
        if (uiRoot != null) uiRoot.SetActive(true);

        for (int ci = index; ci < cutscenes.Count; ci++)
        {
            var cs = cutscenes[ci];

            // background
            if (backgroundImage != null)
            {
                if (cs.background != null)
                {
                    backgroundImage.sprite = cs.background;
                    backgroundImage.enabled = true;
                }
                else
                {
                    backgroundImage.enabled = false;
                }
            }

            // music
            if (cs.music != null)
            {
                audioSource.clip = cs.music;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }

            // se não tem linhas (ex: tela de instruções) só espera endDelay
            if (cs.lines == null || cs.lines.Length == 0)
            {
                if (dialogueText != null) dialogueText.text = "";
                yield return new WaitForSeconds(Mathf.Max(0.01f, cs.endDelay));
                continue;
            }

            // para cada linha
            for (int li = 0; li < cs.lines.Length; li++)
            {
                string fullLine = cs.lines[li] ?? "";

                if (dialogueText != null)
                {
                    dialogueText.text = fullLine;
                    dialogueText.ForceMeshUpdate();
                    int total = dialogueText.textInfo.characterCount;
                    dialogueText.maxVisibleCharacters = 0;

                    if (total == 0)
                    {
                        yield return new WaitForSeconds(Mathf.Max(0.01f, cs.lineDelay));
                        continue;
                    }

                    float delay = 1f / Mathf.Max(1f, lettersPerSecond);

                    for (int c = 0; c < total; c++)
                    {
                        dialogueText.maxVisibleCharacters = c + 1;

                        if (cs.typingSfx != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(cs.typingSfx);
                        }

                        float t = 0f;
                        while (t < delay)
                        {
                            if (allowClickToReveal)
                            {
                                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                                {
                                    dialogueText.maxVisibleCharacters = total;
                                    break;
                                }
                            }

                            t += Time.deltaTime;
                            yield return null;
                        }

                        if (dialogueText.maxVisibleCharacters == total) break;
                    }

                    yield return new WaitForSeconds(Mathf.Max(0.01f, cs.lineDelay));
                }
                else
                {
                    yield return new WaitForSeconds(Mathf.Max(0.01f, cs.lineDelay));
                }
            } // fim linhas

            // espera o endDelay antes da próxima cutscene
            yield return new WaitForSeconds(Mathf.Max(0.01f, cs.endDelay));
        } // fim cutscenes

        // não desative a UI local ainda — vamos carregar a cena nova em background.
        // decide qual cena carregar
        string nameToLoad = (useSceneName && !string.IsNullOrEmpty(sceneNameToLoad)) ? sceneNameToLoad : null;
        int indexToLoad = (!useSceneName) ? buildIndexToLoad : -1;

        AsyncOperation op;
        if (nameToLoad != null)
            op = SceneManager.LoadSceneAsync(nameToLoad);
        else
            op = SceneManager.LoadSceneAsync(indexToLoad);

        op.allowSceneActivation = false;

        // aguarda até carregar 90% (Unity fica em 0.9 até allowSceneActivation = true)
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // opcional: um frame extra
        yield return null;

        // finalmente ativa cena nova (troca instantânea sem mostrar "clear color" da nova cena antes de pronto)
        op.allowSceneActivation = true;

        // aguarda ativação completa
        while (!op.isDone)
        {
            yield return null;
        }

        // agora a nova cena está ativa — podemos destruir este manager/UI preservada
        // (certifique-se de que a nova cena tem sua própria UI/manager se necessário)
        Destroy(gameObject);
        yield break;
    }
}
