using UnityEngine;
using FishNet.Object;
using MoreMountains.TopDownEngine;

public class DashHitKill : NetworkBehaviour
{
    [Header("TDE")]
    [SerializeField] private Health targetHealth;          
    [SerializeField] private string otherPlayerTag = "Player";

    [Header("Kill rules")]
    [SerializeField] private int killDamage = 9999;
    [SerializeField] private float hitCooldownSeconds = 0.25f;

    [Header("VFX")]
    [SerializeField] private GameObject explosionPrefab;

    private bool _isDashing;
    private float _lastHitTime;

    public void SetDashingTrue()  => _isDashing = true;
    public void SetDashingFalse() => _isDashing = false;

    private void OnTriggerEnter(Collider other)
    {
        // Server-authoritative
        if (!IsServerInitialized) return;

        if (!_isDashing) return;
        if (Time.time - _lastHitTime < hitCooldownSeconds) return;
        if (!other.CompareTag(otherPlayerTag)) return;

        Health otherHealth = other.GetComponentInParent<Health>();
        if (otherHealth == null) return;

        // Prevent self-hit
        if (targetHealth != null && otherHealth == targetHealth) return;

        // Kill
        otherHealth.Damage(killDamage, gameObject, 0f, 0f, Vector3.zero);

        _lastHitTime = Time.time;

        // Points
        var score = GetComponent<PlayerScore>();
        if (score != null) score.AddPointServer();

        // VFX on everyone
        Vector3 pos = other.ClosestPoint(transform.position);
        SpawnExplosionObservers(pos);
    }

    [ObserversRpc(BufferLast = false)]
    private void SpawnExplosionObservers(Vector3 position)
    {
        if (explosionPrefab == null) return;

        var go = Instantiate(explosionPrefab, position, Quaternion.identity);
        Destroy(go, 2.5f);
    }
}
