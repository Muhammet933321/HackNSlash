using UnityEngine;

/// <summary>
/// Güç artırıcı eşya - Düşmanlar tarafından düşürülür
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        DamageBoost,    // Saldırı gücü artışı
        HealthRestore,  // Can yenileme
        SpeedBoost      // Hız artışı (opsiyonel)
    }

    [Header("Ayarlar")]
    [SerializeField] private PowerUpType type = PowerUpType.DamageBoost;
    [SerializeField] private float damageBoostAmount = 5f;
    [SerializeField] private float healthRestoreAmount = 25f;
    [SerializeField] private float lifetime = 15f;

    [Header("Görsel")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private float spawnTime;

    private void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;

        // Collider'ı trigger yap
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Lifetime sonunda yok et
        Destroy(gameObject, lifetime);

        // Renk ayarla
        SetColorByType();
    }

    private void Update()
    {
        // Döndürme animasyonu
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Yukarı aşağı hareket
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    /// <summary>
    /// Tipe göre renk ayarlar
    /// </summary>
    private void SetColorByType()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        switch (type)
        {
            case PowerUpType.DamageBoost:
                rend.material.color = Color.cyan;
                break;
            case PowerUpType.HealthRestore:
                rend.material.color = Color.green;
                break;
            case PowerUpType.SpeedBoost:
                rend.material.color = Color.yellow;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyPowerUp(other.gameObject);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Güç artışını uygular
    /// </summary>
    private void ApplyPowerUp(GameObject player)
    {
        switch (type)
        {
            case PowerUpType.DamageBoost:
                ApplyDamageBoost(player);
                break;
            case PowerUpType.HealthRestore:
                ApplyHealthRestore();
                break;
            case PowerUpType.SpeedBoost:
                // İleride eklenebilir
                break;
        }

        Debug.Log($"PowerUp alındı: {type}");
    }

    /// <summary>
    /// Hasar artışı uygular
    /// </summary>
    private void ApplyDamageBoost(GameObject player)
    {
        PlayerBase playerBase = player.GetComponent<PlayerBase>();
        if (playerBase != null)
        {
            playerBase.IncreaseDamage(damageBoostAmount);
        }
    }

    /// <summary>
    /// Can yenileme uygular
    /// </summary>
    private void ApplyHealthRestore()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Heal(healthRestoreAmount);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
