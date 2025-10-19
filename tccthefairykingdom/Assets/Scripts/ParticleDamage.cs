using UnityEngine;
using System.Collections.Generic;

public class ParticleDamage : MonoBehaviour
{
    [Tooltip("Dano por partícula (multiplicado pelo número de partículas colididas neste frame).")]
    public int damagePerParticle = 1;

    ParticleSystem ps;
    List<ParticleCollisionEvent> collisionEvents;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("ParticleCollision com: " + other.name);
        // procura o componente FadaDano no objeto colidido
        FadaDano fada = other.GetComponent<FadaDano>();
        if (fada == null) return;

        int events = ParticlePhysicsExtensions.GetCollisionEvents(ps, other, collisionEvents);
        if (events <= 0) return;

        int totalDamage = damagePerParticle * events;

        // chama o método público que respeita cooldown e piscar
        fada.TryTakeDamageFromExternal(totalDamage);
    }
    
}

