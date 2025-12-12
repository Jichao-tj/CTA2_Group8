using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform target;              // player
    public Transform firePoint;           // muzzle / firing origin

    [Header("Ranges")]
    public float visionRange = 15f;
    public float attackRange = 5f;

    [Header("Attack")]
    public float fireCooldown = 1.2f;     // seconds between shots
    public int damage = 10;               // damage dealt per shot

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolPointTolerance = 0.5f;

    [Header("Muzzle & Sound")]
    public ParticleSystem muzzleFlashPrefab; // optional
    public AudioClip fireClip;               // optional
    [Range(0f, 1f)] public float fireVolume = 0.8f;

    [Header("Raycast (instant hit)")]
    public float rayRange = 50f;           // how far the shot reaches
    public LayerMask hitMask = ~0;         // what layers the shot can hit (default: everything)
    public ParticleSystem hitImpactPrefab; // optional impact VFX

    private float fireTimer = 0f;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private int patrolIndex = 0;

    private enum State { Patrolling, Chasing, Attacking }
    private State currentState = State.Patrolling;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        audioSource.volume = fireVolume;
    }

    void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = State.Patrolling;
            SetNextPatrolDestination();
        }
        else
        {
            currentState = State.Chasing;
        }
    }

    void Update()
    {
        if (target == null)
        {
            if (currentState != State.Patrolling)
                currentState = State.Patrolling;
        }
        else
        {
            float distToPlayer = Vector3.Distance(transform.position, target.position);

            if (distToPlayer > visionRange)
            {
                currentState = State.Patrolling;
            }
            else
            {
                // Only set Attacking if within attackRange AND has line of sight.
                // If LOS is blocked, treat as Chasing (or Patrolling depending on your design).
                bool hasLOS = HasLineOfSight();
                if (!hasLOS)
                {
                    // Player is in range but behind cover -> don't attack
                    currentState = State.Chasing;
                }
                else
                {
                    currentState = (distToPlayer > attackRange) ? State.Chasing : State.Attacking;
                }
            }
        }

        switch (currentState)
        {
            case State.Patrolling: HandlePatrol(); break;
            case State.Chasing: HandleChase(); break;
            case State.Attacking: HandleAttack(); break;
        }
    }

    // Patrol
    void HandlePatrol()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) return;

        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance <= patrolPointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            SetNextPatrolDestination();
        }
    }

    void SetNextPatrolDestination()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    // Chase
    void HandleChase()
    {
        if (agent == null || target == null) return;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    // Attack
    void HandleAttack()
    {
        if (agent != null) agent.isStopped = true;
        if (target == null) return;

        // face player horizontally
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

        // Confirm LOS before firing (in case something moved in between)
        if (!HasLineOfSight())
        {
            // lose sight -> switch to chase
            currentState = State.Chasing;
            return;
        }

        // fire cooldown
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireRay();
            fireTimer = fireCooldown;
        }
    }

    // ------------ Raycast-based fire (instant hit) ------------
    void FireRay()
    {
        // 1) muzzle flash + sound
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            ParticleSystem flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            var main = flash.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            flash.Play();
            Destroy(flash.gameObject, 2f);
        }

        if (fireClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireClip, fireVolume);
        }

        // 2) raycast from muzzle
        if (firePoint == null) return;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            // optional: spawn impact VFX
            if (hitImpactPrefab != null)
            {
                // orient the impact to surface normal
                var impact = Instantiate(hitImpactPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                var main = impact.main;
                main.stopAction = ParticleSystemStopAction.Destroy;
                impact.Play();
                Destroy(impact.gameObject, 2f);
            }

            // handle damage: Player
            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
            }
            else
            {
                // if the hit object has some damageable component, you can call it here:
                var dmg = hitInfo.collider.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(damage);
                }
            }
        }

        // debug draw (optional, visible in Scene view)
        Debug.DrawRay(firePoint.position, firePoint.forward * rayRange, Color.red, 0.2f);
    }

    // --------- Line-of-sight (LOS) check ----------
    // Returns true if a ray from firePoint (or from enemy if firePoint missing) directly hits the player without obstruction
    bool HasLineOfSight()
    {
        if (target == null) return false;

        Vector3 origin = (firePoint != null) ? firePoint.position : transform.position;
        Vector3 dir = (target.position - origin).normalized;
        float distance = Vector3.Distance(origin, target.position);

        // Use visionRange as max distance for LOS check
        float maxDist = Mathf.Min(distance, visionRange);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, hitMask, QueryTriggerInteraction.Ignore))
        {
            // If the first thing hit is the player, we have LOS
            if (hit.collider.CompareTag("Player"))
                return true;
            else
                return false;
        }

        // Nothing hit — treat as no LOS
        return false;
    }
}

// Optional interface you can add to other objects to be damageable:
public interface IDamageable
{
    void TakeDamage(int amount);
}