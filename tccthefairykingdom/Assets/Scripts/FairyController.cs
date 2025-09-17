using UnityEngine;

public class FairyController : MonoBehaviour
{
     private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.Play("Fly"); // já começa voando
    }
}
