using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 防止飞太久不消失
    }

    void Update()
    {
        // 直线飞行
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 如果碰到玩家
        if (other.CompareTag("Player"))
        {
            // 玩家要有一个 PlayerHealth 脚本（你暂时用 Dummy）
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }

        // 如果碰到其它物体（墙/地面）
        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            Debug.Log(other.tag);
        }
    }
}
