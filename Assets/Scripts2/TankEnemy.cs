using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Tank Enemy Settings")]
    public float healthMultiplier = 2.5f;
    public float speedMultiplier = 0.6f;
    public float statusResistance = 0.5f; // 50% reduced slow/poison effect

    protected override void Start()
    {
        base.Start();
        baseSpeed *= speedMultiplier;
        maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
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
}
