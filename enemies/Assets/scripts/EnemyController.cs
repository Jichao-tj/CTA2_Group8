using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform target;              // 玩家
    public Transform firePoint;           // 发射点
    public GameObject bulletPrefab;       // 子弹预制体

    [Header("Ranges")]
    public float visionRange = 15f;       // 发现玩家的距离
    public float attackRange = 5f;        // 停下来攻击的距离

    [Header("Attack")]
    public float fireCooldown = 1.2f;     // 射击间隔

    [Header("Patrol")]
    public Transform[] patrolPoints;      // 巡逻点数组
    public float patrolPointTolerance = 0.5f; // 接近巡逻点多少算“到达”

    private float fireTimer = 0f;
    private NavMeshAgent agent;

    private int patrolIndex = 0;

    private enum State
    {
        Patrolling,
        Chasing,
        Attacking
    }

    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 如果有巡逻点，从第一个开始
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = State.Patrolling;
            SetNextPatrolDestination();
        }
        else
        {
            currentState = State.Chasing; // 没巡逻点就直接盯着玩家
        }
    }

    void Update()
    {
        if (target == null)
        {
            // 没有玩家时就只巡逻
            if (currentState != State.Patrolling)
                currentState = State.Patrolling;
        }
        else
        {
            float distToPlayer = Vector3.Distance(transform.position, target.position);

            // 玩家超出视野 → 回到巡逻
            if (distToPlayer > visionRange)
            {
                currentState = State.Patrolling;
            }
            else
            {
                // 在视野内，根据距离切换追击/攻击
                if (distToPlayer > attackRange)
                    currentState = State.Chasing;
                else
                    currentState = State.Attacking;
            }
        }

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrol();
                break;
            case State.Chasing:
                HandleChase();
                break;
            case State.Attacking:
                HandleAttack();
                break;
        }
    }

    // ---------------- 巡逻逻辑 ----------------
    void HandlePatrol()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        agent.isStopped = false;

        // 还在路上
        if (!agent.pathPending && agent.remainingDistance <= patrolPointTolerance)
        {
            // 到达当前点 → 切下一个
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            SetNextPatrolDestination();
        }
    }

    void SetNextPatrolDestination()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    // ---------------- 追击逻辑 ----------------
    void HandleChase()
    {
        if (agent == null || target == null) return;

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    // ---------------- 攻击逻辑 ----------------
    void HandleAttack()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (target == null) return;

        // 朝向玩家（只转水平）
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // 射击冷却
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireCooldown;
        }
    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}