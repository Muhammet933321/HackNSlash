using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Düşman spawn sistemi - NavMesh üzerinde düşman oluşturur
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab Ayarları")]
    [SerializeField] private GameObject[] enemyPrefabs; // Birden fazla düşman tipi
    [SerializeField] private GameObject defaultEnemyPrefab; // Varsayılan prefab

    [Header("Spawn Ayarları")]
    [SerializeField] private float baseSpawnInterval = 5f;
    [SerializeField] private int baseEnemiesPerWave = 3;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float minSpawnDistance = 10f; // Minimum oyuncu mesafesi

    [Header("NavMesh Spawn Ayarları")]
    [SerializeField] private float navMeshSampleDistance = 5f; // NavMesh bulma mesafesi
    [SerializeField] private int maxSpawnAttempts = 10; // Spawn denemesi sayısı

    [Header("Özel Spawn Noktaları")]
    [SerializeField] private bool useCustomSpawnPoints = false;
    [SerializeField] private Transform[] customSpawnPoints;

    [Header("Zorluk Ayarları")]
    [SerializeField] private float enemyHealthMultiplier = 1f;
    [SerializeField] private float enemyDamageMultiplier = 1f;
    [SerializeField] private float enemySpeedMultiplier = 1f;

    // Spawn durumu
    private bool isSpawning;
    private Transform playerTransform;
    private Coroutine spawnCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();

    // Spawn yönleri
    private enum SpawnDirection { North, South, East, West }

    // Public properties
    public int ActiveEnemyCount => GetActiveEnemyCount();
    public bool IsSpawning => isSpawning;

    private void Start()
    {
        FindPlayer();
        StartSpawning();
    }

    private void Update()
    {
        // Aktif düşman listesini temizle
        activeEnemies.RemoveAll(e => e == null);
    }

    /// <summary>
    /// Oyuncuyu bulur
    /// </summary>
    public void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// Spawn'ı başlatır
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning) return;
        
        FindPlayer();
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnWaveRoutine());
    }

    /// <summary>
    /// Spawn'ı durdurur
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// Wave spawn döngüsü
    /// </summary>
    private IEnumerator SpawnWaveRoutine()
    {
        while (isSpawning)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield break;
            }

            // Level'e göre değerleri hesapla
            float spawnInterval = GameManager.Instance != null 
                ? GameManager.Instance.GetSpawnInterval(baseSpawnInterval) 
                : baseSpawnInterval;
            
            int enemyCount = GameManager.Instance != null 
                ? GameManager.Instance.GetEnemySpawnCount(baseEnemiesPerWave) 
                : baseEnemiesPerWave;

            // Wave'i spawn et
            SpawnWave(enemyCount);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Bir wave düşman spawn eder
    /// </summary>
    private void SpawnWave(int count)
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        // Rastgele bir yön seç (4 kenardan biri)
        SpawnDirection direction = (SpawnDirection)Random.Range(0, 4);

        int spawnedCount = 0;
        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos = GetTargetSpawnPosition(direction, i, count);
            
            // NavMesh üzerinde geçerli pozisyon bul
            if (TryGetNavMeshPosition(targetPos, out Vector3 validPos))
            {
                SpawnEnemy(validPos);
                spawnedCount++;
            }
        }

        if (spawnedCount > 0)
        {
            Debug.Log($"Wave spawn edildi! Düşman sayısı: {spawnedCount}, Yön: {direction}");
        }
    }

    /// <summary>
    /// Hedef spawn pozisyonunu hesaplar
    /// </summary>
    private Vector3 GetTargetSpawnPosition(SpawnDirection direction, int index, int total)
    {
        if (useCustomSpawnPoints && customSpawnPoints.Length > 0)
        {
            return customSpawnPoints[Random.Range(0, customSpawnPoints.Length)].position;
        }

        Vector3 playerPos = playerTransform.position;
        float offset = (index - total / 2f) * 2f;

        Vector3 spawnPos = Vector3.zero;

        switch (direction)
        {
            case SpawnDirection.North:
                spawnPos = playerPos + new Vector3(offset, 0, spawnRadius);
                break;
            case SpawnDirection.South:
                spawnPos = playerPos + new Vector3(offset, 0, -spawnRadius);
                break;
            case SpawnDirection.East:
                spawnPos = playerPos + new Vector3(spawnRadius, 0, offset);
                break;
            case SpawnDirection.West:
                spawnPos = playerPos + new Vector3(-spawnRadius, 0, offset);
                break;
        }

        return spawnPos;
    }

    /// <summary>
    /// NavMesh üzerinde geçerli pozisyon bulur
    /// </summary>
    private bool TryGetNavMeshPosition(Vector3 targetPosition, out Vector3 result)
    {
        result = targetPosition;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * navMeshSampleDistance;
            randomOffset.y = 0;
            Vector3 samplePosition = targetPosition + randomOffset;

            if (NavMesh.SamplePosition(samplePosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                // Oyuncuya çok yakın mı kontrol et
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(hit.position, playerTransform.position);
                    if (distanceToPlayer < minSpawnDistance)
                    {
                        continue;
                    }
                }

                result = hit.position;
                return true;
            }
        }

        // NavMesh bulunamadı, orijinal pozisyonu dene
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit finalHit, navMeshSampleDistance * 2f, NavMesh.AllAreas))
        {
            result = finalHit.position;
            return true;
        }

        Debug.LogWarning($"NavMesh pozisyonu bulunamadı: {targetPosition}");
        return false;
    }

    /// <summary>
    /// Tek bir düşman spawn eder
    /// </summary>
    private void SpawnEnemy(Vector3 position)
    {
        GameObject prefab = GetRandomEnemyPrefab();
        GameObject enemy;

        if (prefab != null)
        {
            enemy = Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Düşman prefabı atanmamış! Varsayılan düşman oluşturuluyor...");
            enemy = CreateDefaultEnemy(position);
        }

        // Tag ve Layer ayarla
        enemy.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        enemy.layer = enemyLayer >= 0 ? enemyLayer : 0;

        // NavMesh üzerinde pozisyonu ayarla
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            float damageMulti = enemyDamageMultiplier;
            if (GameManager.Instance != null)
            {
                damageMulti *= GameManager.Instance.GetEnemyDamageMultiplier();
            }
            enemyComponent.Initialize(enemyHealthMultiplier, damageMulti, enemySpeedMultiplier);
            enemyComponent.WarpToNavMesh(position);
        }

        activeEnemies.Add(enemy);
    }

    /// <summary>
    /// Rastgele düşman prefabı seçer
    /// </summary>
    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            // Null olmayan prefabları filtrele
            var validPrefabs = new List<GameObject>();
            foreach (var prefab in enemyPrefabs)
            {
                if (prefab != null) validPrefabs.Add(prefab);
            }

            if (validPrefabs.Count > 0)
            {
                return validPrefabs[Random.Range(0, validPrefabs.Count)];
            }
        }

        return defaultEnemyPrefab;
    }

    /// <summary>
    /// Varsayılan düşman oluşturur (prefab yoksa)
    /// </summary>
    private GameObject CreateDefaultEnemy(Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy_Default";
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(1f, 1f, 1f);
        
        // Renk ayarla
        Renderer rend = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        rend.sharedMaterial = mat;
        
        enemy.tag = "Enemy";
        
        // Layer ayarla
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        enemy.layer = enemyLayer >= 0 ? enemyLayer : 0;

        // Enemy script ekle (NavMeshAgent otomatik eklenir)
        enemy.AddComponent<Enemy>();

        return enemy;
    }

    /// <summary>
    /// Belirli bir prefabı spawn eder
    /// </summary>
    public void SpawnSpecificEnemy(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        if (TryGetNavMeshPosition(position, out Vector3 validPos))
        {
            GameObject enemy = Instantiate(prefab, validPos, Quaternion.identity);
            
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.Initialize(enemyHealthMultiplier, enemyDamageMultiplier, enemySpeedMultiplier);
                enemyComponent.WarpToNavMesh(validPos);
            }

            activeEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// Tüm aktif düşmanları yok eder
    /// </summary>
    public void DestroyAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    /// <summary>
    /// Aktif düşman sayısını döndürür
    /// </summary>
    public int GetActiveEnemyCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }

    /// <summary>
    /// Zorluk çarpanlarını günceller
    /// </summary>
    public void SetDifficultyMultipliers(float health, float damage, float speed)
    {
        enemyHealthMultiplier = health;
        enemyDamageMultiplier = damage;
        enemySpeedMultiplier = speed;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;
        
        // Spawn yarıçapını göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center + Vector3.forward * spawnRadius, 2f);
        Gizmos.DrawWireSphere(center + Vector3.back * spawnRadius, 2f);
        Gizmos.DrawWireSphere(center + Vector3.right * spawnRadius, 2f);
        Gizmos.DrawWireSphere(center + Vector3.left * spawnRadius, 2f);

        // Minimum mesafeyi göster
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, minSpawnDistance);

        // Spawn yarıçapını göster
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}
