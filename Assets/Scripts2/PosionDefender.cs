using UnityEngine;

public class PoisonDefender : DefenderBase, IDamageableDefender
{
    public int poisonDamage = 2;
    public float poisonDuration = 5f;
    public float tickRate = 1f;

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
            enemy.ApplyPoison(poisonDamage, poisonDuration, tickRate);
        }
    }
}
