using UnityEngine;

/// <summary>
/// Oyuncuyu takip eden kamera
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 15, -10);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Sınırlar (Opsiyonel)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-50, -50);
    [SerializeField] private Vector2 maxBounds = new Vector2(50, 50);

    private void Start()
    {
        // Hedef atanmamışsa oyuncuyu bul
        if (target == null)
        {
            FindPlayer();
        }

        // Başlangıç pozisyonu
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }

        // Hedef pozisyon
        Vector3 desiredPosition = target.position + offset;

        // Sınırlandırma
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        // Yumuşak takip
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Hedefe bak
        transform.LookAt(target);
    }

    /// <summary>
    /// Oyuncuyu bulur
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    /// <summary>
    /// Hedefi ayarlar
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
