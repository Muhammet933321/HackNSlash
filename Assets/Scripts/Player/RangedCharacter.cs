using UnityEngine;

/// <summary>
/// Uzak mesafe saldırı karakteri - Mermi atar
/// </summary>
public class RangedCharacter : PlayerBase
{
    [Header("Ranged Ayarları")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Özel Saldırı - Seri Atış")]
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.1f;
    [SerializeField] private float specialCooldown = 5f;
    [SerializeField] private float burstSpreadAngle = 15f; // Seri atışta yayılma açısı

    [Header("Efektler")]
    [SerializeField] private GameObject muzzleFlashPrefab; // Namlu parlaması
    [SerializeField] private float muzzleFlashDuration = 0.1f;
    
    private float lastSpecialTime;

    protected override void Awake()
    {
        base.Awake();
        // Özel saldırı cooldown'ını ayarla
        secondaryAttackCooldown = specialCooldown;
    }

    /// <summary>
    /// Cooldown UI'ını günceller (override)
    /// </summary>
    protected override void UpdateCooldownUI()
    {
        if (UIManager.Instance == null) return;

        // Birincil saldırı cooldown
        float primaryElapsed = Time.time - lastAttackTime;
        float primaryRemaining = Mathf.Max(0, attackCooldown - primaryElapsed);
        float primaryFill = primaryRemaining / attackCooldown;
        UIManager.Instance.UpdatePrimaryCooldown(primaryFill, primaryRemaining);

        // İkincil saldırı cooldown (specialCooldown kullan)
        float secondaryElapsed = Time.time - lastSpecialTime;
        float secondaryRemaining = Mathf.Max(0, specialCooldown - secondaryElapsed);
        float secondaryFill = secondaryRemaining / specialCooldown;
        UIManager.Instance.UpdateSecondaryCooldown(secondaryFill, secondaryRemaining);
    }

    protected override string GetCharacterName()
    {
        return "Nişancı";
    }

    protected override void PerformPrimaryAttack()
    {
        ShootProjectile(transform.forward);
        ShowMuzzleFlash();
    }

    protected override void PerformSecondaryAttack()
    {
        // Seri atış (burst fire)
        if (Time.time >= lastSpecialTime + specialCooldown)
        {
            StartCoroutine(BurstFire());
            lastSpecialTime = Time.time;
        }
    }

    /// <summary>
    /// Tek mermi atar
    /// </summary>
    private void ShootProjectile(Vector3 direction)
    {
        Vector3 spawnPos = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1.5f;
        GameObject projectile;

        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
        }
        else
        {
            // Varsayılan mermi oluştur
            projectile = CreateRuntimeProjectile(spawnPos, direction);
        }

        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(currentDamage, projectileSpeed, projectileLifetime, direction);
        }
    }

    /// <summary>
    /// Namlu parlaması efekti gösterir
    /// </summary>
    private void ShowMuzzleFlash()
    {
        Vector3 spawnPos = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1f;

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, spawnPos, transform.rotation);
            Destroy(flash, muzzleFlashDuration);
        }
        else
        {
            // Varsayılan muzzle flash oluştur
            CreateDefaultMuzzleFlash(spawnPos);
        }
    }

    /// <summary>
    /// Varsayılan namlu parlaması oluşturur
    /// </summary>
    private void CreateDefaultMuzzleFlash(Vector3 position)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "MuzzleFlash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.4f;

        // Collider kapat
        Destroy(flash.GetComponent<Collider>());

        // Parlak sarı material
        Renderer rend = flash.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.9f, 0.3f, 1f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.yellow * 5f);
        rend.sharedMaterial = mat;

        Destroy(flash, muzzleFlashDuration);
    }

    /// <summary>
    /// Runtime'da varsayılan mermi oluşturur
    /// </summary>
    private GameObject CreateRuntimeProjectile(Vector3 position, Vector3 direction)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.position = position;
        projectile.transform.rotation = transform.rotation;
        projectile.transform.localScale = Vector3.one * 0.3f;
        
        // Renk
        Renderer rend = projectile.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        mat.SetFloat("_Metallic", 0.8f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.yellow * 2f);
        rend.sharedMaterial = mat;
        
        // Collider'ı trigger yap
        SphereCollider col = projectile.GetComponent<SphereCollider>();
        col.isTrigger = true;
        
        // Rigidbody ekle (kinematic)
        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Projectile script
        projectile.AddComponent<Projectile>();
        
        return projectile;
    }

    /// <summary>
    /// Seri atış yapar - yayılmalı, her mermi için ayrı ateşleme efekti
    /// </summary>
    private System.Collections.IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            // Yayılma açısı hesapla
            float spreadOffset = ((float)i / (burstCount - 1) - 0.5f) * 2f * burstSpreadAngle;
            Vector3 direction = Quaternion.Euler(0, spreadOffset, 0) * transform.forward;
            
            // Önce ateşleme efekti göster
            ShowMuzzleFlash();
            
            // Efekt bittikten sonra mermi at
            yield return new WaitForSeconds(muzzleFlashDuration);
            
            ShootProjectile(direction);
            
            // Bir sonraki atış için bekle (son atışta bekleme)
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Attack point'i göster
        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1.5f;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPos, 0.2f);
        
        // Mermi yönünü göster
        Gizmos.DrawLine(attackPos, attackPos + transform.forward * 3f);
        
        // Burst yayılma açısını göster
        Gizmos.color = Color.cyan;
        Vector3 leftDir = Quaternion.Euler(0, -burstSpreadAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, burstSpreadAngle, 0) * transform.forward;
        Gizmos.DrawLine(attackPos, attackPos + leftDir * 3f);
        Gizmos.DrawLine(attackPos, attackPos + rightDir * 3f);
    }
}
