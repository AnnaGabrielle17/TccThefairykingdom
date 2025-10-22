using UnityEngine;

public class MenuPrincipalManager : MonoBehaviour
{
    [Header("Commands")]
    [SerializeField] private CommandSO jogarCommand;
    [SerializeField] private CommandSO sairCommand;

    // Chamados pelos botões do UI (OnClick)
    public void Jogar()
    {
        if (jogarCommand == null)
        {
            Debug.LogWarning("[MenuPrincipalManager] jogarCommand não atribuído.");
            return;
        }
        jogarCommand.Execute();
    }

    public void SairJogo()
    {
        if (sairCommand == null)
        {
            Debug.LogWarning("[MenuPrincipalManager] sairCommand não atribuído.");
            return;
        }
        sairCommand.Execute();
    }
}