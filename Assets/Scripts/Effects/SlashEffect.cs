using UnityEngine;

/// <summary>
/// Kılıç darbesi efekti - Yarım ay şeklinde kesme izi
/// </summary>
public class SlashEffect : MonoBehaviour
{
    [Header("Efekt Ayarları")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float endScale = 2f;

    [Header("Renk")]
    [SerializeField] private Color startColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color endColor = new Color(1f, 0.8f, 0.4f, 0f);

    [Header("Hareket")]
    [SerializeField] private float forwardSpeed = 2f;
    [SerializeField] private bool rotateEffect = true;
    [SerializeField] private float rotationSpeed = 180f;

    private float spawnTime;
    private Renderer effectRenderer;
    private Material effectMaterial;
    private Vector3 initialScale;

    private void Start()
    {
        spawnTime = Time.time;
        initialScale = transform.localScale;
        
        effectRenderer = GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            // Yeni material oluştur
            effectMaterial = new Material(Shader.Find("Standard"));
            effectMaterial.color = startColor;
            
            // Emission ekle (parlak görünsün)
            effectMaterial.EnableKeyword("_EMISSION");
            effectMaterial.SetColor("_EmissionColor", startColor * 2f);
            
            // Transparent yap
            effectMaterial.SetFloat("_Mode", 3); // Transparent mode
            effectMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            effectMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            effectMaterial.SetInt("_ZWrite", 0);
            effectMaterial.DisableKeyword("_ALPHATEST_ON");
            effectMaterial.EnableKeyword("_ALPHABLEND_ON");
            effectMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            effectMaterial.renderQueue = 3000;
            
            effectRenderer.material = effectMaterial;
        }

        // Collider varsa kapat
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, duration);
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;
        float t = elapsed / duration;

        // Scale animasyonu
        float currentScale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = initialScale * currentScale;

        // Renk ve alpha animasyonu
        if (effectMaterial != null)
        {
            Color currentColor = Color.Lerp(startColor, endColor, t);
            effectMaterial.color = currentColor;
            effectMaterial.SetColor("_EmissionColor", currentColor * (1f - t) * 2f);
        }

        // İleri hareket
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        // Rotasyon
        if (rotateEffect)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Efekti özelleştirir
    /// </summary>
    public void Initialize(Color color, float scale = 1f, float customDuration = -1f)
    {
        startColor = color;
        endColor = new Color(color.r, color.g, color.b, 0f);
        
        if (customDuration > 0)
        {
            duration = customDuration;
        }

        transform.localScale *= scale;
    }
}
