using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Player takes damage: " + damage + ", current HP = " + health);

        if (health <= 0)
        {
            GameManager.Instance.PlayerDied();
            Debug.Log("Player dead!");
        }
    }
}