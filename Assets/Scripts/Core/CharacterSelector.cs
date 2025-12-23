using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Karakter seçim sistemi - Prefab tabanlı karakter seçimi ve spawn
/// </summary>
public class CharacterSelector : MonoBehaviour
{
    public enum CharacterType
    {
        Ranged,     // Nişancı
        Melee,      // Savaşçı
        Trapper     // Tuzakçı
    }

    [Header("Karakter Prefabları (Zorunlu)")]
    [Tooltip("Nişancı karakter prefabı")]
    [SerializeField] private GameObject rangedPrefab;
    [Tooltip("Savaşçı karakter prefabı")]
    [SerializeField] private GameObject meleePrefab;
    [Tooltip("Tuzakçı karakter prefabı")]
    [SerializeField] private GameObject trapperPrefab;

    [Header("Spawn Ayarları")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool autoSpawnOnStart = true; // Oyun sahnesinde otomatik spawn
    [SerializeField] private CharacterType defaultCharacter = CharacterType.Ranged;
    [SerializeField] private bool useGameSettings = true; // Ana menüden gelen seçimi kullan

    [Header("Varsayılan Karakter Ayarları (Prefab Yoksa)")]
    [SerializeField] private bool allowDefaultCharacterCreation = true;
    [SerializeField] private Color rangedColor = Color.blue;
    [SerializeField] private Color meleeColor = Color.red;
    [SerializeField] private Color trapperColor = new Color(0.5f, 0f, 0.5f);

    // Seçilen karakter
    private static CharacterType selectedCharacter = CharacterType.Ranged;
    private GameObject currentPlayer;

    // Public properties
    public static CharacterType SelectedCharacter
    {
        get => selectedCharacter;
        set => selectedCharacter = value;
    }

    public GameObject CurrentPlayer => currentPlayer;
    public bool HasRangedPrefab => rangedPrefab != null;
    public bool HasMeleePrefab => meleePrefab != null;
    public bool HasTrapperPrefab => trapperPrefab != null;

    private void Start()
    {
        // Ana menüden gelen seçimi uygula
        if (useGameSettings)
        {
            ApplyGameSettings();
        }

        if (autoSpawnOnStart)
        {
            SpawnCharacter(selectedCharacter);
        }
    }

    /// <summary>
    /// Ana menüdeki GameSettings'den seçimi uygular
    /// </summary>
    private void ApplyGameSettings()
    {
        // GameSettings'deki CharacterType'ı bu sınıfın CharacterType'ına dönüştür
        int charIndex = GameSettings.SelectedCharacter;
        
        // GameSettings: 0=Melee, 1=Ranged, 2=Trapper
        // CharacterSelector: 0=Ranged, 1=Melee, 2=Trapper
        // Dönüşüm gerekli
        switch (charIndex)
        {
            case 0: // GameSettings: Melee
                selectedCharacter = CharacterType.Melee;
                break;
            case 1: // GameSettings: Ranged
                selectedCharacter = CharacterType.Ranged;
                break;
            case 2: // GameSettings: Trapper
                selectedCharacter = CharacterType.Trapper;
                break;
            default:
                selectedCharacter = defaultCharacter;
                break;
        }
        
        Debug.Log($"GameSettings'den karakter yüklendi: {selectedCharacter}");
    }

    /// <summary>
    /// Karakter seçer (UI butonları tarafından çağrılır)
    /// </summary>
    public void SelectCharacter(int characterIndex)
    {
        selectedCharacter = (CharacterType)characterIndex;
        Debug.Log($"Karakter seçildi: {selectedCharacter}");
    }

    /// <summary>
    /// Seçilen karakteri spawn eder
    /// </summary>
    public void SpawnSelectedCharacter()
    {
        SpawnCharacter(selectedCharacter);
    }

    /// <summary>
    /// Belirtilen karakteri spawn eder
    /// </summary>
    public void SpawnCharacter(CharacterType type)
    {
        // Mevcut oyuncuyu yok et
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject prefab = GetPrefabForType(type);

        if (prefab != null)
        {
            currentPlayer = Instantiate(prefab, spawnPos, spawnRot);
            Debug.Log($"Karakter prefabdan spawn edildi: {type}");
        }
        else if (allowDefaultCharacterCreation)
        {
            Debug.LogWarning($"{type} prefabı atanmamış! Varsayılan karakter oluşturuluyor...");
            currentPlayer = CreateDefaultCharacter(type, spawnPos);
        }
        else
        {
            Debug.LogError($"{type} prefabı atanmamış ve varsayılan oluşturma kapalı!");
            return;
        }

        // Tag'i ayarla
        if (currentPlayer != null)
        {
            currentPlayer.tag = "Player";
        }
    }

    /// <summary>
    /// Karakter tipine göre prefab döndürür
    /// </summary>
    private GameObject GetPrefabForType(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Ranged:
                return rangedPrefab;
            case CharacterType.Melee:
                return meleePrefab;
            case CharacterType.Trapper:
                return trapperPrefab;
            default:
                return rangedPrefab;
        }
    }

    /// <summary>
    /// Karakter tipine göre renk döndürür
    /// </summary>
    private Color GetColorForType(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Ranged:
                return rangedColor;
            case CharacterType.Melee:
                return meleeColor;
            case CharacterType.Trapper:
                return trapperColor;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Renderer'a yeni material atayıp renk verir
    /// </summary>
    private void SetRendererColor(Renderer rend, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        rend.sharedMaterial = mat;
    }

    /// <summary>
    /// Varsayılan karakter oluşturur (prefab yoksa)
    /// </summary>
    private GameObject CreateDefaultCharacter(CharacterType type, Vector3 position)
    {
        // Ana gövde
        GameObject player = new GameObject($"Player_{type}");
        player.transform.position = position;
        player.tag = "Player";
        
        int playerLayer = LayerMask.NameToLayer("Player");
        player.layer = playerLayer >= 0 ? playerLayer : 0;

        // Gövde (Küp)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(player.transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.transform.localScale = new Vector3(1f, 1f, 0.5f);

        // Kafa (Küp)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(player.transform);
        head.transform.localPosition = new Vector3(0, 1.25f, 0);
        head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Attack Point
        GameObject attackPoint = new GameObject("AttackPoint");
        attackPoint.transform.SetParent(player.transform);
        attackPoint.transform.localPosition = new Vector3(0, 0.5f, 1f);

        // Rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // CapsuleCollider
        CapsuleCollider col = player.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.75f, 0);
        col.height = 1.5f;
        col.radius = 0.4f;

        // Renk ve Script
        Color characterColor = GetColorForType(type);

        switch (type)
        {
            case CharacterType.Ranged:
                player.AddComponent<RangedCharacter>();
                break;
            case CharacterType.Melee:
                player.AddComponent<MeleeCharacter>();
                break;
            case CharacterType.Trapper:
                player.AddComponent<TrapperCharacter>();
                break;
            default:
                player.AddComponent<RangedCharacter>();
                break;
        }

        // Renkleri uygula
        SetRendererColor(body.GetComponent<Renderer>(), characterColor);
        SetRendererColor(head.GetComponent<Renderer>(), characterColor * 0.7f);

        return player;
    }

    /// <summary>
    /// Prefabları runtime'da ayarlar
    /// </summary>
    public void SetPrefabs(GameObject ranged, GameObject melee, GameObject trapper)
    {
        rangedPrefab = ranged;
        meleePrefab = melee;
        trapperPrefab = trapper;
    }

    /// <summary>
    /// Belirli bir prefabı ayarlar
    /// </summary>
    public void SetPrefab(CharacterType type, GameObject prefab)
    {
        switch (type)
        {
            case CharacterType.Ranged:
                rangedPrefab = prefab;
                break;
            case CharacterType.Melee:
                meleePrefab = prefab;
                break;
            case CharacterType.Trapper:
                trapperPrefab = prefab;
                break;
        }
    }

    /// <summary>
    /// Oyunu başlatır (seçim ekranından)
    /// </summary>
    public void StartGame(int sceneIndex = 1)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Ranged karakteri seçer
    /// </summary>
    public void SelectRanged()
    {
        SelectCharacter(0);
    }

    /// <summary>
    /// Melee karakteri seçer
    /// </summary>
    public void SelectMelee()
    {
        SelectCharacter(1);
    }

    /// <summary>
    /// Trapper karakteri seçer
    /// </summary>
    public void SelectTrapper()
    {
        SelectCharacter(2);
    }

    /// <summary>
    /// Mevcut oyuncuyu yok eder
    /// </summary>
    public void DestroyCurrentPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
    }
}
