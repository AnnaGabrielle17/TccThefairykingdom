using UnityEngine;
using System.Collections;


public class EnemyFairy : MonoBehaviour
{
    public Animator animator; // arraste o componente Animator aqui no Inspector
    private const string ANIM_ATTACK_BOOL = "isAttacking";

    void OnPlayerEnter()
    {
        // ... lógica existente ...
        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, true);
    }

    void OnPlayerExit()
    {
        // ... lógica existente ...
        if (animator != null) animator.SetBool(ANIM_ATTACK_BOOL, false);
    }
}