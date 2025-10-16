using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Tank Enemy Settings")]
    public float healthMultiplier = 2.5f;
    public float speedMultiplier = 0.6f;
    public float statusResistance = 0.5f; // 50% reduced slow/poison effect

    [Header("UI")]
    public GameObject healthBarPrefab;   // Assign same prefab used by normal enemies

    protected override void Start()
    {
        base.Start();

        // Adjust stats for Tank Enemy
        baseSpeed *= speedMultiplier;
        maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        currentHealth = maxHealth;

        // Spawn health bar
        if (healthBarPrefab != null && healthBar == null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false);
            healthBar = hb.GetComponent<EnemyHealthBar>();
            healthBar?.SetHealth(currentHealth, maxHealth);
        }
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
