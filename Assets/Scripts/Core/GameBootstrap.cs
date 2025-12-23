using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Oyun başlatıcı - Tüm sistemleri prefab tabanlı olarak başlatır
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Otomatik Oluşturma")]
    [SerializeField] private bool autoCreateSystems = true;
    [SerializeField] private bool autoSpawnPlayer = true;
    [SerializeField] private bool autoGenerateMap = true;

    [Header("Karakter Prefabları")]
    [Tooltip("Nişancı karakter prefabı")]
    [SerializeField] private GameObject rangedCharacterPrefab;
    [Tooltip("Savaşçı karakter prefabı")]
    [SerializeField] private GameObject meleeCharacterPrefab;
    [Tooltip("Tuzakçı karakter prefabı")]
    [SerializeField] private GameObject trapperCharacterPrefab;

    [Header("Düşman Prefabları")]
    [Tooltip("Düşman prefabları - birden fazla tip için array")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [Tooltip("Varsayılan düşman prefabı")]
    [SerializeField] private GameObject defaultEnemyPrefab;

    [Header("UI Prefabı")]
    [Tooltip("Oyun içi Canvas prefabı - UIManager içermeli")]
    [SerializeField] private GameObject canvasPrefab;

    [Header("Karakter Seçimi")]
    [SerializeField] private CharacterSelector.CharacterType startingCharacter = CharacterSelector.CharacterType.Ranged;

    [Header("Harita Seçimi")]
    [SerializeField] private MapGenerator.MapType startingMap = MapGenerator.MapType.Forest;

    // Referanslar
    private CharacterSelector characterSelector;
    private EnemySpawner enemySpawner;

    private void Awake()
    {
        if (autoCreateSystems)
        {
            CreateGameSystems();
        }
    }

    private void Start()
    {
        if (autoGenerateMap)
        {
            GenerateMap();
        }

        if (autoSpawnPlayer)
        {
            SpawnPlayer();
        }

        SetupCamera();
    }

    /// <summary>
    /// Oyun sistemlerini oluşturur
    /// </summary>
    private void CreateGameSystems()
    {
        // GameManager
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        // CharacterSelector
        characterSelector = FindAnyObjectByType<CharacterSelector>();
        if (characterSelector == null)
        {
            GameObject selectorObj = new GameObject("CharacterSelector");
            characterSelector = selectorObj.AddComponent<CharacterSelector>();
        }

        // Karakter prefablarını ata
        if (characterSelector != null)
        {
            characterSelector.SetPrefabs(rangedCharacterPrefab, meleeCharacterPrefab, trapperCharacterPrefab);
        }

        // EnemySpawner
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (enemySpawner == null)
        {
            GameObject spawnerObj = new GameObject("EnemySpawner");
            enemySpawner = spawnerObj.AddComponent<EnemySpawner>();
        }

        // Düşman prefablarını ata
        SetupEnemySpawnerPrefabs();

        // UI Manager - Prefab'dan oluştur veya mevcut olanı kullan
        if (FindAnyObjectByType<UIManager>() == null)
        {
            CreateUIFromPrefab();
        }

        Debug.Log("Oyun sistemleri oluşturuldu.");
    }

    /// <summary>
    /// EnemySpawner'a prefabları atar
    /// </summary>
    private void SetupEnemySpawnerPrefabs()
    {
        if (enemySpawner == null) return;

        var type = typeof(EnemySpawner);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // Düşman prefablarını ata
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            type.GetField("enemyPrefabs", flags)?.SetValue(enemySpawner, enemyPrefabs);
        }

        if (defaultEnemyPrefab != null)
        {
            type.GetField("defaultEnemyPrefab", flags)?.SetValue(enemySpawner, defaultEnemyPrefab);
        }
    }

    /// <summary>
    /// Canvas prefab'ından UI oluşturur
    /// </summary>
    private void CreateUIFromPrefab()
    {
        if (canvasPrefab != null)
        {
            // Prefab'dan Canvas oluştur
            GameObject canvasInstance = Instantiate(canvasPrefab);
            canvasInstance.name = "Canvas";
            
            // UIManager'ı kontrol et
            UIManager uiManager = canvasInstance.GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = canvasInstance.GetComponentInChildren<UIManager>();
            }
            
            if (uiManager != null)
            {
                Debug.Log("Canvas prefab'dan oluşturuldu. Butonları Inspector'dan OnClick eventlerine atayın.");
            }
            else
            {
                Debug.LogWarning("Canvas prefab'ında UIManager bulunamadı!");
            }
        }
        else
        {
            Debug.LogWarning("Canvas prefab atanmamış! UI oluşturulamadı.");
        }
    }

    /// <summary>
    /// Tam UI sistemi oluşturur (Prefab yoksa fallback)
    /// </summary>
    private void CreateFullUI()
    {
        // Ana Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // UI Manager
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // === ÜST PANEL (Sağlık, Can, Skor, Level) ===
        GameObject topPanel = CreatePanel(canvasObj.transform, "TopPanel", 
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, 0), new Vector2(0, 80));
        topPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        
        RectTransform topRect = topPanel.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = new Vector2(0, -80); 
        topRect.offsetMax = new Vector2(0, 0);
        
        HorizontalLayoutGroup topLayout = topPanel.AddComponent<HorizontalLayoutGroup>();
        topLayout.spacing = 40;
        topLayout.padding = new RectOffset(30, 30, 15, 15);
        topLayout.childAlignment = TextAnchor.MiddleCenter;
        topLayout.childControlWidth = false;
        topLayout.childControlHeight = false;
        topLayout.childForceExpandWidth = false;

        // --- Sağlık Barı Container ---
        GameObject healthContainer = new GameObject("HealthContainer");
        healthContainer.transform.SetParent(topPanel.transform, false);
        RectTransform healthContRect = healthContainer.AddComponent<RectTransform>();
        healthContRect.sizeDelta = new Vector2(280, 50);
        LayoutElement healthLE = healthContainer.AddComponent<LayoutElement>();
        healthLE.preferredWidth = 280;
        healthLE.preferredHeight = 50;

        // Health Bar Background
        GameObject healthBg = CreateImage(healthContainer.transform, "HealthBarBg",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(5, 0), new Vector2(200, 25), new Color(0.3f, 0.1f, 0.1f));

        // Health Bar Slider
        GameObject healthSliderObj = new GameObject("HealthBar");
        healthSliderObj.transform.SetParent(healthBg.transform, false);
        RectTransform healthSliderRect = healthSliderObj.AddComponent<RectTransform>();
        healthSliderRect.anchorMin = Vector2.zero;
        healthSliderRect.anchorMax = Vector2.one;
        healthSliderRect.sizeDelta = Vector2.zero;
        healthSliderRect.anchoredPosition = Vector2.zero;

        Slider healthSlider = healthSliderObj.AddComponent<Slider>();
        healthSlider.minValue = 0;
        healthSlider.maxValue = 1;
        healthSlider.value = 1;
        healthSlider.interactable = false;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(healthSliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -6);
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        // Fill
        GameObject fill = CreateImage(fillArea.transform, "Fill",
            Vector2.zero, new Vector2(1, 1), new Vector2(0, 0.5f),
            Vector2.zero, Vector2.zero, Color.green);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        
        Image fillImage = fill.GetComponent<Image>();
        healthSlider.fillRect = fillRect;

        // Health Text
        TextMeshProUGUI healthText = CreateText(healthContainer.transform, "HealthText",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0.5f),
            new Vector2(210, 0), new Vector2(70, 30), "100/100", 14, TextAlignmentOptions.Left);

        // --- Can Göstergesi ---
        GameObject livesContainer = new GameObject("LivesContainer");
        livesContainer.transform.SetParent(topPanel.transform, false);
        RectTransform livesContRect = livesContainer.AddComponent<RectTransform>();
        livesContRect.sizeDelta = new Vector2(100, 50);
        LayoutElement livesLE = livesContainer.AddComponent<LayoutElement>();
        livesLE.preferredWidth = 100;
        livesLE.preferredHeight = 50;

        TextMeshProUGUI livesText = CreateText(livesContainer.transform, "LivesText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(100, 40), "❤ x 3", 24, TextAlignmentOptions.Center);
        livesText.color = Color.red;

        // --- Skor ---
        GameObject scoreContainer = new GameObject("ScoreContainer");
        scoreContainer.transform.SetParent(topPanel.transform, false);
        RectTransform scoreContRect = scoreContainer.AddComponent<RectTransform>();
        scoreContRect.sizeDelta = new Vector2(150, 50);
        LayoutElement scoreLE = scoreContainer.AddComponent<LayoutElement>();
        scoreLE.preferredWidth = 150;
        scoreLE.preferredHeight = 50;

        TextMeshProUGUI scoreText = CreateText(scoreContainer.transform, "ScoreText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(150, 40), "Skor: 0", 22, TextAlignmentOptions.Center);
        scoreText.color = Color.yellow;

        // --- Level ---
        GameObject levelContainer = new GameObject("LevelContainer");
        levelContainer.transform.SetParent(topPanel.transform, false);
        RectTransform levelContRect = levelContainer.AddComponent<RectTransform>();
        levelContRect.sizeDelta = new Vector2(120, 50);
        LayoutElement levelLE = levelContainer.AddComponent<LayoutElement>();
        levelLE.preferredWidth = 120;
        levelLE.preferredHeight = 50;

        TextMeshProUGUI levelText = CreateText(levelContainer.transform, "LevelText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(120, 40), "Level: 1", 22, TextAlignmentOptions.Center);
        levelText.color = Color.cyan;

        // --- Sonraki Level ---
        GameObject nextLevelContainer = new GameObject("NextLevelContainer");
        nextLevelContainer.transform.SetParent(topPanel.transform, false);
        RectTransform nextContRect = nextLevelContainer.AddComponent<RectTransform>();
        nextContRect.sizeDelta = new Vector2(180, 50);
        LayoutElement nextLE = nextLevelContainer.AddComponent<LayoutElement>();
        nextLE.preferredWidth = 180;
        nextLE.preferredHeight = 50;

        TextMeshProUGUI nextLevelText = CreateText(nextLevelContainer.transform, "NextLevelText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(180, 40), "Hedef: 100", 18, TextAlignmentOptions.Center);
        nextLevelText.color = new Color(0.8f, 0.8f, 0.8f);

        // === GAME OVER PANELİ ===
        GameObject gameOverPanel = CreateCenteredPanel(canvasObj.transform, "GameOverPanel", 
            new Vector2(400, 300), new Color(0.1f, 0.1f, 0.1f, 0.95f));
        gameOverPanel.SetActive(false);

        VerticalLayoutGroup goLayout = gameOverPanel.AddComponent<VerticalLayoutGroup>();
        goLayout.spacing = 25;
        goLayout.padding = new RectOffset(40, 40, 50, 40);
        goLayout.childAlignment = TextAnchor.MiddleCenter;
        goLayout.childControlWidth = true;
        goLayout.childControlHeight = false;

        TextMeshProUGUI gameOverTitle = CreateText(gameOverPanel.transform, "GameOverTitle",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(320, 60), "OYUN BİTTİ!", 40, TextAlignmentOptions.Center);
        gameOverTitle.color = Color.red;
        gameOverTitle.fontStyle = FontStyles.Bold;

        TextMeshProUGUI finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(320, 45), "Final Skor: 0", 28, TextAlignmentOptions.Center);
        finalScoreText.color = Color.yellow;

        Button restartBtn = CreateButton(gameOverPanel.transform, "RestartButton",
            new Vector2(250, 55), "YENİDEN BAŞLA", 20);

        // === PAUSE PANELİ ===
        GameObject pausePanel = CreateCenteredPanel(canvasObj.transform, "PausePanel",
            new Vector2(350, 280), new Color(0.1f, 0.1f, 0.1f, 0.95f));
        pausePanel.SetActive(false);

        VerticalLayoutGroup pauseLayout = pausePanel.AddComponent<VerticalLayoutGroup>();
        pauseLayout.spacing = 20;
        pauseLayout.padding = new RectOffset(40, 40, 40, 40);
        pauseLayout.childAlignment = TextAnchor.MiddleCenter;
        pauseLayout.childControlWidth = true;
        pauseLayout.childControlHeight = false;

        TextMeshProUGUI pauseTitle = CreateText(pausePanel.transform, "PauseTitle",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(270, 55), "DURAKLATILDI", 34, TextAlignmentOptions.Center);
        pauseTitle.color = Color.white;
        pauseTitle.fontStyle = FontStyles.Bold;

        Button resumeBtn = CreateButton(pausePanel.transform, "ResumeButton",
            new Vector2(220, 50), "DEVAM ET", 18);

        Button mainMenuBtn = CreateButton(pausePanel.transform, "MainMenuButton",
            new Vector2(220, 50), "ANA MENÜ", 18);
        mainMenuBtn.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);

        // === UIManager Referanslarını Ata ===
        AssignUIReferences(uiManager, healthSlider, fillImage, healthText, livesText, 
            scoreText, levelText, nextLevelText, gameOverPanel, pausePanel, finalScoreText);

        // Buton eventlerini bağla
        restartBtn.onClick.AddListener(() => uiManager.OnRestartButtonClicked());
        resumeBtn.onClick.AddListener(() => uiManager.OnResumeButtonClicked());
        mainMenuBtn.onClick.AddListener(() => uiManager.OnMainMenuButtonClicked());

        Debug.Log("UI sistemi oluşturuldu.");
    }

    /// <summary>
    /// UIManager referanslarını reflection ile atar
    /// </summary>
    private void AssignUIReferences(UIManager uiManager, Slider healthBar, Image healthFill,
        TextMeshProUGUI healthText, TextMeshProUGUI livesText, TextMeshProUGUI scoreText,
        TextMeshProUGUI levelText, TextMeshProUGUI nextLevelText, GameObject gameOverPanel,
        GameObject pausePanel, TextMeshProUGUI finalScoreText)
    {
        var type = typeof(UIManager);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        type.GetField("healthBar", flags)?.SetValue(uiManager, healthBar);
        type.GetField("healthFill", flags)?.SetValue(uiManager, healthFill);
        type.GetField("healthText", flags)?.SetValue(uiManager, healthText);
        type.GetField("livesText", flags)?.SetValue(uiManager, livesText);
        type.GetField("scoreText", flags)?.SetValue(uiManager, scoreText);
        type.GetField("levelText", flags)?.SetValue(uiManager, levelText);
        type.GetField("nextLevelText", flags)?.SetValue(uiManager, nextLevelText);
        type.GetField("gameOverPanel", flags)?.SetValue(uiManager, gameOverPanel);
        type.GetField("pausePanel", flags)?.SetValue(uiManager, pausePanel);
        type.GetField("finalScoreText", flags)?.SetValue(uiManager, finalScoreText);
    }

    #region UI Helper Methods

    private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, 
        Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        return panel;
    }

    private GameObject CreateCenteredPanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = color;

        return panel;
    }

    private GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 position, Vector2 size, Color color)
    {
        GameObject imgObj = new GameObject(name);
        imgObj.transform.SetParent(parent, false);

        RectTransform rect = imgObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = imgObj.AddComponent<Image>();
        img.color = color;

        return imgObj;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 position, Vector2 size, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        // Layout element ekle
        LayoutElement le = textObj.AddComponent<LayoutElement>();
        le.preferredHeight = size.y;

        return tmp;
    }

    private Button CreateButton(Transform parent, string name, Vector2 size, string text, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.2f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.2f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.1f);
        btn.colors = colors;

        // Layout element ekle
        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = size.y;
        le.preferredWidth = size.x;

        // Button Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = fontSize;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        return btn;
    }

    #endregion

    /// <summary>
    /// Haritayı oluşturur
    /// </summary>
    private void GenerateMap()
    {
        MapGenerator mapGen = FindAnyObjectByType<MapGenerator>();
        if (mapGen == null)
        {
            GameObject mapGenObj = new GameObject("MapGenerator");
            mapGen = mapGenObj.AddComponent<MapGenerator>();
        }

        // Harita tipini ayarla ve oluştur
        mapGen.SetMapType(startingMap);
        mapGen.GenerateMap();
    }

    /// <summary>
    /// Oyuncuyu spawn eder
    /// </summary>
    private void SpawnPlayer()
    {
        if (characterSelector == null)
        {
            characterSelector = FindAnyObjectByType<CharacterSelector>();
            if (characterSelector == null)
            {
                GameObject selectorObj = new GameObject("CharacterSelector");
                characterSelector = selectorObj.AddComponent<CharacterSelector>();
                characterSelector.SetPrefabs(rangedCharacterPrefab, meleeCharacterPrefab, trapperCharacterPrefab);
            }
        }

        CharacterSelector.SelectedCharacter = startingCharacter;
        characterSelector.SpawnSelectedCharacter();

        // Player spawn edildikten sonra referansları ata
        SetupPlayerReferences();

        // EnemySpawner'a oyuncuyu bildir
        if (enemySpawner != null)
        {
            enemySpawner.FindPlayer();
        }
    }

    /// <summary>
    /// Oyuncu referanslarını ayarlar
    /// </summary>
    private void SetupPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Attack Point'i bul ve ata
        Transform attackPoint = player.transform.Find("AttackPoint");
        
        PlayerBase playerBase = player.GetComponent<PlayerBase>();
        if (playerBase != null && attackPoint != null)
        {
            var type = typeof(PlayerBase);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            type.GetField("attackPoint", flags)?.SetValue(playerBase, attackPoint);
        }
    }

    /// <summary>
    /// Kamerayı ayarlar
    /// </summary>
    private void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null)
            {
                camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            }
        }
    }

    /// <summary>
    /// Test için hızlı başlatma
    /// </summary>
    [ContextMenu("Quick Start Game")]
    public void QuickStartGame()
    {
        // Önce mevcut objeleri temizle
        CleanupExistingObjects();
        
        CreateGameSystems();
        GenerateMap();
        SpawnPlayer();
        SetupCamera();

        Debug.Log("Oyun başlatıldı!");
    }

    /// <summary>
    /// Mevcut objeleri temizler (yeniden başlatma için)
    /// </summary>
    private void CleanupExistingObjects()
    {
        // Mevcut Canvas'ı sil
        Canvas existingCanvas = FindAnyObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas.gameObject);
        }

        // Mevcut MapGenerator'ı sil
        MapGenerator existingMap = FindAnyObjectByType<MapGenerator>();
        if (existingMap != null)
        {
            existingMap.ClearMap();
            DestroyImmediate(existingMap.gameObject);
        }

        // Mevcut Player'ı sil
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            DestroyImmediate(existingPlayer);
        }

        // Mevcut CharacterSelector'ı sil
        CharacterSelector existingSelector = FindAnyObjectByType<CharacterSelector>();
        if (existingSelector != null)
        {
            DestroyImmediate(existingSelector.gameObject);
        }

        // Düşmanları temizle
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.DestroyAllEnemies();
            DestroyImmediate(spawner.gameObject);
        }

        // Referansları temizle
        characterSelector = null;
        enemySpawner = null;
    }

    /// <summary>
    /// Runtime'da prefabları değiştir
    /// </summary>
    public void SetCharacterPrefabs(GameObject ranged, GameObject melee, GameObject trapper)
    {
        rangedCharacterPrefab = ranged;
        meleeCharacterPrefab = melee;
        trapperCharacterPrefab = trapper;

        if (characterSelector != null)
        {
            characterSelector.SetPrefabs(ranged, melee, trapper);
        }
    }

    /// <summary>
    /// Runtime'da düşman prefablarını değiştir
    /// </summary>
    public void SetEnemyPrefabs(GameObject[] enemies, GameObject defaultEnemy = null)
    {
        enemyPrefabs = enemies;
        if (defaultEnemy != null)
        {
            defaultEnemyPrefab = defaultEnemy;
        }

        SetupEnemySpawnerPrefabs();
    }
}
