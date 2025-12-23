using UnityEngine;
using System.Collections;

/// <summary>
/// Patlayıcı - Belirli süre sonra veya manuel olarak patlar
/// </summary>
public class Explosive : MonoBehaviour
{
    [Header("Patlama Ayarları")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float delay = 3f;
    [SerializeField] private bool detonateOnContact = false;
    [SerializeField] private LayerMask affectedLayers;

    [Header("Görsel")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private float blinkSpeed = 5f;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionEffectDuration = 1.5f;

    private float spawnTime;
    private bool hasExploded;
    private bool isInitialized;
    private Renderer explosiveRenderer;

    private void Awake()
    {
        explosiveRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        spawnTime = Time.time;

        if (!isInitialized)
        {
            Initialize(damage, delay, explosionRadius);
        }
    }

    private void Update()
    {
        if (hasExploded) return;

        float timeRemaining = (spawnTime + delay) - Time.time;

        // Yanıp sönme efekti
        if (explosiveRenderer != null && timeRemaining < delay * 0.5f)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            explosiveRenderer.material.color = Color.Lerp(normalColor, warningColor, t);
        }

        // Süre doldu, patlat
        if (timeRemaining <= 0)
        {
            Detonate();
        }
    }

    /// <summary>
    /// Patlayıcıyı başlatır
    /// </summary>
    public void Initialize(float damage, float delay, float radius)
    {
        this.damage = damage;
        this.delay = delay;
        this.explosionRadius = radius;
        isInitialized = true;
        spawnTime = Time.time;
    }

    /// <summary>
    /// Patlayıcıyı patlama efekti ile başlatır
    /// </summary>
    public void Initialize(float damage, float delay, float radius, GameObject effectPrefab, float effectDuration)
    {
        this.damage = damage;
        this.delay = delay;
        this.explosionRadius = radius;
        this.explosionEffectPrefab = effectPrefab;
        this.explosionEffectDuration = effectDuration;
        isInitialized = true;
        spawnTime = Time.time;
    }

    /// <summary>
    /// Patlayıcıyı patlatır
    /// </summary>
    public void Detonate()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Patlama yarıçapındaki düşmanları bul
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Mesafeye göre hasar (merkezde tam hasar)
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    damageMultiplier = Mathf.Clamp01(damageMultiplier);
                    
                    enemy.TakeDamage(damage * damageMultiplier);
                }
            }
        }

        // Patlama efekti
        ShowExplosionEffect();

        Debug.Log($"Patlayıcı patladı! Hasar: {damage}, Yarıçap: {explosionRadius}");

        // Yok et
        Destroy(gameObject);
    }

    /// <summary>
    /// Patlama efekti gösterir
    /// </summary>
    private void ShowExplosionEffect()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            
            // Particle System varsa otomatik destroy
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.stopAction = ParticleSystemStopAction.Destroy;
                ps.Play();
            }
            
            // Belirli süre sonra yok et
            Destroy(effect, explosionEffectDuration);
        }
        else
        {
            // Basit patlama efekti
            StartCoroutine(SimpleExplosionEffect());
        }
    }

    /// <summary>
    /// Basit patlama efekti
    /// </summary>
    private IEnumerator SimpleExplosionEffect()
    {
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.name = "ExplosionEffect";
        explosion.transform.position = transform.position;
        explosion.GetComponent<Collider>().enabled = false;
        
        Material mat = explosion.GetComponent<Renderer>().material;
        mat.color = new Color(1f, 0.5f, 0f, 0.8f);

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0.5f, explosionRadius * 2f, t);
            explosion.transform.localScale = Vector3.one * scale;
            
            Color color = mat.color;
            color.a = 1f - t;
            mat.color = color;

            yield return null;
        }

        Destroy(explosion);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (detonateOnContact && other.CompareTag("Enemy"))
        {
            Detonate();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Patlama yarıçapını göster
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
