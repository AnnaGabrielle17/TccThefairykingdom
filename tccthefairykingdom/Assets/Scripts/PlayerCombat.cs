using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque Normal")]
    public GameObject normalPowerPrefab;
    public float normalPowerSpeed = 5f;
    public int normalDamage = 10;

    [Header("Ataque Super")]
    public GameObject superPowerPrefab;
    public int superDamage = 25;

    [Header("Power Collectável")]
    public bool hasSuperPower = false;
    public float powerDuration = 5f; 
    private Coroutine powerTimer;

    [Header("Spawn do disparo")]
    public Transform firePoint;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Chamado pela animação de ataque
    public void OnAttackEvent()
    {
        SpawnPower();
    }

    public void SpawnPower()
    {
        GameObject prefabToUse;
        int damageToUse;

        if (hasSuperPower)
        {
            prefabToUse = superPowerPrefab;
            damageToUse = superDamage;
        }
        else
        {
            prefabToUse = normalPowerPrefab;
            damageToUse = normalDamage;
        }

        if (prefabToUse == null)
        {
            Debug.LogError("Prefab do poder não atribuído!");
            return;
        }

        GameObject obj = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);

        Projectile proj = obj.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Initialize(Vector2.right, normalPowerSpeed, damageToUse);

            if (hasSuperPower)
                proj.SetAsSuper(true);
        }
        else
        {
            Debug.LogWarning("Projectile script não encontrado no prefab!");
        }
    }

    // Ativado quando o player pega o coletável de superpower
    public void AddOrRefreshPower()
    {
        hasSuperPower = true;

        if (powerTimer != null)
            StopCoroutine(powerTimer);

        powerTimer = StartCoroutine(PowerCountdown());
    }

    private IEnumerator PowerCountdown()
    {
        yield return new WaitForSeconds(powerDuration);
        hasSuperPower = false;
        powerTimer = null;
    }
}
