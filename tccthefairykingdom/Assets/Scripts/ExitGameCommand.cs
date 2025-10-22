using UnityEngine;

[CreateAssetMenu(menuName = "Commands/Exit Game Command")]
public class ExitGameCommand : CommandSO
{
    public override void Execute()
    {
        Debug.Log("Sair do Jogo (comando) chamada.");

#if UNITY_EDITOR
        // No editor: para simular o quit, paramos o play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
