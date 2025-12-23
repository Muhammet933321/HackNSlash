using UnityEngine;

/// <summary>
/// Mermi - Ranged karakter tarafından atılır
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;

    [Header("Efektler")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private bool destroyOnHit = true;

    private Vector3 moveDirection;
    private bool isInitialized;
    private bool hasHit;

    private void Awake()
    {
        // Collider'ı trigger yap
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        // Varsayılan olarak ileri yönde hareket et
        if (!isInitialized)
        {
            moveDirection = transform.forward;
        }

        // Lifetime sonunda yok et
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;
        
        // Hareket
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    /// <summary>
    /// Mermiyi başlatır
    /// </summary>
    public void Initialize(float damage, float speed, float lifetime, Vector3 direction)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.moveDirection = direction.normalized;
        isInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Kendi oyuncusuna çarpmasın
        if (other.CompareTag("Player")) return;
        
        // Diğer mermilere çarpmasın
        if (other.GetComponent<Projectile>() != null) return;
        
        // Tuzak ve patlayıcılara çarpmasın
        if (other.GetComponent<Trap>() != null || other.GetComponent<Explosive>() != null) return;

        // Düşmana çarptı mı?
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Mermi düşmana isabet! Hasar: {damage}");
            }

            hasHit = true;
            ShowHitEffect();

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        // Duvara, zemine veya engele çarptı mı?
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            hasHit = true;
            ShowHitEffect();
            Destroy(gameObject);
        }
        // Bilinmeyen objeye çarptı (powerup hariç)
        else if (!other.CompareTag("PowerUp") && !other.isTrigger)
        {
            hasHit = true;
            ShowHitEffect();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Çarpma efekti gösterir
    /// </summary>
    private void ShowHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // 2 saniye sonra efekti yok et
        }
        else
        {
            // Varsayılan basit efekt
            CreateDefaultHitEffect();
        }
    }

    /// <summary>
    /// Varsayılan çarpma efekti oluşturur
    /// </summary>
    private void CreateDefaultHitEffect()
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.name = "HitEffect";
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.5f;
        
        // Collider'ı kaldır
        Destroy(effect.GetComponent<Collider>());
        
        // Renk
        Renderer rend = effect.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.8f, 0f, 0.8f);
        rend.sharedMaterial = mat;
        
        // Küçülerek yok ol
        effect.AddComponent<HitEffectFade>();
        
        Destroy(effect, 0.3f);
    }
}

/// <summary>
/// Basit çarpma efekti - küçülerek kaybolur
/// </summary>
public class HitEffectFade : MonoBehaviour
{
    private float startTime;
    private Vector3 startScale;

    private void Start()
    {
        startTime = Time.time;
        startScale = transform.localScale;
    }

    private void Update()
    {
        float t = (Time.time - startTime) / 0.3f;
        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
    }
}
