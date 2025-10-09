using UnityEngine;

public class InfiniteTiledBackground : MonoBehaviour
{
   
    public Camera targetCamera;            // arraste a camera (ou deixe null p/ Camera.main)
    public Material mat;                   // arraste o material (instância)
    [Tooltip("Pixels per unit equivalente, se estiver convertendo pixels->unidades")]
    public float pixelsPerUnit = 100f;     // ajuste conforme sua import (ex.: 100 ou 16)
    public Vector2 scrollSpeed = Vector2.one; // 1,1 move ao ritmo da câmera (sem parallax)
    Material runtimeMat;
    Texture2D tex;

    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (mat == null) {
            Debug.LogError("Assign material on InfiniteTiledBackground.");
            enabled = false;
            return;
        }

        // instanciar material para não modificar o asset original
        runtimeMat = Instantiate(mat);
        GetComponent<MeshRenderer>().material = runtimeMat;

        tex = runtimeMat.mainTexture as Texture2D;
        if (tex == null) {
            Debug.LogError("Material precisa ter uma Texture2D.");
        }

        UpdateSizeAndTiling();
    }

    void Update()
    {
        // mantém centralizado e atualiza offset para efeito infinito
        if (targetCamera == null) return;

        // faz o quad cobrir a câmera
        UpdateSizeAndTiling();

        // offset baseado na posição da câmera (faz parecer infinito)
        if (tex != null) {
            // mundo -> quantos tex em X/Y
            float texWorldW = tex.width / pixelsPerUnit;
            float texWorldH = tex.height / pixelsPerUnit;
            Vector2 offset = new Vector2(
                (targetCamera.transform.position.x / texWorldW) * scrollSpeed.x,
                (targetCamera.transform.position.y / texWorldH) * scrollSpeed.y
            );
            runtimeMat.mainTextureOffset = offset;
        }

        // manter o quad alinhado XY com a câmera
        Vector3 camPos = targetCamera.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
    }

    void UpdateSizeAndTiling() {
        if (targetCamera == null || runtimeMat == null || tex == null) return;

        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        // ajusta escala do quad (quad padrão é 1x1 unidades)
        transform.localScale = new Vector3(camWidth, camHeight, 1f);

        // textura em unidades mundo:
        float texWorldW = tex.width / pixelsPerUnit;
        float texWorldH = tex.height / pixelsPerUnit;

        // quantas repetições precisamos cobrir a largura/altura
        float repeatX = camWidth / texWorldW;
        float repeatY = camHeight / texWorldH;

        runtimeMat.mainTextureScale = new Vector2(repeatX, repeatY);
    }
}

