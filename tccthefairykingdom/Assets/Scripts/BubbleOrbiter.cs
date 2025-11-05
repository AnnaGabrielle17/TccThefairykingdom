using UnityEngine;

public class BubbleOrbiter : MonoBehaviour
{
    [Header("Orbit")]
    public float radius = 0.7f;            // raio usado se Init não setar
    public float spinSpeed = 40f;          // deg/s (padrão local)
    public float baseScale = 0.4f;         // escala base da bolha
    public bool useParentAsCenter = true;  // usa transform.parent como centro

    // estado interno
    Vector3 spherePoint;  // ponto unitário na esfera (x,y,z)
    float spinOffsetDeg;  // offset inicial (para variar posição)
    SpriteRenderer sr;
    Transform center;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Init chamado pelo emitter pra configurar posição inicial e escala
    public void Init(Vector3 spherePointWorld, float baseScaleLocal, float emitterSpinDeg)
    {
        // se passar spherePointWorld = posição no espaço local do emitPoint, transformará pra unit vector
        // assumimos spherePointWorld já é vetor (x,y,z) da esfera * radius
        spherePoint = spherePointWorld.normalized;
        baseScale = baseScaleLocal;
        // use um spinOffset para não coincidir todas as bolhas
        spinOffsetDeg = Random.Range(0f, 360f) + emitterSpinDeg * 0.01f;
        radius = spherePointWorld.magnitude;
        center = useParentAsCenter && transform.parent != null ? transform.parent : null;
    }

    void Update()
    {
        // rota a esfera local incrementando phi (azimuth) pelo spinSpeed
        float delta = spinSpeed * Time.deltaTime;
        // rotações simples: girar o ponto em torno do eixo Y (vertical do "mundo 3D")
        Quaternion rot = Quaternion.Euler(0f, delta, 0f);
        spherePoint = rot * spherePoint; // rotaciona o vetor na "esfera"

        // calcula projeção 3D->2D: x -> x, z -> y; usamos y como profundidade (para scale/sorting)
        float x = spherePoint.x * radius;
        float y3d = spherePoint.y * radius; // profundidade (vai de -radius..+radius)
        float z = spherePoint.z * radius;

        Vector2 localPos2D = new Vector2(x, z); // x horizontal, z vertical

        if (center != null)
            transform.localPosition = localPos2D; // local ao emitPoint
        else
            transform.position = new Vector3(localPos2D.x, localPos2D.y, 0f);

        // escala com base no "y3d" para simular aproximação/afastamento
        float depthNormalized = (y3d / Mathf.Max(0.0001f, radius) + 1f) * 0.5f; // 0..1
        float scaleFactor = Mathf.Lerp(0.6f, 1.0f, depthNormalized); // ajuste visual
        transform.localScale = Vector3.one * (baseScale * scaleFactor);

        // ajuste do sortingOrder para que bolhas "na frente" desenhem por cima
        // maior y3d => mais na frente
        if (sr != null)
        {
            int order = Mathf.RoundToInt(depthNormalized * 100f);
            sr.sortingOrder = order;
        }
    }
}
