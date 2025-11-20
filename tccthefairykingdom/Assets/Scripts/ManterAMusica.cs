using UnityEngine;

public class ManterAMusica : MonoBehaviour
{
    public static ManterAMusica instance;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
