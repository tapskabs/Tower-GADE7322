using UnityEngine;

public class PoisonDefender : DefenderBase
{
    public int poisonDamage = 2;
    public float poisonDuration = 5f;
    public float tickRate = 1f;

    protected override void Attack()
    {
        if (targetEnemy == null) return;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ReceiveDamage(attackDamage);
            enemy.ApplyPoison(poisonDamage, poisonDuration, tickRate);
        }
    }
}
