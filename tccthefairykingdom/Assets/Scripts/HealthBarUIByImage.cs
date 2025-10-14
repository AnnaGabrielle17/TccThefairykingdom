using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIByImage : MonoBehaviour
{
   [SerializeField] private Sprite[] frames; // 0 = vazio ... N = cheio
    private Image img;

    private void Awake() => img = GetComponent<Image>();

    public void SetHealthFraction(float fraction)
    {
        if (frames == null || frames.Length == 0) return;
        fraction = Mathf.Clamp01(fraction);
        int index = Mathf.RoundToInt((frames.Length - 1) * fraction);
        img.sprite = frames[index];
    }

    public void SetHealth(float current, float max)
    {
        SetHealthFraction(max <= 0 ? 0f : (float)current / max);
    }

    // Se seus frames estiverem invertidos (0 = cheio), use esta variante:
    public void SetHealth_InvertedFrames(float current, float max)
    {
        if (frames == null || frames.Length == 0) return;
        float fraction = max <= 0 ? 0f : (float)current / max;
        int index = Mathf.RoundToInt((frames.Length - 1) * (1f - Mathf.Clamp01(fraction)));
        img.sprite = frames[index];
    }
}


