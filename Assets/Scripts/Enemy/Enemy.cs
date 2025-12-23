using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Düşman AI - NavMeshAgent ile hareket eder ve oyuncuya saldırır
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("NavMesh Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float angularSpeed = 120f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float obstacleAvoidanceRadius = 0.5f;

    [Header("Puan ve Loot")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float dropChance = 0.1f; // %10 şans
    [SerializeField] private GameObject powerUpPrefab;

    [Header("Görsel")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    // Mevcut durum
    private float currentHealth;
    private float currentDamage;
    private float lastAttackTime;
    private bool isDead;
    
    // Hedef
    private Transform target;
    private Renderer enemyRenderer;
    private Color originalColor;

    // NavMeshAgent
    private NavMeshAgent navAgent;
    private bool isInitialized;

    // Public properties
    public float MoveSpeed => moveSpeed;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        // Renderer'ı bul (child'larda da arayabilir)
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

        // NavMeshAgent'ı al veya ekle
        SetupNavMeshAgent();
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Initialize(1f, 1f, 1f);
        }
    }

    /// <summary>
    /// NavMeshAgent'ı ayarlar
    /// </summary>
    private void SetupNavMeshAgent()
    {
        navAgent = GetComponent<NavMeshAgent>();
        
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // NavMeshAgent ayarları
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = angularSpeed;
        navAgent.acceleration = acceleration;
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.radius = obstacleAvoidanceRadius;
        navAgent.height = 2f;
        navAgent.baseOffset = 0f;
        navAgent.autoRepath = true;
        navAgent.autoBraking = true;
    }

    /// <summary>
    /// Düşmanı başlatır (EnemySpawner tarafından çağrılır)
    /// </summary>
    public void Initialize(float healthMultiplier, float damageMultiplier, float speedMultiplier)
    {
        currentHealth = maxHealth * healthMultiplier;
        currentDamage = baseDamage * damageMultiplier;

        // Level'e göre hasar ayarla
        if (GameManager.Instance != null)
        {
            currentDamage *= GameManager.Instance.GetEnemyDamageMultiplier();
        }

        // Hız çarpanını uygula
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed * speedMultiplier;
        }

        // Oyuncuyu bul
        FindTarget();
        
        isInitialized = true;
    }

    private void Update()
    {
        if (isDead) return;

        // Hedef yoksa tekrar ara
        if (target == null)
        {
            FindTarget();
            return; // Hedef bulunana kadar bekle
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            // Saldırı menzilinde - dur ve saldır
            StopMovement();
            AttackTarget();
        }
        else
        {
            // Hedefe doğru hareket et
            MoveTowardsTarget();
        }
    }

    /// <summary>
    /// Oyuncuyu bulur
    /// </summary>
    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    /// <summary>
    /// NavMeshAgent ile hedefe doğru hareket eder
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(target.position);
        }
    }

    /// <summary>
    /// Hareketi durdurur
    /// </summary>
    private void StopMovement()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }
    }

    /// <summary>
    /// Hedefe saldırır
    /// </summary>
    private void AttackTarget()
    {
        // Hedefe bak
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // GameManager'a hasar bildir
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeDamage(currentDamage);
            }

            lastAttackTime = Time.time;
            Debug.Log($"Düşman saldırdı! Hasar: {currentDamage}");
        }
    }

    /// <summary>
    /// Hasar alır
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        // Görsel geri bildirim
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Hasar aldığında yanıp söner
    /// </summary>
    private System.Collections.IEnumerator DamageFlash()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = originalColor;
            }
        }
    }

    /// <summary>
    /// Ölüm işlemi
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Puan ekle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Loot düşür
        TryDropLoot();

        // Yok et
        Destroy(gameObject);
    }

    /// <summary>
    /// Şansa bağlı olarak eşya düşürür
    /// </summary>
    private void TryDropLoot()
    {
        if (Random.value <= dropChance)
        {
            if (powerUpPrefab != null)
            {
                Instantiate(powerUpPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
            else
            {
                // Varsayılan power-up oluştur
                CreateDefaultPowerUp();
            }
            Debug.Log("Eşya düşürüldü!");
        }
    }

    /// <summary>
    /// Varsayılan power-up oluşturur
    /// </summary>
    private void CreateDefaultPowerUp()
    {
        GameObject powerUp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        powerUp.name = "PowerUp";
        powerUp.transform.position = transform.position + Vector3.up * 0.5f;
        powerUp.transform.localScale = Vector3.one * 0.5f;
        powerUp.GetComponent<Renderer>().material.color = Color.cyan;
        powerUp.GetComponent<Collider>().isTrigger = true;
        powerUp.AddComponent<PowerUp>();
    }

    /// <summary>
    /// NavMesh üzerinde pozisyonu ayarlar (spawn sonrası)
    /// </summary>
    public void WarpToNavMesh(Vector3 position)
    {
        if (navAgent != null)
        {
            navAgent.Warp(position);
        }
    }

    /// <summary>
    /// Hareket hızını günceller
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if (navAgent != null)
        {
            navAgent.speed = speed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Saldırı menzilini göster
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
