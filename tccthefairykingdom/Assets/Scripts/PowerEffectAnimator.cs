using UnityEngine;
using System.Collections;

public class PowerEffectAnimator : MonoBehaviour
{
    public Animator animator;
    public string parameterName = "Active";
    public bool useBoolParameter = true;

    Coroutine lifeCoroutine;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        Debug.Log($"PowerEffectAnimator.Awake on '{gameObject.name}': animator={(animator!=null? animator.runtimeAnimatorController?.name : "NULL")}, useBoolParameter={useBoolParameter}, parameterName='{parameterName}'");
    }

    public void Activate(float seconds)
    {
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);

        Debug.Log($"PowerEffectAnimator.Activate on '{gameObject.name}' seconds={seconds}");

        if (animator == null)
        {
            Debug.LogWarning("PowerEffectAnimator: Animator não encontrado no prefab.");
        }

        if (useBoolParameter && !string.IsNullOrEmpty(parameterName) && animator != null)
        {
            animator.SetBool(parameterName, true);
        }
        else if (animator != null)
        {
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);
        }

        if (seconds > 0f)
            lifeCoroutine = StartCoroutine(DeactivateAfter(seconds));
    }

    IEnumerator DeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (useBoolParameter && !string.IsNullOrEmpty(parameterName) && animator != null)
        {
            animator.SetBool(parameterName, false);
        }

        Destroy(gameObject, 0.2f);
    }

    public void DeactivateImmediate()
    {
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        if (useBoolParameter && !string.IsNullOrEmpty(parameterName) && animator != null)
        {
            animator.SetBool(parameterName, false);
        }
        Destroy(gameObject, 0.1f);
    }
}
