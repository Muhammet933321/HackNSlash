using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Ana menü UI yöneticisi - Tüm menü panellerini ve butonları kontrol eder
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject playCanvas;
    [SerializeField] private GameObject howToPlayCanvas;

    [Header("Ana Menü Butonları")]
    [SerializeField] private Button playGameButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button exitButton;

    [Header("Play Canvas Butonları")]
    [SerializeField] private Button meleeButton;
    [SerializeField] private Button rangedButton;
    [SerializeField] private Button trapperButton;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button desertButton;
    [SerializeField] private Button cityButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button playBackButton;

    [Header("How To Play Butonları")]
    [SerializeField] private Button howToPlayBackButton;

    [Header("Sahne Ayarları")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Seçim Göstergeleri (Opsiyonel)")]
    [SerializeField] private Color selectedColor = new Color(0f, 0.8f, 0f);
    [SerializeField] private Color normalColor = new Color(0f, 0.4f, 0f);

    // Seçili değerler
    private int selectedCharacter = 0; // 0: Melee, 1: Ranged, 2: Trapper
    private int selectedMapStyle = 0;  // 0: Forest, 1: Desert, 2: City

    // Buton referansları (renk değiştirmek için)
    private Button[] characterButtons;
    private Button[] mapButtons;

    private void Start()
    {
        // Buton dizilerini oluştur
        characterButtons = new Button[] { meleeButton, rangedButton, trapperButton };
        mapButtons = new Button[] { forestButton, desertButton, cityButton };

        // Başlangıçta sadece ana menüyü göster
        ShowMainMenu();

        // Buton listener'larını ayarla
        SetupButtonListeners();

        // Varsayılan seçimleri göster
        UpdateCharacterSelection(0);
        UpdateMapSelection(0);
    }

    /// <summary>
    /// Tüm butonlara listener ekler
    /// </summary>
    private void SetupButtonListeners()
    {
        // Ana Menü Butonları
        if (playGameButton != null)
            playGameButton.onClick.AddListener(OnPlayGameClicked);
        
        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        // Play Canvas Butonları - Karakter Seçimi
        if (meleeButton != null)
            meleeButton.onClick.AddListener(() => OnCharacterSelected(0));
        
        if (rangedButton != null)
            rangedButton.onClick.AddListener(() => OnCharacterSelected(1));
        
        if (trapperButton != null)
            trapperButton.onClick.AddListener(() => OnCharacterSelected(2));

        // Play Canvas Butonları - Harita Seçimi
        if (forestButton != null)
            forestButton.onClick.AddListener(() => OnMapSelected(0));
        
        if (desertButton != null)
            desertButton.onClick.AddListener(() => OnMapSelected(1));
        
        if (cityButton != null)
            cityButton.onClick.AddListener(() => OnMapSelected(2));

        // Oyunu Başlat
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);

        // Geri Butonları
        if (playBackButton != null)
            playBackButton.onClick.AddListener(OnBackToMainMenu);
        
        if (howToPlayBackButton != null)
            howToPlayBackButton.onClick.AddListener(OnBackToMainMenu);
    }

    #region Panel Geçişleri

    /// <summary>
    /// Ana menüyü gösterir
    /// </summary>
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuCanvas);
    }

    /// <summary>
    /// Play Canvas'ı gösterir
    /// </summary>
    public void ShowPlayCanvas()
    {
        SetActivePanel(playCanvas);
    }

    /// <summary>
    /// How To Play Canvas'ı gösterir
    /// </summary>
    public void ShowHowToPlayCanvas()
    {
        SetActivePanel(howToPlayCanvas);
    }

    /// <summary>
    /// Belirtilen paneli aktif yapar, diğerlerini kapatır
    /// </summary>
    private void SetActivePanel(GameObject activePanel)
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(mainMenuCanvas == activePanel);
        
        if (playCanvas != null)
            playCanvas.SetActive(playCanvas == activePanel);
        
        if (howToPlayCanvas != null)
            howToPlayCanvas.SetActive(howToPlayCanvas == activePanel);
    }

    #endregion

    #region Buton İşlevleri

    /// <summary>
    /// Play Game butonuna basıldığında
    /// </summary>
    private void OnPlayGameClicked()
    {
        Debug.Log("Play Game tıklandı");
        ShowPlayCanvas();
    }

    /// <summary>
    /// How To Play butonuna basıldığında
    /// </summary>
    private void OnHowToPlayClicked()
    {
        Debug.Log("How To Play tıklandı");
        ShowHowToPlayCanvas();
    }

    /// <summary>
    /// Exit butonuna basıldığında
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Oyundan çıkılıyor...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Karakter seçildiğinde
    /// </summary>
    private void OnCharacterSelected(int characterIndex)
    {
        selectedCharacter = characterIndex;
        UpdateCharacterSelection(characterIndex);
        
        string[] characterNames = { "Melee", "Ranged", "Trapper" };
        Debug.Log($"Karakter seçildi: {characterNames[characterIndex]}");
    }

    /// <summary>
    /// Harita seçildiğinde
    /// </summary>
    private void OnMapSelected(int mapIndex)
    {
        selectedMapStyle = mapIndex;
        UpdateMapSelection(mapIndex);
        
        string[] mapNames = { "Forest", "Desert", "City" };
        Debug.Log($"Harita seçildi: {mapNames[mapIndex]}");
    }

    /// <summary>
    /// Oyunu başlat butonuna basıldığında
    /// </summary>
    private void OnStartGameClicked()
    {
        // Seçimleri kaydet (diğer sahnede kullanmak için)
        GameSettings.SelectedCharacter = selectedCharacter;
        GameSettings.SelectedMapStyle = selectedMapStyle;
        
        Debug.Log($"Oyun başlatılıyor! Karakter: {selectedCharacter}, Harita: {selectedMapStyle}");
        
        // Oyun sahnesini yükle
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Geri butonuna basıldığında
    /// </summary>
    private void OnBackToMainMenu()
    {
        Debug.Log("Ana menüye dönülüyor");
        ShowMainMenu();
    }

    #endregion

    #region Görsel Güncellemeler

    /// <summary>
    /// Karakter butonlarının görselini günceller
    /// </summary>
    private void UpdateCharacterSelection(int selectedIndex)
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] != null)
            {
                ColorBlock colors = characterButtons[i].colors;
                colors.normalColor = (i == selectedIndex) ? selectedColor : normalColor;
                colors.highlightedColor = (i == selectedIndex) ? selectedColor : normalColor;
                colors.selectedColor = (i == selectedIndex) ? selectedColor : normalColor;
                characterButtons[i].colors = colors;
            }
        }
    }

    /// <summary>
    /// Harita butonlarının görselini günceller
    /// </summary>
    private void UpdateMapSelection(int selectedIndex)
    {
        for (int i = 0; i < mapButtons.Length; i++)
        {
            if (mapButtons[i] != null)
            {
                ColorBlock colors = mapButtons[i].colors;
                colors.normalColor = (i == selectedIndex) ? selectedColor : normalColor;
                colors.highlightedColor = (i == selectedIndex) ? selectedColor : normalColor;
                colors.selectedColor = (i == selectedIndex) ? selectedColor : normalColor;
                mapButtons[i].colors = colors;
            }
        }
    }

    #endregion
}

/// <summary>
/// Oyun ayarlarını sahneler arası taşımak için static sınıf
/// </summary>
public static class GameSettings
{
    // Karakter seçimi: 0 = Melee, 1 = Ranged, 2 = Trapper
    public static int SelectedCharacter { get; set; } = 0;
    
    // Harita stili: 0 = Forest, 1 = Desert, 2 = City
    public static int SelectedMapStyle { get; set; } = 0;

    /// <summary>
    /// Seçili karakter tipini döndürür
    /// </summary>
    public static CharacterType GetCharacterType()
    {
        return (CharacterType)SelectedCharacter;
    }

    /// <summary>
    /// Seçili harita stilini döndürür
    /// </summary>
    public static MapStyle GetMapStyle()
    {
        return (MapStyle)SelectedMapStyle;
    }
}

/// <summary>
/// Karakter tipleri enum
/// </summary>
public enum CharacterType
{
    Melee = 0,
    Ranged = 1,
    Trapper = 2
}

/// <summary>
/// Harita stilleri enum
/// </summary>
public enum MapStyle
{
    Forest = 0,
    Desert = 1,
    City = 2
}
