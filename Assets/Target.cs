using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50.0f;
    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
