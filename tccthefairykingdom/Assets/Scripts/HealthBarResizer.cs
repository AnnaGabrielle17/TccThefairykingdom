using UnityEngine;

public class HealthBarResizer : MonoBehaviour
{
    public RectTransform fillRect; // arraste o RectTransform do Fill (filho)
    private float maxWidth;

    void Start() {
        // assume que o fill inicial está com largura máxima
        maxWidth = fillRect.sizeDelta.x;
    }

    public void UpdateHealth(float current, float max) {
        float ratio = Mathf.Clamp01(current / max);
        Vector2 size = fillRect.sizeDelta;
        size.x = maxWidth * ratio;
        fillRect.sizeDelta = size;
    }
}

