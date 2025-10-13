using UnityEngine;

public class SlowDefender : DefenderBase
{
    public float slowDuration = 2f;
    public float slowFactor = 0.5f; // enemy speed * 0.5

    protected override void Attack()
    {
        if (targetEnemy == null) return;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ReceiveDamage(attackDamage);
            enemy.ApplySlow(slowFactor, slowDuration);
        }
    }
}
