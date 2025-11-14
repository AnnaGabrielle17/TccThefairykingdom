using UnityEngine;
using System.Collections;

public class PowerEffectAnimator : MonoBehaviour
{
    public Animator animator;
    public string parameterName = "Active"; // se usar bool para controlar; se não, usaremos Animator.Play
    public bool useBoolParameter = true;

    Coroutine lifeCoroutine;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    // Ativa o efeito por 'seconds' segundos. Se seconds <= 0 => ativa indefinidamente.
    public void Activate(float seconds)
    {
        // Se já existe rotina, reinicia
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);

        if (useBoolParameter && !string.IsNullOrEmpty(parameterName))
        {
            animator.SetBool(parameterName, true);
        }
        else
        {
            // garante que a clip seja reproduzida
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);
        }

        if (seconds > 0f)
            lifeCoroutine = StartCoroutine(DeactivateAfter(seconds));
    }

    IEnumerator DeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (useBoolParameter && !string.IsNullOrEmpty(parameterName))
        {
            animator.SetBool(parameterName, false);
        }
        else
        {
            // opcional: fade out ou parar partículas antes de destruir
        }

        // destruir o objeto / ou desativar
        Destroy(gameObject, 0.2f);
    }

    // força desativação imediata
    public void DeactivateImmediate()
    {
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        if (useBoolParameter && !string.IsNullOrEmpty(parameterName))
        {
            animator.SetBool(parameterName, false);
        }
        Destroy(gameObject, 0.1f);
    }

}
