using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;
    public int damage = 10;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        Destroy(gameObject, lifeTime); 
    }

    void Update()
    {
        // ----------------------------
        // 1. Move bullet forward
        // ----------------------------
        Vector3 nextPosition = transform.position + transform.forward * speed * Time.deltaTime;

        // ----------------------------
        // 2. Raycast from last frame → this frame
        //    This prevents tunneling (穿模)
        // ----------------------------
        if (Physics.Raycast(lastPosition, transform.forward, out RaycastHit hit, 
                            Vector3.Distance(lastPosition, nextPosition)))
        {
            // Hit Player
            if (hit.collider.CompareTag("Player"))
            {
                var health = hit.collider.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }

            // Destroy if hit anything except Enemy
            if (!hit.collider.CompareTag("Enemy"))
            {
                Destroy(gameObject);
                return;
            }
        }

        // ----------------------------
        // 3. Apply movement
        // ----------------------------
        transform.position = nextPosition;
        lastPosition = transform.position;
    }
}