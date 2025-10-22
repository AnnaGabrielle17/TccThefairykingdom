using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Commands/Play Scene Command")]
public class PlaySceneCommand : CommandSO
{
    [SerializeField] private string sceneName;

    public override void Execute()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[PlaySceneCommand] sceneName está vazio.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // Opcional: exposicao de setter/getter para trocar em runtime
    public void SetSceneName(string name) => sceneName = name;
}
