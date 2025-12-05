using UnityEngine;

public class AutoMoveDummyInBounds : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 120f;
    public float changeDirectionInterval = 2f;

    private float timer = 0f;
    private Vector3 moveDirection;

    // 地面范围
    private float minX, maxX, minZ, maxZ;

    void Start()
    {
        // 自动获取地面的范围（必须选中地面并保证它是 Ground）
        GameObject ground = GameObject.Find("ground");

        if (ground != null)
        {
            // 地面必须是一个 Plane 或 Cube，有 MeshRenderer
            MeshRenderer mr = ground.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Bounds b = mr.bounds;
                minX = b.min.x;
                maxX = b.max.x;
                minZ = b.min.z;
                maxZ = b.max.z;
            }
        }

        PickNewDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeDirectionInterval)
        {
            PickNewDirection();
            timer = 0f;
        }

        // 移动
        Vector3 newPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        // 限制在灰色地面范围内
        if (newPos.x < minX || newPos.x > maxX || newPos.z < minZ || newPos.z > maxZ)
        {
            // 如果快出界，直接反向
            PickNewDirection(true);
        }
        else
        {
            transform.position = newPos;

            // 转头朝向移动方向
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }

    // 随机方向 / 或反向
    void PickNewDirection(bool reverse = false)
    {
        if (reverse)
        {
            moveDirection = -moveDirection;
            return;
        }

        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);

        moveDirection = new Vector3(x, 0f, z).normalized;

        if (moveDirection.magnitude < 0.1f)
            moveDirection = Vector3.forward;
    }
}