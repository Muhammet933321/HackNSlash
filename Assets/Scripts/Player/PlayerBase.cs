using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tüm karakterler için temel sınıf - Hareket ve ortak özellikler
/// Rigidbody tabanlı fizik hareket sistemi
/// </summary>
public abstract class PlayerBase : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] protected float moveSpeed = 10f;
    [SerializeField] protected float acceleration = 50f;
    [SerializeField] protected float deceleration = 40f;
    [SerializeField] protected float rotationSpeed = 15f;

    [Header("Saldırı Ayarları")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float attackCooldown = 0.5f;
    [SerializeField] protected float secondaryAttackCooldown = 2f; // Özel saldırı cooldown

    [Header("Referanslar")]
    [SerializeField] protected Transform attackPoint;

    // Mevcut durum
    protected float currentDamage;
    protected float lastAttackTime;
    protected float lastSecondaryAttackTime;
    protected bool canAttack = true;

    // Hareket değişkenleri
    protected Vector3 moveInput;
    protected Vector3 currentVelocity;
    protected bool isMoving;

    // Bileşenler
    protected Rigidbody rb;
    protected Camera mainCamera;

    // Propertyler
    public float CurrentDamage => currentDamage;
    public string CharacterName => GetCharacterName();
    public bool IsMoving => isMoving;
    public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        currentDamage = baseDamage;
        
        SetupRigidbody();
    }

    /// <summary>
    /// Rigidbody ayarlarını yapılandırır
    /// </summary>
    protected virtual void SetupRigidbody()
    {
        if (rb == null) return;
        
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
    }

    protected virtual void Start()
    {
        // Alt sınıflar override edebilir
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        ReadInput();
        HandleRotation();
        HandleAttackInput();
        UpdateCooldownUI();
    }

    protected virtual void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        HandleMovement();
    }

    /// <summary>
    /// Input okuma (Update'de çağrılır)
    /// </summary>
    protected virtual void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector3.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;

        moveInput = new Vector3(horizontal, 0f, vertical).normalized;
        isMoving = moveInput.magnitude > 0.1f;
    }

    /// <summary>
    /// Rigidbody tabanlı hareket (FixedUpdate'de çağrılır)
    /// </summary>
    protected virtual void HandleMovement()
    {
        if (rb == null) return;

        Vector3 targetVelocity = moveInput * moveSpeed;
        
        // Mevcut yatay hızı al
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // Hızlanma veya yavaşlama
        float accel = isMoving ? acceleration : deceleration;
        
        // Smooth velocity değişimi
        currentVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetVelocity, accel * Time.fixedDeltaTime);
        
        // Y eksenindeki hızı koru (yerçekimi için)
        currentVelocity.y = rb.linearVelocity.y;
        
        // Rigidbody'ye uygula
        rb.linearVelocity = currentVelocity;
    }

    /// <summary>
    /// Fare yönüne dönüş
    /// </summary>
    protected virtual void HandleRotation()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || mainCamera == null) return;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.magnitude > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Saldırı girdisini kontrol eder
    /// </summary>
    protected virtual void HandleAttackInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Sol tık - birincil saldırı
        if (mouse.leftButton.isPressed && CanPerformAttack())
        {
            PerformPrimaryAttack();
            lastAttackTime = Time.time;
        }

        // Sağ tık - ikincil saldırı (varsa)
        if (mouse.rightButton.wasPressedThisFrame && CanPerformSecondaryAttack())
        {
            PerformSecondaryAttack();
            lastSecondaryAttackTime = Time.time;
        }
    }

    /// <summary>
    /// Saldırı yapılabilir mi kontrol eder
    /// </summary>
    protected virtual bool CanPerformAttack()
    {
        return canAttack && Time.time >= lastAttackTime + attackCooldown;
    }

    /// <summary>
    /// Özel saldırı yapılabilir mi kontrol eder
    /// </summary>
    protected virtual bool CanPerformSecondaryAttack()
    {
        return canAttack && Time.time >= lastSecondaryAttackTime + secondaryAttackCooldown;
    }

    /// <summary>
    /// Cooldown UI'ını günceller
    /// </summary>
    protected virtual void UpdateCooldownUI()
    {
        if (UIManager.Instance == null) return;

        // Birincil saldırı cooldown
        float primaryElapsed = Time.time - lastAttackTime;
        float primaryRemaining = Mathf.Max(0, attackCooldown - primaryElapsed);
        float primaryFill = primaryRemaining / attackCooldown;
        UIManager.Instance.UpdatePrimaryCooldown(primaryFill, primaryRemaining);

        // İkincil saldırı cooldown
        float secondaryElapsed = Time.time - lastSecondaryAttackTime;
        float secondaryRemaining = Mathf.Max(0, secondaryAttackCooldown - secondaryElapsed);
        float secondaryFill = secondaryRemaining / secondaryAttackCooldown;
        UIManager.Instance.UpdateSecondaryCooldown(secondaryFill, secondaryRemaining);
    }

    /// <summary>
    /// Birincil saldırı - Alt sınıflar implement eder
    /// </summary>
    protected abstract void PerformPrimaryAttack();

    /// <summary>
    /// İkincil saldırı - Alt sınıflar override edebilir
    /// </summary>
    protected virtual void PerformSecondaryAttack()
    {
        // Varsayılan olarak bir şey yapmaz
    }

    /// <summary>
    /// Karakter adını döndürür
    /// </summary>
    protected abstract string GetCharacterName();

    /// <summary>
    /// Saldırı gücünü artırır (PowerUp için)
    /// </summary>
    public void IncreaseDamage(float amount)
    {
        currentDamage += amount;
        Debug.Log($"Saldırı gücü arttı! Yeni güç: {currentDamage}");
    }

    /// <summary>
    /// Saldırı gücünü yüzde olarak artırır
    /// </summary>
    public void IncreaseDamagePercent(float percent)
    {
        currentDamage *= (1f + percent / 100f);
        Debug.Log($"Saldırı gücü %{percent} arttı! Yeni güç: {currentDamage}");
    }

    /// <summary>
    /// Hasarı sıfırlar
    /// </summary>
    public void ResetDamage()
    {
        currentDamage = baseDamage;
    }

    private void OnDrawGizmosSelected()
    {
        // Attack point'i görselleştir
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 0.3f);
        }
    }
}
