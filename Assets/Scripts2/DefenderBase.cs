using UnityEngine;

public abstract class DefenderBase : MonoBehaviour
{
    public float attackRange = 5f;
    public float attackRate = 1f;
    public int attackDamage = 10;

    protected float attackTimer;
    protected Transform targetEnemy;

    protected virtual void Update()
    {
        attackTimer -= Time.deltaTime;

        if (targetEnemy == null)
            FindTarget();

        if (targetEnemy != null && attackTimer <= 0f)
        {
            Attack();
            attackTimer = 1f / attackRate;
        }
    }

    protected void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float closest = Mathf.Infinity;
        Transform nearest = null;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < closest && dist <= attackRange)
            {
                closest = dist;
                nearest = e.transform;
            }
        }

        targetEnemy = nearest;
    }

    protected abstract void Attack();
}
