using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Tank Enemy Settings")]
    public float healthMultiplier = 2.5f;
    public float speedMultiplier = 0.6f;
    public float statusResistance = 0.5f; // 50% reduced slow/poison effect
    private IDamageableDefender currentDefenderTarget;

    protected override void Start()
    {
        base.Start();

        // Adjust stats for Tank Enemy
        baseSpeed *= speedMultiplier;
        maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        currentHealth = maxHealth;

        
    }

    public override void ApplySlow(float slowFactor, float duration)
    {
        // Halve slow strength and duration
        float adjustedFactor = Mathf.Lerp(1f, slowFactor, statusResistance);
        float adjustedDuration = duration * statusResistance;
        base.ApplySlow(adjustedFactor, adjustedDuration);
    }

    public override void ApplyPoison(int tickDamage, float duration, float tickRate)
    {
        // Reduce poison potency
        int reducedDamage = Mathf.RoundToInt(tickDamage * statusResistance);
        float reducedDuration = duration * statusResistance;
        base.ApplyPoison(reducedDamage, reducedDuration, tickRate);
    }
    private void DetectNearbyDefender()
    {
        currentDefenderTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, reachRadius);
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Defender"))
            {
                IDamageableDefender dmgDef = hit.GetComponent<IDamageableDefender>();
                if (dmgDef != null)
                {
                    float dist = Vector3.Distance(transform.position, dmgDef.GetPosition());
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        currentDefenderTarget = dmgDef;
                    }
                }
            }
        }
    }
    private void AttackDefender()
    {
        if (currentDefenderTarget == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            currentDefenderTarget.ReceiveDamage(damage);
            attackTimer = 0f;
        }
    }

    protected override void Update()
    {
        base.Update();

        // Keep health bar aligned with world position
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }
}
