using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PowerData {
    public string id = "superPower";           // identifica o power (usado para renovar)
    [Tooltip("Multiplicador de dano (ex: 1.5 = +50%)")]
    public float damageMultiplier = 1.5f;
    [Tooltip("Dano flat adicional (opcional)")]
    public float extraDamageFlat = 0f;
    [Tooltip("Duração em segundos (0 = permanente). Aqui vamos usar >0 (temporário).")]
    public float duration = 10f;
    [Tooltip("Se true, ativa uma mecânica extra (placeholder)")]
    public bool grantsExtraMechanic = false;
}

[System.Serializable]
public class ActivePower {
    public string id;
    public float damageMultiplier;
    public float extraDamageFlat;
    public float expiryTime; // Time.time quando expira; 0 = permanente
    public bool grantsExtraMechanic;
}

public class PlayerCombat : MonoBehaviour
{
    [Header("Dano")]
    public float baseDamage = 10f;
    public float currentDamage { get; private set; }

    [Header("Power Visual")]
    [Tooltip("Prefab do efeito visual do power (deve ter PowerEffectAnimator)")]
    public GameObject powerEffectPrefab;

    // lista de poderes ativos
    private List<ActivePower> activePowers = new List<ActivePower>();

    void Start()
    {
        RecalculateDamage();
    }

    void Update()
    {
        // Remove poderes expirados automaticamente
        if (activePowers.Count == 0) return;

        bool changed = false;
        float now = Time.time;
        for (int i = activePowers.Count - 1; i >= 0; i--)
        {
            if (activePowers[i].expiryTime > 0f && activePowers[i].expiryTime <= now)
            {
                // desativa mecânica extra (se aplicável)
                if (activePowers[i].grantsExtraMechanic)
                {
                    DisableExtraMechanic(activePowers[i]);
                }

                activePowers.RemoveAt(i);
                changed = true;

                // remover visual (caso exista) - opcional: destrói o filho PowerEffect_id
                Transform child = transform.Find("PowerEffect_" + activePowers[i].id);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        if (changed) RecalculateDamage();
    }

    /// <summary>
    /// Adiciona ou renova o power recebido. Se já existir um power com mesmo id, renova duração e atualiza valores.
    /// </summary>
    public void AddOrRefreshPower(PowerData pd)
    {
        if (pd == null)
        {
            Debug.LogWarning("AddOrRefreshPower recebeu null PowerData.");
            return;
        }

        float now = Time.time;
        float expiry = pd.duration > 0f ? now + pd.duration : 0f;

        // procurar power com mesmo id
        for (int i = 0; i < activePowers.Count; i++)
        {
            if (activePowers[i].id == pd.id)
            {
                // renovar e atualizar valores
                activePowers[i].damageMultiplier = pd.damageMultiplier;
                activePowers[i].extraDamageFlat = pd.extraDamageFlat;
                activePowers[i].expiryTime = expiry;
                activePowers[i].grantsExtraMechanic = pd.grantsExtraMechanic;

                // (Re)habilitar mecânica extra se necessário
                if (pd.grantsExtraMechanic) EnableExtraMechanic(activePowers[i]);

                // Recalcula dano e atualiza efeito visual (reinicia duração)
                RecalculateDamage();
                ActivateOrRefreshVisual(pd);
                return;
            }
        }

        // não existe -> adiciona novo
        ActivePower ap = new ActivePower()
        {
            id = pd.id,
            damageMultiplier = pd.damageMultiplier,
            extraDamageFlat = pd.extraDamageFlat,
            expiryTime = expiry,
            grantsExtraMechanic = pd.grantsExtraMechanic
        };

        activePowers.Add(ap);

        if (ap.grantsExtraMechanic) EnableExtraMechanic(ap);

        RecalculateDamage();
        ActivateOrRefreshVisual(pd);
    }

    /// <summary>
    /// Recalcula currentDamage com base nos powers ativos.
    /// Multiplicadores são aplicados multiplicativamente.
    /// </summary>
    void RecalculateDamage()
    {
        float multiplier = 1f;
        float flat = 0f;
        foreach (var p in activePowers)
        {
            multiplier *= p.damageMultiplier;
            flat += p.extraDamageFlat;
        }
        currentDamage = baseDamage * multiplier + flat;
        // Debug.Log($"Dano atual: {currentDamage}");
    }

    /// <summary>
    /// Ativa ou renova o efeito visual do power (procura filho "PowerEffect_<id>" ou instancia um novo prefab).
    /// </summary>
    void ActivateOrRefreshVisual(PowerData pd)
    {
        if (powerEffectPrefab == null) return;

        string childName = "PowerEffect_" + pd.id;
        Transform existing = transform.Find(childName);
        if (existing != null)
        {
            var pe = existing.GetComponent<PowerEffectAnimator>();
            if (pe != null) pe.Activate(pd.duration);
        }
        else
        {
            GameObject go = Instantiate(powerEffectPrefab, transform);
            go.name = childName;
            go.transform.localPosition = Vector3.zero; // ajuste o offset local se quiser (ex: new Vector3(0,0.2f,0))
            var pe = go.GetComponent<PowerEffectAnimator>();
            if (pe != null) pe.Activate(pd.duration);
        }
    }

    /// <summary>
    /// Habilita a mecânica extra (implemente aqui o que o power deve fazer além de aumentar dano).
    /// Ex.: habilitar double jump, tiros especiais, dash etc.
    /// </summary>
    void EnableExtraMechanic(ActivePower ap)
    {
        Debug.Log("EnableExtraMechanic: " + ap.id);
        // TODO: implemente a ação concreta, por exemplo:
        // var shooter = GetComponent<PlayerShooter>();
        // if (shooter != null) shooter.EnableExplosiveBullets();
    }

    /// <summary>
    /// Desabilita a mecânica extra quando o power expira.
    /// </summary>
    void DisableExtraMechanic(ActivePower ap)
    {
        Debug.Log("DisableExtraMechanic: " + ap.id);
        // TODO: implemente a desativação da mecânica extra
        // var shooter = GetComponent<PlayerShooter>();
        // if (shooter != null) shooter.DisableExplosiveBullets();
    }

    /// <summary>
    /// Exemplo de ataque: aplica damage calculado ao inimigo.
    /// Adapte para o seu sistema (projectiles, hitscan, etc).
    /// </summary>
    public void Attack(GameObject target)
{
    if (target == null) return;
    var ef = target.GetComponent<EnemyFairy>();
    if (ef != null) ef.TakeDamage(currentDamage);
}

    // --- Métodos auxiliares públicos (opcionais) ---

    /// <summary>
    /// Remove imediatamente um power pelo id (útil para debugging).
    /// </summary>
    public void RemovePowerById(string id)
    {
        for (int i = activePowers.Count - 1; i >= 0; i--)
        {
            if (activePowers[i].id == id)
            {
                if (activePowers[i].grantsExtraMechanic) DisableExtraMechanic(activePowers[i]);
                activePowers.RemoveAt(i);
            }
        }
        // remover visual se existir
        Transform child = transform.Find("PowerEffect_" + id);
        if (child != null) Destroy(child.gameObject);

        RecalculateDamage();
    }

    /// <summary>
    /// Limpa todos os poderes (útil em checkpoints / respawn).
    /// </summary>
    public void ClearAllPowers()
    {
        foreach (var p in activePowers)
        {
            if (p.grantsExtraMechanic) DisableExtraMechanic(p);
        }
        activePowers.Clear();

        // destruir visuais
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform c = transform.GetChild(i);
            if (c.name.StartsWith("PowerEffect_"))
                Destroy(c.gameObject);
        }

        RecalculateDamage();
    }
}