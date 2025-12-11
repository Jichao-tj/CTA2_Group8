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

    [Header("Attack")]
    public float fireCooldown = 1.0f;     // Fire interval
    public float rotateSpeed = 5f;        // Smooth rotation speed
    public float rayRange = 50f;          // Raycast distance
    public LayerMask hitMask;             // Layers the ray can hit
    public int damage = 10;               // Damage to player
    public ParticleSystem hitImpactPrefab; // Hit effect on walls, player, etc.

    private float fireTimer = 0f;

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

    void FireRaycast()
    {
        // 1. Play muzzle flash at gunHead
        if (muzzleFlash != null)
        {
            ParticleSystem flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation, gunHead);
            flash.Play();
            Destroy(flash.gameObject, 1f);
        }

        // 2. Play fire sound
        if (audioSource != null && fireClip != null)
        {
            audioSource.PlayOneShot(fireClip);
        }

        // 3. Raycast to simulate hit-scan bullet
        Ray ray = new Ray(firePoint.position, firePoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Spawn hit effect
            if (hitImpactPrefab != null)
            {
                var impact = Instantiate(hitImpactPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                impact.Play();
                Destroy(impact.gameObject, 2f);
            }

            // Damage player
            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
            }
        }

        // Optional: Debug ray
        Debug.DrawRay(firePoint.position, firePoint.forward * rayRange, Color.red, 0.2f);
    }
}