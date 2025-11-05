using UnityEngine;
using System.Collections;

public class BirdController_NormalDash : MonoBehaviour
{
   [Header("Movimento")]
    public float moveSpeed = 2f;            // velocidade constante para esquerda
    public float flapVelocity = 5f;         // velocidade vertical ao bater asas

    [Header("Dash / Poder")]
    public float dashSpeed = 14f;           // velocidade horizontal durante dash
    public float dashDuration = 0.18f;      // duração do dash
    public float dashCooldown = 1f;         // cooldown entre dashes
    public GameObject dashEffectPrefab;     // prefab do efeito (use o prefab que você criou)
    public Vector3 powerEffectOffset = new Vector3(-0.6f, 0f, 0f); // offset local onde o efeito aparece
    public bool spawnEffectAsWorld = true;  // true -> efeito instanciado sem parent (fica no mundo)

    [Header("Referências (opcionais)")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    // estado
    private bool isDashing = false;
    private bool canDash = true;
    private float direction = -1f; // -1 = esquerda

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) spriteRenderer.flipX = true; // ajusta se sua sprite precisa
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump")) Flap();
        if (Input.GetButtonDown("Fire1")) TryDash();

        // atualiza animator se tiver apenas parâmetro simples de voo
        if (animator != null)
        {
            bool hasIsFlying = HasAnimatorParameter("isFlying");
            if (hasIsFlying)
            {
                bool flying = Mathf.Abs(rb != null ? rb.linearVelocity.y : 0f) > 0.05f;
                animator.SetBool("isFlying", flying);
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (!isDashing) rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        else rb.linearVelocity = new Vector2(direction * dashSpeed, rb.linearVelocity.y);
    }

    public void Flap()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, flapVelocity);
        // se tiver trigger "flap" no animator, pode usar:
        // if (animator != null && HasAnimatorParameter("flap")) animator.SetTrigger("flap");
    }

    public void TryDash()
    {
        if (canDash) StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;

        // instancia o efeito visual (prefab simples com SpriteRenderer + script DashEffect)
        if (dashEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.TransformVector(powerEffectOffset);
            GameObject fx = Instantiate(dashEffectPrefab, spawnPos, Quaternion.identity);
            if (!spawnEffectAsWorld)
            {
                // opcional: manter o efeito como filho do pássaro para mover junto
                fx.transform.SetParent(transform, true);
                fx.transform.localPosition = powerEffectOffset;
            }
            // garanta que o sprite aponte para a esquerda (se necessário)
            SpriteRenderer fxSr = fx.GetComponent<SpriteRenderer>();
            if (fxSr != null)
            {
                fxSr.flipX = false; // ajuste se a sua sprite precisar ser invertida
            }
        }

        // redução temporária de gravidade para dash mais 'liso'
        float oldGravity = rb != null ? rb.gravityScale : 1f;
        if (rb != null) rb.gravityScale = 0f;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (rb != null) rb.gravityScale = oldGravity;
        isDashing = false;

        // espera cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters) if (p.name == paramName) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.TransformVector(powerEffectOffset), 0.05f);
    }

}
