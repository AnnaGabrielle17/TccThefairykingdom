using UnityEngine;
using UnityEngine.UI;

public class DebugFrameCycler : MonoBehaviour
{
   [Tooltip("Sprites: 0 = vazio ... last = cheio (ou marque inverted se o contrário)")]
    public Sprite[] frames;

    [Tooltip("Marque se frames[0] = cheio e frames[last] = vazio")]
    public bool framesAreInverted = false;

    private Image img;
    private int idx = 0;

    private void Awake()
    {
        img = GetComponent<Image>();
        // inicia cheio automaticamente
        if (frames != null && frames.Length > 0)
            idx = framesAreInverted ? 0 : frames.Length - 1;
        UpdateSprite();
    }

    // NOTE: removidas as teclas de debug (RightArrow / LeftArrow)

    void UpdateSprite()
    {
        if (img == null || frames == null || frames.Length == 0) return;
        idx = Mathf.Clamp(idx, 0, frames.Length - 1);
        img.sprite = frames[idx];
    }

    // APIs públicas que você pode chamar de outro script:
    public void Next() // avança 1 frame (mais cheio ou mais vazio, dependendo da ordem)
    {
        if (frames == null || frames.Length == 0) return;
        if (framesAreInverted) idx = Mathf.Max(0, idx - 1);
        else idx = Mathf.Min(frames.Length - 1, idx + 1);
        UpdateSprite();
    }

    public void Prev() // volta 1 frame
    {
        if (frames == null || frames.Length == 0) return;
        if (framesAreInverted) idx = Mathf.Min(frames.Length - 1, idx + 1);
        else idx = Mathf.Max(0, idx - 1);
        UpdateSprite();
    }

    // reduz 1 (chame quando tomar dano)
    public void DecreaseOne()
    {
        Prev();
    }

    // aumenta 1 (chame quando curar)
    public void IncreaseOne()
    {
        Next();
    }

    // define índice diretamente (0..frames.Length-1)
    public void SetIndex(int index)
    {
        if (frames == null || frames.Length == 0) return;
        idx = Mathf.Clamp(index, 0, frames.Length - 1);
        UpdateSprite();
    }

    // define por fração (0..1) mapeando para índice
    public void SetFraction(float fraction)
    {
        if (frames == null || frames.Length == 0) return;
        fraction = Mathf.Clamp01(fraction);
        int last = frames.Length - 1;
        int index = Mathf.RoundToInt(last * fraction);
        SetIndex(index);
    }

    // utilitários
    public void SetFull()
    {
        idx = framesAreInverted ? 0 : Mathf.Max(0, frames.Length - 1);
        UpdateSprite();
    }

    public void SetEmpty()
    {
        idx = framesAreInverted ? Mathf.Max(0, frames.Length - 1) : 0;
        UpdateSprite();
    }

    // retorna índice atual (útil para sincronização)
    public int GetIndex() => idx;
}