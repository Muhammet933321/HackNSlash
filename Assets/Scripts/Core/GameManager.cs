using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Ana oyun yöneticisi - Skor, level, can ve oyun durumu yönetimi
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Oyun Ayarları")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private int pointsPerLevel = 100; // Her level için gereken puan çarpanı

    [Header("Mevcut Durum")]
    private int currentLives;
    private float currentHealth;
    private int currentScore;
    private int currentLevel = 1;
    private bool isGameOver;
    private bool isPaused;

    // Eventler - UI ve diğer sistemler bunları dinleyecek
    public event Action<int> OnScoreChanged;
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<int> OnLivesChanged;
    public event Action<int> OnLevelChanged;
    public event Action OnGameOver;

    // Propertyler
    public int CurrentScore => currentScore;
    public int CurrentLevel => currentLevel;
    public int CurrentLives => currentLives;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// Oyunu başlangıç durumuna getirir
    /// </summary>
    public void InitializeGame()
    {
        currentLives = startingLives;
        currentHealth = maxHealth;
        currentScore = 0;
        currentLevel = 1;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Tüm eventleri tetikle
        OnScoreChanged?.Invoke(currentScore);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnLivesChanged?.Invoke(currentLives);
        OnLevelChanged?.Invoke(currentLevel);
    }

    /// <summary>
    /// Puan ekler ve level kontrolü yapar
    /// </summary>
    public void AddScore(int points)
    {
        if (isGameOver) return;

        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);

        // Level atlama kontrolü: level x 100 puana ulaşınca
        int requiredPoints = currentLevel * pointsPerLevel;
        if (currentScore >= requiredPoints)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// Bir sonraki levele geçiş
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        OnLevelChanged?.Invoke(currentLevel);
        Debug.Log($"Level Atlama! Yeni Level: {currentLevel}");

        // Canı yenile
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Oyuncuya hasar verir
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isGameOver) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            LoseLife();
        }
    }

    /// <summary>
    /// Oyuncuyu iyileştirir
    /// </summary>
    public void Heal(float amount)
    {
        if (isGameOver) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Bir can kaybeder
    /// </summary>
    private void LoseLife()
    {
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Canı yenile ve devam et
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log($"Can kaybedildi! Kalan can: {currentLives}");
        }
    }

    /// <summary>
    /// Oyun bitti
    /// </summary>
    private void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
        Debug.Log("OYUN BİTTİ!");
    }

    /// <summary>
    /// Oyunu duraklatır/devam ettirir
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    /// <summary>
    /// Oyunu yeniden başlatır
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        InitializeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Ana menüye döner
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        Destroy(gameObject);
        Instance = null;
        SceneManager.LoadScene("MainMenuScene"); // Ana menü sahne adı
    }

    /// <summary>
    /// Mevcut level için canavar hasarını hesaplar
    /// </summary>
    public float GetEnemyDamageMultiplier()
    {
        // Her level %20 daha fazla hasar
        return 1f + (currentLevel - 1) * 0.2f;
    }

    /// <summary>
    /// Mevcut level için spawn sayısını hesaplar
    /// </summary>
    public int GetEnemySpawnCount(int baseCount)
    {
        // Her level 2 canavar daha fazla
        return baseCount + (currentLevel - 1) * 2;
    }

    /// <summary>
    /// Mevcut level için spawn aralığını hesaplar
    /// </summary>
    public float GetSpawnInterval(float baseInterval)
    {
        // Her level %10 daha hızlı (minimum 1 saniye)
        float interval = baseInterval * Mathf.Pow(0.9f, currentLevel - 1);
        return Mathf.Max(1f, interval);
    }
}
