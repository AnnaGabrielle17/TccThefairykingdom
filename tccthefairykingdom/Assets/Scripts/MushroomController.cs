using UnityEngine;
using System.Collections;

public class MushroomController : MonoBehaviour
{
    [Header("Particle System (opcional)")]
    public ParticleSystem particulas; // arraste o Particle System (bolhas) aqui

    [Header("Prefab de bolha (opcional)")]
    public GameObject bubblePrefab;   // arraste o prefab da bolha aqui (se usar prefabs)
    public Transform bubbleSpawnPoint; // ponto de onde as bolhas saem; se nulo usa transform.position

    [Header("Burst settings")]
    public int burstCount = 8;             // quantas bolhas soltar no pulso
    public float prefabBurstInterval = 0.03f; // intervalo entre instancias (para efeito de jorro)
    public float prefabRadius = 0.12f;     // espalhamento inicial

    // --------- MÉTODOS PÚBLICOS QUE PODEM SER CHAMADOS PELA ANIMAÇÃO ----------
    // Chama o particle system (usa Emit para emitir instantaneamente burstCount partículas)
    public void LiberarParticulas()
    {
        if (particulas != null)
        {
            // Se quiser apenas tocar (se for loop) use particulas.Play();
            // Para um jorro instantâneo:
            particulas.Emit(burstCount);
        }
    }

    // Instancia prefabs de bolha em burst (mais controle por bolha)
    public void SpawnBubblePrefabBurst()
    {
        if (bubblePrefab == null) return;
        // garante que coroutine rode mesmo se Animation Event for chamado repetidamente
        StartCoroutine(SpawnBurstCoroutine());
    }

    IEnumerator SpawnBurstCoroutine()
    {
        // proteção contra destruir enquanto a coroutine roda
        if (bubblePrefab == null) yield break;

        Vector3 origin = bubbleSpawnPoint != null ? bubbleSpawnPoint.position : transform.position;

        for (int i = 0; i < burstCount; i++)
        {
            if (this == null || bubblePrefab == null) yield break; // segurança
            Vector2 offset = Random.insideUnitCircle * prefabRadius;
            Vector3 pos = origin + (Vector3)offset;

            GameObject go = Instantiate(bubblePrefab, pos, Quaternion.identity);
            // opcional: se o prefab tem um componente "Bubble" com Initialize, configura ele
            var b = go.GetComponent<Bubble>();
            if (b != null)
            {
                float spd = Random.Range(0.6f, 1.2f);
                float life = Random.Range(1.2f, 2.2f);
                Vector2 vel = new Vector2(Random.Range(-0.15f, 0.15f), spd);
                b.Initialize(vel, life);
            }

            yield return new WaitForSeconds(prefabBurstInterval);
        }
    }

    // Método auxiliar: chama tudo que você quiser (ParticleSystem + prefabs)
    // Você pode ligar esse único método na animação para executar ambos ao mesmo tempo
    public void LiberarTudo()
    {
        LiberarParticulas();
        SpawnBubblePrefabBurst();
    }

    // Garanta parar coroutines se o object for destruído (prevenção de erros)
    void OnDisable()
    {
        StopAllCoroutines();
    }
}

