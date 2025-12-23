using UnityEngine;

/// <summary>
/// Yakın dövüş karakteri - Kılıç/Balta ile vurur
/// </summary>
public class MeleeCharacter : PlayerBase
{
    [Header("Melee Ayarları")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 90f; // Saldırı açısı
    [SerializeField] private LayerMask enemyLayer;

    [Header("Özel Saldırı")]
    [SerializeField] private float spinDamageMultiplier = 0.5f;
    [SerializeField] private float spinRadius = 3f;
    [SerializeField] private float specialCooldown = 3f;

    [Header("Görsel Efekt")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private Color slashColor = new Color(1f, 0.9f, 0.7f, 1f);
    [SerializeField] private Color spinColor = new Color(1f, 0.3f, 0.1f, 0.8f);

    private float lastSpecialTime;

    protected override void Awake()
    {
        base.Awake();
        // Enemy layer'ı ayarla
        if (enemyLayer == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }
        
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
        return "Savaşçı";
    }

    protected override void PerformPrimaryAttack()
    {
        MeleeAttack();
    }

    protected override void PerformSecondaryAttack()
    {
        // Dönerek saldırı (spin attack)
        if (Time.time >= lastSpecialTime + specialCooldown)
        {
            SpinAttack();
            lastSpecialTime = Time.time;
        }
    }

    /// <summary>
    /// Yakın mesafe saldırısı
    /// </summary>
    private void MeleeAttack()
    {
        Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;
        
        // Menzildeki tüm collider'ları bul (layer bazlı veya tümü)
        Collider[] hitColliders;
        if (enemyLayer != 0)
        {
            hitColliders = Physics.OverlapSphere(attackOrigin, attackRange, enemyLayer);
        }
        else
        {
            // Layer ayarlanmamışsa tüm collider'ları al ve tag ile filtrele
            hitColliders = Physics.OverlapSphere(attackOrigin, attackRange);
        }

        int hitCount = 0;
        foreach (Collider hit in hitColliders)
        {
            // Tag kontrolü
            if (!hit.CompareTag("Enemy")) continue;
            
            // Açı kontrolü - önümüzde mi?
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle <= attackAngle / 2f)
            {
                // Düşmana hasar ver
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(currentDamage);
                    hitCount++;
                    Debug.Log($"Melee hit! Hasar: {currentDamage}");
                }
            }
        }

        // Görsel efekt
        ShowSlashEffect();
    }

    /// <summary>
    /// Dönerek saldırı - 360 derece
    /// </summary>
    private void SpinAttack()
    {
        Vector3 attackOrigin = transform.position;
        
        // Çevredeki tüm collider'ları bul
        Collider[] hitColliders;
        if (enemyLayer != 0)
        {
            hitColliders = Physics.OverlapSphere(attackOrigin, spinRadius, enemyLayer);
        }
        else
        {
            hitColliders = Physics.OverlapSphere(attackOrigin, spinRadius);
        }

        int hitCount = 0;
        foreach (Collider hit in hitColliders)
        {
            if (!hit.CompareTag("Enemy")) continue;
            
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                float spinDamage = currentDamage * spinDamageMultiplier;
                enemy.TakeDamage(spinDamage);
                hitCount++;
            }
        }

        Debug.Log($"Spin Saldırı! {hitCount} düşman vuruldu!");
        
        // Spin efekti göster
        ShowSpinEffect();
    }

    /// <summary>
    /// Saldırı efekti gösterir
    /// </summary>
    private void ShowSlashEffect()
    {
        Vector3 effectPos = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
        
        if (slashEffectPrefab != null)
        {
            // Efekti oluştur
            GameObject effect = Instantiate(slashEffectPrefab, effectPos, transform.rotation);
            
            // Tüm child particle system'leri bul ve başlat
            ParticleSystem[] allParticles = effect.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in allParticles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
            
            // 2 saniye sonra sil
            Destroy(effect, 2f);
        }
        else
        {
            // Varsayılan slash efekti oluştur
            CreateDefaultSlashEffect(effectPos);
        }
    }

    /// <summary>
    /// Spin efekti gösterir - 4 yöne slash efekti oluşturur
    /// </summary>
    private void ShowSpinEffect()
    {
        if (slashEffectPrefab != null)
        {
            // 4 yöne slash efekti oluştur (ileri, sağ, geri, sol)
            Vector3[] directions = new Vector3[]
            {
                transform.forward,   // İleri
                transform.right,     // Sağ
                -transform.forward,  // Geri
                -transform.right     // Sol
            };

            for (int i = 0; i < 4; i++)
            {
                // Pozisyon: Karakterden spinRadius mesafesinde
                Vector3 spawnPos = transform.position + directions[i] * (spinRadius * 0.5f);
                spawnPos.y = transform.position.y + 0.5f; // Biraz yukarıda
                
                // Rotasyon: O yöne baksın
                Quaternion rotation = Quaternion.LookRotation(directions[i], Vector3.up);
                
                // Efekti oluştur
                GameObject effect = Instantiate(slashEffectPrefab, spawnPos, rotation);
                
                // 2 saniye sonra sil
                Destroy(effect, 2f);
            }
        }
        else
        {
            CreateDefaultSpinEffect();
        }
    }

    /// <summary>
    /// Varsayılan slash efekti oluşturur
    /// </summary>
    private void CreateDefaultSlashEffect(Vector3 position)
    {
        // Yarım ay şeklinde slash
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Quad);
        slash.name = "SlashEffect";
        slash.transform.position = position;
        slash.transform.rotation = transform.rotation * Quaternion.Euler(0, 0, -45);
        slash.transform.localScale = new Vector3(2f, 0.3f, 1f);
        
        // Collider kapat
        Destroy(slash.GetComponent<Collider>());
        
        // Material
        Renderer rend = slash.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = slashColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", slashColor * 3f);
        
        // Transparent yap
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        rend.material = mat;
        
        // SlashEffect script ekle
        SlashEffect effect = slash.AddComponent<SlashEffect>();
        effect.Initialize(slashColor, 1f, 0.2f);
    }

    /// <summary>
    /// Varsayılan spin efekti oluşturur
    /// </summary>
    private void CreateDefaultSpinEffect()
    {
        GameObject spinObj = new GameObject("SpinEffect");
        spinObj.transform.position = transform.position;
        
        SpinEffect effect = spinObj.AddComponent<SpinEffect>();
        effect.Initialize(spinRadius, spinColor);
    }

    private void OnDrawGizmosSelected()
    {
        // Saldırı menzilini görselleştir
        Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin, attackRange);

        // Saldırı açısını göster
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * transform.forward * attackRange;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * transform.forward * attackRange;
        Gizmos.DrawLine(attackOrigin, attackOrigin + leftBoundary);
        Gizmos.DrawLine(attackOrigin, attackOrigin + rightBoundary);

        // Spin menzilini göster
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, spinRadius);
    }
}
