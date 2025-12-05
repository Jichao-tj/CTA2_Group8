using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    [Header("References")]
    public Transform target;          // 玩家
    public Transform gunHead;         // 旋转的“炮头”
    public Transform firePoint;       // 发射点
    public GameObject bulletPrefab;   // 子弹预制体

    [Header("Detection")]
    public float visionRange = 20f;   // 感应距离
    public float maxViewAngle = 120f; // 可选：视角（度），0~360

    [Header("Attack")]
    public float fireCooldown = 1.0f; // 射击间隔
    public float rotateSpeed = 5f;    // 转头速度（插值用）

    private float fireTimer = 0f;

    void Update()
    {
        if (target == null || gunHead == null || firePoint == null || bulletPrefab == null)
            return;

        Vector3 toTarget = target.position - gunHead.position;
        float dist = toTarget.magnitude;

        // 超出距离就不管玩家
        if (dist > visionRange)
            return;

        // 只看水平角度
        Vector3 toTargetFlat = new Vector3(toTarget.x, 0f, toTarget.z);
        if (toTargetFlat.sqrMagnitude < 0.0001f)
            return;

        // 判断是否在视角内（可选）
        float angle = Vector3.Angle(gunHead.forward, toTargetFlat.normalized);
        if (angle > maxViewAngle * 0.5f)
        {
            // 玩家在背后，不射
            return;
        }

        // 平滑朝向玩家
        Quaternion targetRot = Quaternion.LookRotation(toTargetFlat.normalized);
        gunHead.rotation = Quaternion.Slerp(gunHead.rotation, targetRot, rotateSpeed * Time.deltaTime);

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
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}