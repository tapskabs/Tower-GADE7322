using UnityEngine;

public class SlowDefender : DefenderBase, IDamageableDefender
{
    public float slowDuration = 2f;
    public float slowFactor = 0.5f; // enemy speed * 0.5

    public void ReceiveDamage(int dmg)
    {
        base.ReceiveDamage(dmg);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
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
