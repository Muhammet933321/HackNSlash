using UnityEngine;

/// <summary>
/// Dönerek saldırı efekti - 360 derece dairesel efekt
/// </summary>
public class SpinEffect : MonoBehaviour
{
    [Header("Efekt Ayarları")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float rotationSpeed = 720f; // 2 tur/saniye

    [Header("Renk")]
    [SerializeField] private Color effectColor = new Color(1f, 0.3f, 0.1f, 0.8f);

    [Header("Trail")]
    [SerializeField] private int trailCount = 3;

    private float spawnTime;
    private GameObject[] trails;
    private Material[] trailMaterials;

    private void Start()
    {
        spawnTime = Time.time;
        CreateTrails();
        Destroy(gameObject, duration);
    }

    private void CreateTrails()
    {
        trails = new GameObject[trailCount];
        trailMaterials = new Material[trailCount];

        for (int i = 0; i < trailCount; i++)
        {
            // Trail objesi oluştur
            GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trail.name = $"SpinTrail_{i}";
            trail.transform.SetParent(transform);
            
            // Pozisyon (radius mesafesinde)
            float angle = (360f / trailCount) * i * Mathf.Deg2Rad;
            trail.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0.5f, Mathf.Sin(angle) * radius);
            trail.transform.localScale = new Vector3(0.3f, 1f, 1.5f);
            trail.transform.LookAt(transform.position + Vector3.up * 0.5f);

            // Collider kapat
            Destroy(trail.GetComponent<Collider>());

            // Material
            Material mat = new Material(Shader.Find("Standard"));
            float alpha = 1f - (i * 0.2f); // Her trail biraz daha soluk
            mat.color = new Color(effectColor.r, effectColor.g, effectColor.b, effectColor.a * alpha);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", effectColor * 2f * alpha);
            
            // Transparent
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            trail.GetComponent<Renderer>().material = mat;
            
            trails[i] = trail;
            trailMaterials[i] = mat;
        }
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;
        float t = elapsed / duration;

        // Döndür
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Fade out
        foreach (Material mat in trailMaterials)
        {
            if (mat != null)
            {
                Color c = mat.color;
                c.a = effectColor.a * (1f - t);
                mat.color = c;
                mat.SetColor("_EmissionColor", effectColor * 2f * (1f - t));
            }
        }
    }

    /// <summary>
    /// Efekti özelleştirir
    /// </summary>
    public void Initialize(float customRadius, Color color)
    {
        radius = customRadius;
        effectColor = color;
    }
}
