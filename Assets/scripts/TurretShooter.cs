using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    [Header("References")]
    public Transform target;              // Player
    public Transform gunHead;             // Rotating head
    public Transform firePoint;           // Raycast start point

    [Header("VFX & SFX")]
    public ParticleSystem muzzleFlash;    // Muzzle flash prefab
    public AudioClip fireClip;            // Shooting sound
    public AudioSource audioSource;       // Audio source for gunfire

    [Header("Detection")]
    public float visionRange = 20f;       // Detection range
    public float maxViewAngle = 120f;     // View angle (0~360)
    public bool debugLOS = false;         // draw LOS debug lines in scene view

    [Header("Attack")]
    public float fireCooldown = 1.0f;     // Fire interval
    public float rotateSpeed = 5f;        // Smooth rotation speed
    public float rayRange = 50f;          // Raycast distance
    public LayerMask hitMask;             // Layers the ray can hit (ensure Player & Shield layers included)
    public int damage = 10;               // Damage to player
    public ParticleSystem hitImpactPrefab; // Hit effect on walls, player, etc.

    private float fireTimer = 0f;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (target == null || gunHead == null || firePoint == null)
            return;

        Vector3 toTarget = target.position - gunHead.position;
        float dist = toTarget.magnitude;

        // Too far → ignore
        if (dist > visionRange)
            return;

        // Flat direction (ignore height)
        Vector3 toTargetFlat = new Vector3(toTarget.x, 0f, toTarget.z);
        if (toTargetFlat.sqrMagnitude < 0.0001f)
            return;

        // Check view angle
        float angle = Vector3.Angle(gunHead.forward, toTargetFlat.normalized);
        if (angle > maxViewAngle * 0.5f)
            return;

        // NEW: LOS check that accepts Player OR Shield as first hit
        if (!HasLineOfSightToPlayerOrShield())
            return;

        // Rotate smoothly toward player
        Quaternion targetRot = Quaternion.LookRotation(toTargetFlat.normalized);
        gunHead.rotation = Quaternion.Slerp(gunHead.rotation, targetRot, rotateSpeed * Time.deltaTime);

        // Fire cooldown
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireRaycast();
            fireTimer = fireCooldown;
        }
    }

    // LOS that accepts Player OR Shield as valid first hit
    bool HasLineOfSightToPlayerOrShield()
    {
        if (target == null || firePoint == null) return false;

        Vector3 origin = firePoint.position;
        Vector3 dirVec = target.position - origin;
        float distanceToTarget = dirVec.magnitude;
        if (distanceToTarget < 0.001f) return false;
        Vector3 dir = dirVec / distanceToTarget;

        float maxDist = Mathf.Min(distanceToTarget, visionRange);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLOS)
                Debug.DrawLine(origin, hit.point, Color.yellow, 0.05f);

            // Accept if the first thing hit is Player or Shield
            return hit.collider.CompareTag("Player") || hit.collider.CompareTag("Shield");
        }
        else
        {
            if (debugLOS)
                Debug.DrawLine(origin, origin + dir * maxDist, Color.gray, 0.05f);
            return false;
        }
    }

    void FireRaycast()
    {
        // Play muzzle flash (attach to gunHead so it follows rotation)
        if (muzzleFlash != null && gunHead != null)
        {
            ParticleSystem flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation, gunHead);
            flash.Play();
            Destroy(flash.gameObject, 1f);
        }

        // Play fire sound
        if (audioSource != null && fireClip != null)
        {
            audioSource.PlayOneShot(fireClip);
        }

        // Raycast to simulate hit-scan bullet
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Spawn hit effect (for shield/player/wall)
            if (hitImpactPrefab != null)
            {
                var impact = Instantiate(hitImpactPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                impact.Play();
                Destroy(impact.gameObject, 2f);
            }

            // If hit Shield: call shield handler if available; DO NOT directly damage Player
            if (hitInfo.collider.CompareTag("Shield"))
            {
                var shieldComponent = hitInfo.collider.GetComponentInParent<MonoBehaviour>(); // try find script on parent
                if (shieldComponent != null)
                {
                    // Try to call OnShieldHit if present (safe reflection)
                    var method = shieldComponent.GetType().GetMethod("OnShieldHit");
                    if (method != null)
                    {
                        // OnShieldHit(Vector3 hitPoint, Vector3 hitNormal, Transform attacker)
                        method.Invoke(shieldComponent, new object[] { hitInfo.point, hitInfo.normal, this.transform });
                    }
                }
                // Shield blocks damage; turret still fired (visual + sound)
                return;
            }

            // If hit Player directly: apply damage
            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
                return;
            }

            // else: hit environment/destructible — apply IDamageable if present
            var dmg = hitInfo.collider.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
            }
        }

        // Optional: Debug ray
        if (debugLOS)
            Debug.DrawRay(firePoint.position, firePoint.forward * rayRange, Color.red, 0.2f);
    }
}