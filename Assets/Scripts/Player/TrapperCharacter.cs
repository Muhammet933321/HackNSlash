using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Tuzakçı karakter - Tuzak ve patlayıcı yerleştirir
/// </summary>
public class TrapperCharacter : PlayerBase
{
    [Header("Diken Tuzak Ayarları (Sol Tık)")]
    [SerializeField] private GameObject spikeTrapPrefab; // Diken tuzak prefab'ı
    [SerializeField] private int maxTraps = 5;
    [SerializeField] private float trapDuration = 15f;
    [SerializeField] private float spikeDamageInterval = 0.3f; // Diken hasar aralığı

    [Header("Patlayıcı Ayarları (Sağ Tık)")]
    [SerializeField] private GameObject explosivePrefab;
    [SerializeField] private int maxExplosives = 3;
    [SerializeField] private float explosiveDelay = 3f;
    [SerializeField] private float explosiveRadius = 4f;

    [Header("Patlama Efekti")]
    [SerializeField] private GameObject explosionEffectPrefab; // Patlama particle system
    [SerializeField] private float explosionEffectDuration = 1.5f;

    [Header("Yerleştirme")]
    [SerializeField] private float placementDistance = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float trapPlacementCooldown = 1f;    // Tuzak yerleştirme cooldown
    [SerializeField] private float explosivePlacementCooldown = 2f; // Patlayıcı yerleştirme cooldown

    // Aktif tuzak ve patlayıcılar
    private List<GameObject> activeTraps = new List<GameObject>();
    private List<GameObject> activeExplosives = new List<GameObject>();
    
    // Cooldown takibi
    private float lastTrapTime;
    private float lastExplosiveTime;

    protected override void Awake()
    {
        base.Awake();
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default", "Ground");
        }
        
        // Cooldown'ları ayarla
        attackCooldown = trapPlacementCooldown;
        secondaryAttackCooldown = explosivePlacementCooldown;
    }

    /// <summary>
    /// Cooldown UI'ını günceller (override)
    /// </summary>
    protected override void UpdateCooldownUI()
    {
        if (UIManager.Instance == null) return;

        // Birincil (tuzak) cooldown
        float primaryElapsed = Time.time - lastTrapTime;
        float primaryRemaining = Mathf.Max(0, trapPlacementCooldown - primaryElapsed);
        float primaryFill = primaryRemaining / trapPlacementCooldown;
        UIManager.Instance.UpdatePrimaryCooldown(primaryFill, primaryRemaining);

        // İkincil (patlayıcı) cooldown
        float secondaryElapsed = Time.time - lastExplosiveTime;
        float secondaryRemaining = Mathf.Max(0, explosivePlacementCooldown - secondaryElapsed);
        float secondaryFill = secondaryRemaining / explosivePlacementCooldown;
        UIManager.Instance.UpdateSecondaryCooldown(secondaryFill, secondaryRemaining);
    }

    protected override string GetCharacterName()
    {
        return "Tuzakçı";
    }

    protected override void PerformPrimaryAttack()
    {
        PlaceTrap();
    }

    protected override void PerformSecondaryAttack()
    {
        PlaceExplosive();
    }

    /// <summary>
    /// Diken tuzak yerleştirir (Sol Tık)
    /// </summary>
    private void PlaceTrap()
    {
        // Eski tuzakları temizle
        CleanupDestroyedObjects(activeTraps);

        if (activeTraps.Count >= maxTraps)
        {
            Debug.Log("Maksimum diken tuzak sayısına ulaşıldı!");
            return;
        }

        // Cooldown takibi
        lastTrapTime = Time.time;

        Vector3 placementPos = GetPlacementPosition();
        GameObject trap;
        
        if (spikeTrapPrefab != null)
        {
            // Spike prefab kullan
            trap = Instantiate(spikeTrapPrefab, placementPos, Quaternion.identity);
            
            // Trap component yoksa ekle
            Trap trapComponent = trap.GetComponent<Trap>();
            if (trapComponent == null)
            {
                trapComponent = trap.AddComponent<Trap>();
            }
            trapComponent.Initialize(currentDamage, trapDuration, spikeDamageInterval);
            
            // Collider'ı trigger yap
            Collider col = trap.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        else
        {
            // Varsayılan diken tuzak oluştur
            trap = CreateDefaultSpikeTrap(placementPos);
        }
        
        activeTraps.Add(trap);
        Debug.Log($"Diken tuzak yerleştirildi! Aktif: {activeTraps.Count}/{maxTraps}");
    }

    /// <summary>
    /// Patlayıcı yerleştirir (Sağ Tık)
    /// </summary>
    private void PlaceExplosive()
    {
        CleanupDestroyedObjects(activeExplosives);

        if (activeExplosives.Count >= maxExplosives)
        {
            Debug.Log("Maksimum patlayıcı sayısına ulaşıldı!");
            return;
        }

        // Cooldown takibi
        lastExplosiveTime = Time.time;

        Vector3 placementPos = GetPlacementPosition();
        GameObject explosive;
        
        if (explosivePrefab != null)
        {
            explosive = Instantiate(explosivePrefab, placementPos, Quaternion.identity);
        }
        else
        {
            // Varsayılan patlayıcı oluştur
            explosive = CreateDefaultExplosive(placementPos);
        }
        
        // Explosive component ayarla
        Explosive explosiveComponent = explosive.GetComponent<Explosive>();
        if (explosiveComponent == null)
        {
            explosiveComponent = explosive.AddComponent<Explosive>();
        }
        
        // Patlama efekti prefab'ını ver
        explosiveComponent.Initialize(currentDamage * 2f, explosiveDelay, explosiveRadius, explosionEffectPrefab, explosionEffectDuration);
        
        activeExplosives.Add(explosive);
        Debug.Log($"Patlayıcı yerleştirildi! Aktif: {activeExplosives.Count}/{maxExplosives}");
    }

    /// <summary>
    /// Yerleştirme pozisyonunu hesaplar
    /// </summary>
    private Vector3 GetPlacementPosition()
    {
        Vector3 targetPos = transform.position + transform.forward * placementDistance;
        
        // Zemine yapıştır
        if (Physics.Raycast(targetPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            return hit.point + Vector3.up * 0.1f;
        }
        
        return targetPos;
    }

    /// <summary>
    /// Yok edilmiş objeleri listeden temizler
    /// </summary>
    private void CleanupDestroyedObjects(List<GameObject> list)
    {
        list.RemoveAll(obj => obj == null);
    }

    /// <summary>
    /// Varsayılan diken tuzak oluşturur
    /// </summary>
    private GameObject CreateDefaultSpikeTrap(Vector3 position)
    {
        // Ana tuzak objesi
        GameObject trap = new GameObject("SpikeTrap");
        trap.transform.position = position;
        
        // Zemin plakası (görünmez trigger)
        GameObject base_plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        base_plate.name = "BasePlate";
        base_plate.transform.SetParent(trap.transform);
        base_plate.transform.localPosition = Vector3.zero;
        base_plate.transform.localScale = new Vector3(2f, 0.05f, 2f);
        base_plate.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.1f, 0.5f); // Koyu kahve
        Destroy(base_plate.GetComponent<Collider>()); // Collider'ı kaldır
        
        // Dikenler oluştur (5 adet)
        CreateSpike(trap.transform, Vector3.zero, 0.4f);
        CreateSpike(trap.transform, new Vector3(0.5f, 0, 0.3f), 0.3f);
        CreateSpike(trap.transform, new Vector3(-0.4f, 0, 0.4f), 0.35f);
        CreateSpike(trap.transform, new Vector3(0.3f, 0, -0.5f), 0.32f);
        CreateSpike(trap.transform, new Vector3(-0.5f, 0, -0.3f), 0.28f);
        
        // Ana collider ekle (trigger)
        BoxCollider boxCollider = trap.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(2f, 1f, 2f);
        boxCollider.center = new Vector3(0, 0.5f, 0);
        boxCollider.isTrigger = true;
        
        // Trap component ekle
        Trap trapComponent = trap.AddComponent<Trap>();
        trapComponent.Initialize(currentDamage, trapDuration, spikeDamageInterval);
        
        return trap;
    }

    /// <summary>
    /// Tek bir diken oluşturur
    /// </summary>
    private void CreateSpike(Transform parent, Vector3 localPos, float height)
    {
        GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spike.name = "Spike";
        spike.transform.SetParent(parent);
        spike.transform.localPosition = localPos + Vector3.up * (height * 0.5f);
        spike.transform.localScale = new Vector3(0.15f, height, 0.15f);
        
        // Gri metalik renk
        Renderer rend = spike.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.4f, 0.4f, 0.45f); // Gri
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 0.6f);
        rend.sharedMaterial = mat;
        
        // Diken ucunu koy
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "SpikeTip";
        tip.transform.SetParent(spike.transform);
        tip.transform.localPosition = new Vector3(0, 0.5f, 0);
        tip.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
        tip.GetComponent<Renderer>().sharedMaterial = mat;
        
        // Collider'ları kaldır (ana objede var)
        Destroy(spike.GetComponent<Collider>());
        Destroy(tip.GetComponent<Collider>());
    }

    /// <summary>
    /// Varsayılan patlayıcı oluşturur
    /// </summary>
    private GameObject CreateDefaultExplosive(Vector3 position)
    {
        GameObject explosive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosive.name = "Explosive";
        explosive.transform.position = position;
        explosive.transform.localScale = Vector3.one * 0.5f;
        explosive.GetComponent<Renderer>().material.color = Color.red;
        
        Explosive explosiveComponent = explosive.AddComponent<Explosive>();
        explosiveComponent.Initialize(currentDamage * 2f, explosiveDelay, explosiveRadius);
        
        activeExplosives.Add(explosive);
        return explosive;
    }

    /// <summary>
    /// Tüm patlayıcıları manuel olarak patlatır (E tuşu ile)
    /// </summary>
    protected override void Update()
    {
        base.Update();

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            DetonateAllExplosives();
        }
    }

    /// <summary>
    /// Tüm patlayıcıları patlatır
    /// </summary>
    private void DetonateAllExplosives()
    {
        CleanupDestroyedObjects(activeExplosives);
        
        foreach (GameObject explosive in activeExplosives)
        {
            if (explosive != null)
            {
                Explosive exp = explosive.GetComponent<Explosive>();
                if (exp != null)
                {
                    exp.Detonate();
                }
            }
        }
        activeExplosives.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        // Yerleştirme pozisyonunu göster
        Gizmos.color = Color.green;
        Vector3 placementPos = transform.position + transform.forward * placementDistance;
        Gizmos.DrawWireSphere(placementPos, 0.5f);

        // Patlama yarıçapını göster
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(placementPos, explosiveRadius);
    }
}
