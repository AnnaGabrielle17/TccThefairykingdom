using UnityEngine;

public class DetectionTrigger : MonoBehaviour
{
    public IceSpikeController controller;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (controller == null) return;
        if (other.CompareTag("Player"))
        {
            controller.Activate();
        }
    }

}
