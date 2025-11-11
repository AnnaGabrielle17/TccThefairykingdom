using UnityEngine;
using System.Collections;

public class IceSpikeController : MonoBehaviour
{
    [Header("Movement (left)")]
    public float leftSpeed = 1f;             // deslocamento constante para a esquerda

    [Header("Extensão vertical")]
    public float extendDistance = 1f;        // quanto sobe/ desce (em unidades)
    public float extendSpeed = 4f;          // velocidade da extensão/retração
    public float waitTime = 0.5f;           // tempo parado ao completar o movimento

    [Header("Comportamento")]
    public bool onlyActivateOnPlayer = true; // se true, aguarda player entrar na detection zone
    public bool startGoingUp = false;       // true = começa indo para cima (use false para começar indo para baixo)
    public Collider2D detectionZone;        // arraste aqui o BoxCollider2D (IsTrigger) usado para detectar o player

    // estado interno
    private Vector3 localStartPos;
    private Vector3 localTargetPos;
    private Coroutine loopCoroutine;
    private bool activated = false;

    void Start()
    {
        localStartPos = transform.localPosition;
        localTargetPos = localStartPos + (startGoingUp ? Vector3.up : Vector3.down) * extendDistance;

        // se não precisa esperar o player, ativa imediatamente
        if (!onlyActivateOnPlayer)
            Activate();
    }

    void Update()
    {
        // deslocamento constante para a esquerda (mundo)
        transform.Translate(Vector2.left * leftSpeed * Time.deltaTime, Space.World);
    }

    // chamado pela detection zone (ou por você manualmente)
    public void Activate()
    {
        if (activated) return;
        activated = true;
        loopCoroutine = StartCoroutine(ExtendRetractLoop());
    }

    IEnumerator ExtendRetractLoop()
    {
        bool toTarget = true;
        while (true)
        {
            Vector3 goal = toTarget ? localTargetPos : localStartPos;
            // move localPosition.y até o objetivo
            while (Mathf.Abs(transform.localPosition.y - goal.y) > 0.01f)
            {
                float newY = Mathf.MoveTowards(transform.localPosition.y, goal.y, extendSpeed * Time.deltaTime);
                Vector3 p = transform.localPosition;
                p.y = newY;
                transform.localPosition = p;
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
            toTarget = !toTarget;
        }
    }

    // se quiser parar:
    public void Deactivate()
    {
        if (loopCoroutine != null) StopCoroutine(loopCoroutine);
        loopCoroutine = null;
        activated = false;
    }

    // Se você preferir que a detection zone dispare este método diretamente:
    // public void OnPlayerEnterDetected() { Activate(); }

}
