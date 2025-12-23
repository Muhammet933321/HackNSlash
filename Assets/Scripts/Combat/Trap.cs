using UnityEngine;
using System.Collections;

/// <summary>
/// Tuzak - Üzerine basan düşmanlara hasar verir ve yavaşlatır
/// </summary>
public class Trap : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float duration = 15f;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Görsel")]
    [SerializeField] private Color activeColor = new Color(0.5f, 0f, 0.5f);
    [SerializeField] private Color triggeredColor = Color.magenta;
    [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f); // Düşman yakınken

    private bool isInitialized;
    private float spawnTime;
    private Renderer trapRenderer;
    private int enemiesInTrap = 0;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        trapRenderer = GetComponent<Renderer>();
        
        // Collider'ı trigger yap
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        spawnTime = Time.time;
        
        if (!isInitialized)
        {
            Initialize(damage, duration);
        }

        // Başlangıç rengini ayarla
        if (trapRenderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = activeColor;
            trapRenderer.sharedMaterial = mat;
        }
    }

    private void Update()
    {
        // Süre kontrolü
        if (Time.time >= spawnTime + duration)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Tuzağı başlatır
    /// </summary>
    public void Initialize(float damage, float duration, float damageInterval = 0.5f)
    {
        this.damage = damage;
        this.duration = duration;
        this.damageInterval = damageInterval;
        isInitialized = true;
        spawnTime = Time.time;

        // Collider'ı trigger yap
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInTrap++;
            
            // Görsel değiştir
            if (trapRenderer != null)
            {
                trapRenderer.material.color = triggeredColor;
            }

            // Hasar vermeye başla
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                StartCoroutine(DamageEnemy(enemy));
                Debug.Log($"Düşman tuzağa yakalandı! Hasar: {damage}");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Tuzakta kalan düşmanın rengini koru
        if (other.CompareTag("Enemy") && trapRenderer != null)
        {
            trapRenderer.material.color = triggeredColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInTrap = Mathf.Max(0, enemiesInTrap - 1);
            
            // Rengi geri al (başka düşman yoksa)
            if (enemiesInTrap == 0 && trapRenderer != null)
            {
                trapRenderer.material.color = activeColor;
            }
        }
    }

    /// <summary>
    /// Düşmana sürekli hasar verir
    /// </summary>
    private IEnumerator DamageEnemy(Enemy enemy)
    {
        // İlk hasar hemen
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(damage);
        }

        // Sürekli hasar
        while (enemy != null && !enemy.IsDead && IsEnemyInTrap(enemy.gameObject))
        {
            yield return new WaitForSeconds(damageInterval);
            
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage * 0.5f); // Sürekli hasar daha düşük
            }
        }
    }

    /// <summary>
    /// Düşman hala tuzakta mı kontrol eder
    /// </summary>
    private bool IsEnemyInTrap(GameObject enemy)
    {
        if (enemy == null) return false;

        Collider trapCollider = GetComponent<Collider>();
        if (trapCollider == null) return false;

        return trapCollider.bounds.Contains(enemy.transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
