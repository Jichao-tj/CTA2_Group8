using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50.0f;

    void Awake()
    {
        GameManager.Instance.RegisterEnemy();
    }

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
        GameManager.Instance.EnemyDefeated();
        Destroy(gameObject);
    }
}
