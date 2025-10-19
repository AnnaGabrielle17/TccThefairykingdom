using UnityEngine;

public class ParticleAreaTrigger : MonoBehaviour
{
    public int damageOnContact = 1;

    private void OnTriggerStay2D(Collider2D other)
    {
        FadaDano fada = other.GetComponent<FadaDano>();
        if (fada != null)
        {
            fada.TryTakeDamageFromExternal(damageOnContact);
        }
    }
}

