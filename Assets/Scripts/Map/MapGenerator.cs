using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

/// <summary>
/// Basit harita oluşturucu - Engeller, yollar ve engebeler
/// </summary>
public class MapGenerator : MonoBehaviour
{
    /// <summary>
    /// Renderer'a yeni material atayıp renk verir (edit mode için güvenli)
    /// </summary>
    private void SetRendererColor(Renderer rend, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        rend.sharedMaterial = mat;
    }
    public enum MapType
    {
        Forest,     // Orman - Ağaç engelleri
        Desert,     // Çöl - Kaya engelleri  
        City        // Şehir - Bina engelleri
    }

    [Header("Harita Ayarları")]
    [SerializeField] private MapType mapType = MapType.Forest;
    [SerializeField] private Vector2 mapSize = new Vector2(50, 50);
    [SerializeField] private float groundHeight = 0f;
    [SerializeField] private bool useGameSettings = true; // Ana menüden gelen seçimi kullan
    [SerializeField] private bool autoGenerateOnStart = true; // Başlangıçta otomatik oluştur

    [Header("Zemin")]
    [SerializeField] private Material groundMaterial;
    [SerializeField] private Color groundColor = new Color(0.3f, 0.5f, 0.3f);

    [Header("Engeller")]
    [SerializeField] private int obstacleCount = 20;
    [SerializeField] private float minObstacleSize = 1f;
    [SerializeField] private float maxObstacleSize = 3f;
    [SerializeField] private float safeZoneRadius = 5f; // Spawn noktası etrafında engel olmasın

    [Header("Engebeler (Tepeler)")]
    [SerializeField] private int hillCount = 5;
    [SerializeField] private float minHillSize = 5f;
    [SerializeField] private float maxHillSize = 10f;
    [SerializeField] private float hillHeightMultiplier = 0.4f; // Tepe yükseklik çarpanı
    [SerializeField] private float hillElevation = 0.3f; // Zeminden yükselme miktarı (0-1)

    [Header("Duvarlar")]
    [SerializeField] private bool createWalls = true;
    [SerializeField] private float wallHeight = 3f;

    [Header("NavMesh")]
    [SerializeField] private bool bakeNavMeshOnGenerate = true; // Harita oluşturulduğunda NavMesh bake et

    // Oluşturulan objeler
    private List<GameObject> generatedObjects = new List<GameObject>();
    private GameObject mapRoot;
    private NavMeshSurface navMeshSurface;

    private void Start()
    {
        // Ana menüden gelen seçimi uygula
        if (useGameSettings)
        {
            ApplyGameSettings();
        }

        // Otomatik oluşturma
        if (autoGenerateOnStart)
        {
            GenerateMap();
        }
    }

    /// <summary>
    /// Ana menüdeki GameSettings'den harita stilini uygular
    /// </summary>
    private void ApplyGameSettings()
    {
        int mapIndex = GameSettings.SelectedMapStyle;
        
        switch (mapIndex)
        {
            case 0:
                mapType = MapType.Forest;
                break;
            case 1:
                mapType = MapType.Desert;
                break;
            case 2:
                mapType = MapType.City;
                break;
            default:
                mapType = MapType.Forest;
                break;
        }
        
        Debug.Log($"GameSettings'den harita yüklendi: {mapType}");
    }

    /// <summary>
    /// Harita tipini ayarlar
    /// </summary>
    public void SetMapType(MapType type)
    {
        mapType = type;
    }

    /// <summary>
    /// Haritayı oluşturur
    /// </summary>
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        ClearMap();

        mapRoot = new GameObject($"Map_{mapType}");
        mapRoot.transform.SetParent(transform);

        CreateGround();
        CreateObstacles();
        CreateHills();

        if (createWalls)
        {
            CreateWalls();
        }

        // NavMesh bake et
        if (bakeNavMeshOnGenerate)
        {
            BakeNavMesh();
        }

        Debug.Log($"Harita oluşturuldu: {mapType}");
    }

    /// <summary>
    /// Haritayı temizler
    /// </summary>
    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        generatedObjects.Clear();

        if (mapRoot != null)
        {
            DestroyImmediate(mapRoot);
        }
    }

    /// <summary>
    /// NavMesh'i runtime'da bake eder
    /// </summary>
    [ContextMenu("Bake NavMesh")]
    public void BakeNavMesh()
    {
        // NavMeshSurface component'i kontrol et veya ekle
        navMeshSurface = GetComponent<NavMeshSurface>();
        
        if (navMeshSurface == null)
        {
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            Debug.Log("NavMeshSurface component eklendi.");
        }

        // NavMeshSurface ayarları
        navMeshSurface.collectObjects = CollectObjects.Children; // Sadece child objeleri topla
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; // Collider'ları kullan
        
        // NavMesh'i bake et
        navMeshSurface.BuildNavMesh();
        
        Debug.Log("NavMesh başarıyla bake edildi!");
    }

    /// <summary>
    /// Zemin oluşturur
    /// </summary>
    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(mapRoot.transform);
        ground.transform.position = new Vector3(0, groundHeight - 0.5f, 0);
        ground.transform.localScale = new Vector3(mapSize.x, 1f, mapSize.y);
        ground.tag = "Ground";
        ground.layer = LayerMask.NameToLayer("Default");

        // Renk
        Renderer rend = ground.GetComponent<Renderer>();
        if (groundMaterial != null)
        {
            rend.sharedMaterial = groundMaterial;
        }
        else
        {
            SetRendererColor(rend, GetGroundColorForMap());
        }

        generatedObjects.Add(ground);
    }

    /// <summary>
    /// Engeller oluşturur
    /// </summary>
    private void CreateObstacles()
    {
        GameObject obstacleParent = new GameObject("Obstacles");
        obstacleParent.transform.SetParent(mapRoot.transform);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 position = GetRandomPosition();
            
            // Güvenli bölge kontrolü
            if (Vector3.Distance(position, Vector3.zero) < safeZoneRadius)
            {
                i--;
                continue;
            }

            GameObject obstacle = CreateObstacleForMap(position);
            obstacle.transform.SetParent(obstacleParent.transform);
            obstacle.tag = "Obstacle";
            obstacle.layer = LayerMask.NameToLayer("Default");

            generatedObjects.Add(obstacle);
        }
    }

    /// <summary>
    /// Harita tipine göre engel oluşturur
    /// </summary>
    private GameObject CreateObstacleForMap(Vector3 position)
    {
        GameObject obstacle;
        float size = Random.Range(minObstacleSize, maxObstacleSize);

        switch (mapType)
        {
            case MapType.Forest:
                // Ağaç - Silindir gövde + Küre yaprak
                obstacle = CreateTree(position, size);
                break;
            case MapType.Desert:
                // Kaya - Küp
                obstacle = CreateRock(position, size);
                break;
            case MapType.City:
                // Bina - Dikdörtgen
                obstacle = CreateBuilding(position, size);
                break;
            default:
                obstacle = CreateRock(position, size);
                break;
        }

        return obstacle;
    }

    /// <summary>
    /// Ağaç oluşturur
    /// </summary>
    private GameObject CreateTree(Vector3 position, float size)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.position = position;

        // Gövde
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, size, 0);
        trunk.transform.localScale = new Vector3(size * 0.3f, size, size * 0.3f);
        SetRendererColor(trunk.GetComponent<Renderer>(), new Color(0.4f, 0.25f, 0.1f));

        // Yapraklar
        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.name = "Leaves";
        leaves.transform.SetParent(tree.transform);
        leaves.transform.localPosition = new Vector3(0, size * 2.2f, 0);
        leaves.transform.localScale = Vector3.one * size * 1.5f;
        SetRendererColor(leaves.GetComponent<Renderer>(), new Color(0.1f, 0.5f, 0.1f));

        return tree;
    }

    /// <summary>
    /// Kaya oluşturur
    /// </summary>
    private GameObject CreateRock(Vector3 position, float size)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.name = "Rock";
        rock.transform.position = position + Vector3.up * size * 0.5f;
        rock.transform.localScale = new Vector3(size, size, size) * 0.8f;
        rock.transform.rotation = Quaternion.Euler(
            Random.Range(-15f, 15f),
            Random.Range(0f, 360f),
            Random.Range(-15f, 15f)
        );
        SetRendererColor(rock.GetComponent<Renderer>(), new Color(0.5f, 0.45f, 0.4f));

        return rock;
    }

    /// <summary>
    /// Bina oluşturur
    /// </summary>
    private GameObject CreateBuilding(Vector3 position, float size)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = "Building";
        float height = size * Random.Range(1.5f, 3f);
        building.transform.position = position + Vector3.up * height * 0.5f;
        building.transform.localScale = new Vector3(size, height, size);
        SetRendererColor(building.GetComponent<Renderer>(), new Color(0.6f, 0.6f, 0.65f));

        return building;
    }

    /// <summary>
    /// Tepeler/Engebeler oluşturur
    /// </summary>
    private void CreateHills()
    {
        GameObject hillParent = new GameObject("Hills");
        hillParent.transform.SetParent(mapRoot.transform);

        for (int i = 0; i < hillCount; i++)
        {
            Vector3 position = GetRandomPosition();
            
            // Güvenli bölge kontrolü
            if (Vector3.Distance(position, Vector3.zero) < safeZoneRadius)
            {
                i--;
                continue;
            }

            float size = Random.Range(minHillSize, maxHillSize);
            float height = size * hillHeightMultiplier; // Tepe yüksekliği

            // Tepe oluştur - yarısı zemin üstünde olacak şekilde
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = "Hill";
            hill.transform.SetParent(hillParent.transform);
            
            // Pozisyon hesaplama:
            // Sphere scale Y = height, yani yarıçap = height/2
            // Zeminin üstünde görünmesi için: groundHeight + (height * elevation)
            float yPos = groundHeight + (height * hillElevation);
            hill.transform.position = new Vector3(position.x, yPos, position.z);
            hill.transform.localScale = new Vector3(size, height, size);
            
            // Rastgele rotasyon ekle (daha doğal görünüm)
            hill.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            // Renk ayarla
            SetRendererColor(hill.GetComponent<Renderer>(), GetGroundColorForMap() * 0.85f);
            
            // SphereCollider'ı kaldır ve MeshCollider ekle
            SphereCollider sphereCol = hill.GetComponent<SphereCollider>();
            if (sphereCol != null)
            {
                DestroyImmediate(sphereCol);
            }
            
            MeshCollider meshCol = hill.AddComponent<MeshCollider>();
            meshCol.convex = true; // NavMesh ve fizik için convex olmalı
            
            hill.layer = LayerMask.NameToLayer("Default");
            hill.tag = "Ground"; // Zemin gibi davransın
            
            // Navigation Static olarak işaretle (NavMesh için)
            hill.isStatic = true;

            generatedObjects.Add(hill);
        }
    }

    /// <summary>
    /// Çevre duvarları oluşturur
    /// </summary>
    private void CreateWalls()
    {
        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(mapRoot.transform);

        float halfX = mapSize.x / 2f;
        float halfY = mapSize.y / 2f;
        Color wallColor = new Color(0.3f, 0.3f, 0.35f);

        // Kuzey duvarı
        CreateWall(wallParent.transform, new Vector3(0, wallHeight / 2f, halfY), 
                   new Vector3(mapSize.x, wallHeight, 1f), wallColor);
        // Güney duvarı
        CreateWall(wallParent.transform, new Vector3(0, wallHeight / 2f, -halfY), 
                   new Vector3(mapSize.x, wallHeight, 1f), wallColor);
        // Doğu duvarı
        CreateWall(wallParent.transform, new Vector3(halfX, wallHeight / 2f, 0), 
                   new Vector3(1f, wallHeight, mapSize.y), wallColor);
        // Batı duvarı
        CreateWall(wallParent.transform, new Vector3(-halfX, wallHeight / 2f, 0), 
                   new Vector3(1f, wallHeight, mapSize.y), wallColor);
    }

    /// <summary>
    /// Tek duvar oluşturur
    /// </summary>
    private void CreateWall(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetRendererColor(wall.GetComponent<Renderer>(), color);
        wall.tag = "Wall";
        wall.layer = LayerMask.NameToLayer("Default");

        generatedObjects.Add(wall);
    }

    /// <summary>
    /// Rastgele pozisyon döndürür
    /// </summary>
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-mapSize.x / 2f + 2f, mapSize.x / 2f - 2f);
        float z = Random.Range(-mapSize.y / 2f + 2f, mapSize.y / 2f - 2f);
        return new Vector3(x, groundHeight, z);
    }

    /// <summary>
    /// Harita tipine göre zemin rengi döndürür
    /// </summary>
    private Color GetGroundColorForMap()
    {
        switch (mapType)
        {
            case MapType.Forest:
                return new Color(0.2f, 0.4f, 0.15f); // Yeşil
            case MapType.Desert:
                return new Color(0.8f, 0.7f, 0.4f); // Sarı-kahve
            case MapType.City:
                return new Color(0.4f, 0.4f, 0.45f); // Gri
            default:
                return groundColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Harita sınırlarını göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize.x, 1, mapSize.y));

        // Güvenli bölgeyi göster
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, safeZoneRadius);
    }
}
