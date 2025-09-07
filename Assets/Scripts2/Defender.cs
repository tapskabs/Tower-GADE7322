using UnityEngine;

public class Defender : MonoBehaviour
{
    public int maxHealth = 50;
    public int attackDamage = 5;
    public float attackRate = 1.2f;
    public float attackRange = 4f;

    private int currentHealth;
    private float attackTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackRate)
        {
            Enemy target = FindEnemyInRange();
            if (target != null)
            {
                target.ReceiveDamage(attackDamage);
                attackTimer = 0f;
            }
        }
    }

    Enemy FindEnemyInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var h in hits)
        {
            Enemy e = h.GetComponent<Enemy>();
            if (e != null) return e;
        }
        return null;
    }

    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
