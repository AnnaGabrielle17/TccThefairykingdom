using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIByImage : MonoBehaviour
{
     [SerializeField] private Sprite[] frames; // arraste os sprites (0 = vazio ... N = cheio)
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    // fraction de 0..1
    public void SetHealthFraction(float fraction)
    {
        if (frames == null || frames.Length == 0) return;
        fraction = Mathf.Clamp01(fraction);
        int index = Mathf.RoundToInt((frames.Length - 1) * fraction);
        img.sprite = frames[index];
    }

    public void SetHealth(float current, float max)
    {
        SetHealthFraction(max <= 0 ? 0f : current / max);
    }
}


