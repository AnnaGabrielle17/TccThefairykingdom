using UnityEngine;

public class FairyMovement : MonoBehaviour
{
   public float speed = 3f; // velocidade da fada

    void Update()
    {
        // Pega apenas o eixo vertical (W/S ou setas ↑ ↓)
        float moveY = Input.GetAxis("Vertical");

        // Cria o vetor de movimento só no Y
        Vector3 movement = new Vector3(0, moveY, 0);

        // Aplica a movimentação
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
