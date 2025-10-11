using UnityEngine;

public class NuvemController : MonoBehaviour
{
    public ParticleSystem particulas; // arraste aqui o Particle System no Inspector

    // Esse método será chamado pela animação
    public void LiberarParticulas()
    {
        if (particulas != null)
        {
            particulas.Play();
        }
    }
}
