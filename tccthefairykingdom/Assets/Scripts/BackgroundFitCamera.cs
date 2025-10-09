using UnityEngine;

public class BackgroundFitCamera : MonoBehaviour
{
     public Camera targetCamera;            // arraste sua Camera aqui (ou deixe null para Camera.main)
    public SpriteRenderer spriteRenderer;  // arraste aqui (ou o script tentará pegar GetComponent)
    public bool cover = true;              // true = "cover" (preencher), false = "fit" (caber inteiro)
    public bool centerOnCamera = true;     // mantém o background centrado na câmera
    public bool integerScaleForPixelArt = false; // arredonda escala para inteiro (pixel art)

    int lastScreenW = 0, lastScreenH = 0;
    float lastOrtho = -1f;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = Camera.main;
        FitToCamera();
    }

    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        // detecta mudança de resolução ou ortho size
        if (Screen.width != lastScreenW || Screen.height != lastScreenH || (targetCamera != null && targetCamera.orthographicSize != lastOrtho))
        {
            FitToCamera();
            CacheScreen();
        }

        if (centerOnCamera && targetCamera != null)
        {
            Vector3 camPos = targetCamera.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
        }
    }

    void CacheScreen()
    {
        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
        lastOrtho = targetCamera ? targetCamera.orthographicSize : -1f;
    }

    public void FitToCamera()
    {
        if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null) return;
        if (!targetCamera.orthographic)
        {
            Debug.LogWarning("BackgroundFitCamera: melhor usar com Camera ortográfica (2D).");
        }

        // tamanho visível em unidades do mundo
        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        // tamanho do sprite em unidades mundo (já levando em conta pixelsPerUnit)
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size; // em unidades do world

        float scaleX = camWidth / spriteSize.x;
        float scaleY = camHeight / spriteSize.y;
        float finalScale = cover ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

        if (integerScaleForPixelArt)
        {
            // arredonda para o inteiro mais próximo (ou você pode usar Mathf.Floor ou Ceil para garantir múltiplos)
            finalScale = Mathf.Max(1f, Mathf.Round(finalScale));
        }

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    // Método público para forçar recalcular (útil em edição)
    public void ForceFit() => FitToCamera();
}

