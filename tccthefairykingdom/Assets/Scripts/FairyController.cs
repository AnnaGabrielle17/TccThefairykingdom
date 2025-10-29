using UnityEngine;

public class FairyController : MonoBehaviour
{
    public GameObject powerPrefab;    // arraste o prefab no inspector
    public Transform firePoint;       // arraste o FirePoint (child) no inspector
    public float powerSpeed = 8f;     // velocidade do projétil

    private Animator anim;
    private int facing = 1; // 1 = direita, -1 = esquerda

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.Play("Fly");
    }

    void Update()
    {
        // Exemplo: acionar ataque com W (só pra teste)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            anim.SetTrigger("Attack");
        }

        // Atualiza facing baseado na escala (se você virar a fada invertendo localScale.x)
        if (transform.localScale.x >= 0) facing = 1;
        else facing = -1;
    }

    // Chamado por Animation Event no clip de Attack (coloque o evento no frame exato)
    public void SpawnPower()
    {
        if (powerPrefab == null || firePoint == null) return;

        GameObject p = Instantiate(powerPrefab, firePoint.position, Quaternion.identity);

        // Se o prefab tem Rigidbody2D, definimos velocidade
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(facing * powerSpeed, 0f);
        }

        // Se o prefab tem um script Projectile, podemos informar direção nele:
        Projectile proj = p.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(facing);
        }

        // (Opcional) ajustar escala do projétil para "virar" quando disparado para esquerda:
        Vector3 s = p.transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        p.transform.localScale = s;
    }
}