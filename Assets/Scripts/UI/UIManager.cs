using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// UI Yöneticisi - Can, skor ve level gösterimi
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Sağlık UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFill;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Can UI")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Image[] lifeIcons;

    [Header("Skor ve Level UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;

    [Header("Oyun Durumu UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Cooldown Göstergeleri")]
    [SerializeField] private Image primaryCooldownFill;   // Sol tık cooldown (Image type: Filled)
    [SerializeField] private Image secondaryCooldownFill; // Sağ tık cooldown (Image type: Filled)
    [SerializeField] private TextMeshProUGUI primaryCooldownText;   // Opsiyonel - kalan süre yazısı
    [SerializeField] private TextMeshProUGUI secondaryCooldownText; // Opsiyonel - kalan süre yazısı

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // GameManager eventlerine abone ol
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnHealthChanged += UpdateHealth;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnLevelChanged += UpdateLevel;
            GameManager.Instance.OnGameOver += ShowGameOver;
        }

        // Panelleri gizle
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Başlangıç değerlerini göster
        RefreshAllUI();
    }

    private void OnDestroy()
    {
        // Eventlerden çık
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnHealthChanged -= UpdateHealth;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnLevelChanged -= UpdateLevel;
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

    private void Update()
    {
        // Pause kontrolü
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Tüm UI'ı yeniler
    /// </summary>
    public void RefreshAllUI()
    {
        if (GameManager.Instance == null) return;

        UpdateScore(GameManager.Instance.CurrentScore);
        UpdateHealth(GameManager.Instance.CurrentHealth, GameManager.Instance.MaxHealth);
        UpdateLives(GameManager.Instance.CurrentLives);
        UpdateLevel(GameManager.Instance.CurrentLevel);
    }

    /// <summary>
    /// Skor gösterimini günceller
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Skor: {score}";
        }

        // Bir sonraki level için gereken puan
        if (nextLevelText != null && GameManager.Instance != null)
        {
            int required = GameManager.Instance.CurrentLevel * 100;
            nextLevelText.text = $"Sonraki Level: {required}";
        }
    }

    /// <summary>
    /// Sağlık gösterimini günceller
    /// </summary>
    public void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.value = current / max;
        }

        // Renk değişimi
        if (healthFill != null)
        {
            float healthPercent = current / max;
            if (healthPercent > 0.6f)
            {
                healthFill.color = healthyColor;
            }
            else if (healthPercent > 0.3f)
            {
                healthFill.color = damagedColor;
            }
            else
            {
                healthFill.color = criticalColor;
            }
        }
    }

    /// <summary>
    /// Can gösterimini günceller
    /// </summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Can: {lives}";
        }

        // İkon gösterimi
        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].gameObject.SetActive(i < lives);
                }
            }
        }
    }

    /// <summary>
    /// Level gösterimini günceller
    /// </summary>
    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }

        // Sonraki level puanını güncelle
        if (nextLevelText != null)
        {
            int required = level * 100;
            nextLevelText.text = $"Sonraki Level: {required} puan";
        }
    }

    /// <summary>
    /// Karakter bilgisini günceller (artık kullanılmıyor)
    /// </summary>
    public void UpdateCharacterInfo(string name, float damage)
    {
        // Artık kullanılmıyor
    }

    /// <summary>
    /// Game Over ekranını gösterir
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null && GameManager.Instance != null)
            {
                finalScoreText.text = $"Final Skor: {GameManager.Instance.CurrentScore}";
            }
        }
    }

    /// <summary>
    /// Pause'u açar/kapatır
    /// </summary>
    public void TogglePause()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(GameManager.Instance?.IsPaused ?? false);
        }
    }

    /// <summary>
    /// Yeniden başlat butonu
    /// </summary>
    public void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Ana menü butonu
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Devam et butonu
    /// </summary>
    public void OnResumeButtonClicked()
    {
        TogglePause();
    }

    #region Cooldown Göstergeleri

    /// <summary>
    /// Sol tık (birincil saldırı) cooldown göstergesini günceller
    /// </summary>
    /// <param name="fillAmount">0-1 arasında doluluk oranı (0 = tam cooldown, 1 = hazır)</param>
    /// <param name="remainingTime">Kalan süre (saniye)</param>
    public void UpdatePrimaryCooldown(float fillAmount, float remainingTime = 0f)
    {
        if (primaryCooldownFill != null)
        {
            // Ters çevir: cooldown bittiğinde dolu, başladığında boş
            primaryCooldownFill.fillAmount = 1f - fillAmount;
        }

        if (primaryCooldownText != null)
        {
            if (remainingTime > 0.1f)
            {
                primaryCooldownText.text = remainingTime.ToString("F1");
            }
            else
            {
                primaryCooldownText.text = "";
            }
        }
    }

    /// <summary>
    /// Sağ tık (özel saldırı) cooldown göstergesini günceller
    /// </summary>
    /// <param name="fillAmount">0-1 arasında doluluk oranı (0 = tam cooldown, 1 = hazır)</param>
    /// <param name="remainingTime">Kalan süre (saniye)</param>
    public void UpdateSecondaryCooldown(float fillAmount, float remainingTime = 0f)
    {
        if (secondaryCooldownFill != null)
        {
            // Ters çevir: cooldown bittiğinde dolu, başladığında boş
            secondaryCooldownFill.fillAmount = 1f - fillAmount;
        }

        if (secondaryCooldownText != null)
        {
            if (remainingTime > 0.1f)
            {
                secondaryCooldownText.text = remainingTime.ToString("F1");
            }
            else
            {
                secondaryCooldownText.text = "";
            }
        }
    }

    #endregion
}
