using UnityEngine;

public class EnemyFairy : MonoBehaviour
{
   public float horizontalSpeed = 2f;  // velocidade horizontal
       public float verticalSpeed = 2f;    // velocidade de sobe/desce
       public float verticalRange = 1f;    // altura do movimento
       private float startY;
   
       void Start()
       {
           startY = transform.position.y; // salva a posição inicial no eixo Y
       }
   
       void Update()
       {
           // Movimento horizontal constante (para a esquerda)
           transform.Translate(Vector2.left * horizontalSpeed * Time.deltaTime);
   
           // Movimento vertical em onda (sobe e desce)
           float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
   
           // Atualiza posição
           transform.position = new Vector3(transform.position.x, newY, transform.position.z);
       }
   }

