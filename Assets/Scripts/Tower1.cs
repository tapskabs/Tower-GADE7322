using UnityEngine;

public class Tower1 : MonoBehaviour
{
    public int health = 100;

    void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Debug.Log("Game Over! Tower destroyed.");
            // Reload scene or show game over UI
        }
    }
}
