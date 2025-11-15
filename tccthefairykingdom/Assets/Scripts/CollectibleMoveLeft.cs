using UnityEngine;

public class CollectibleMoveLeft : MonoBehaviour
{
    public float speed = 2f;

    private void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombat pc = collision.GetComponent<PlayerCombat>();

            if (pc != null)
            {
                pc.AddOrRefreshPower();
            }

            Destroy(gameObject);
        }
    }
}
