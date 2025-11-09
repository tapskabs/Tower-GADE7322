using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Tower))]
public class ProceduralTower : MonoBehaviour
{
    public ProceduralTowerProfile profile;
    [Header("Auto generation options")]
    public bool generateOnAwake = true;
    public int randomSeed = 0;

    private Tower tower;
    private Renderer[] renderers;
    private Vector3 originalScale;
    private float originalAttackRate;
    private int originalMaxHealth;
    private int originalDamage;
    private float originalRange;
    private GameObject activeParticleInstance;

    void Awake()
    {
        tower = GetComponent<Tower>();
        renderers = GetComponentsInChildren<Renderer>();
        originalScale = transform.localScale;

        // Cache base stats of Tower as current "base" (these are the values upgraded by TowerUpgrade later)
        originalAttackRate = tower.attackRate;
        originalMaxHealth = tower.maxHealth;
        originalDamage = tower.damage;
        originalRange = tower.attackRange;

        if (generateOnAwake)
            GenerateRandomProfile();
    }

    public void GenerateRandomProfile()
    {
        if (randomSeed == 0) randomSeed = Random.Range(1, 999999);
        Random.InitState(randomSeed);

        profile = new ProceduralTowerProfile();
        profile.seedName = $"Seed-{randomSeed}";
        profile.tint = Color.HSVToRGB(Random.value, Mathf.Lerp(0.4f, 1f, Random.value), Mathf.Lerp(0.6f, 1f, Random.value));
        profile.scaleMultiplier = Random.Range(0.85f, 1.25f);

        // Balanced multipliers so game remains fair
        profile.damageMultiplier = Random.Range(0.9f, 1.25f);
        profile.attackRateMultiplier = Random.Range(0.85f, 1.15f); // <1 faster
        profile.rangeMultiplier = Random.Range(0.9f, 1.2f);
        profile.healthMultiplier = Random.Range(0.9f, 1.3f);

        // occasional special effects
        if (Random.value < 0.25f) profile.slowChance = Random.Range(0.05f, 0.25f);
        if (Random.value < 0.2f) profile.poisonChance = Random.Range(0.05f, 0.25f);

        ApplyProfileVisuals();
        ApplyProfileStats();
    }

    public void ApplyProfileVisuals()
    {
        // tint materials (safe: iterate renderers)
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r == null || r.material == null) continue;
                if (r.material.HasProperty("_Color"))
                {
                    r.material.color = profile.tint;
                }
                else if (r.material.HasProperty("_BaseColor"))
                {
                    r.material.SetColor("_BaseColor", profile.tint);
                }
            }
        }

        // scale
        transform.localScale = originalScale * profile.scaleMultiplier;

        // spawn particle effect if provided
        if (activeParticleInstance != null) Destroy(activeParticleInstance);
        if (profile.particlePrefab != null)
        {
            activeParticleInstance = Instantiate(profile.particlePrefab, transform);
            activeParticleInstance.transform.localPosition = Vector3.zero;
        }
    }

    public void ApplyProfileStats()
    {
        // Apply to the tower's base stats (these are the values upgrades will modify further)
        tower.maxHealth = Mathf.Max(1, Mathf.RoundToInt(originalMaxHealth * profile.healthMultiplier));
        tower.damage = Mathf.Max(1, Mathf.RoundToInt(originalDamage * profile.damageMultiplier));
        tower.attackRate = Mathf.Max(0.05f, originalAttackRate * profile.attackRateMultiplier);
        tower.attackRange = Mathf.Max(0.1f, originalRange * profile.rangeMultiplier);

        // Restore full health to reflect new max
        tower.RestoreFullHealth();
    }

    /// <summary>
    /// Fuse this tower with the given neighbor profiles to create a new profile.
    /// This does not destroy neighbors; instead it creates a merged set of traits for this tower.
    /// </summary>
    public void FuseWithNeighbors(ProceduralTower[] neighbors)
    {
        if (profile == null) GenerateRandomProfile();

        // produce a combined profile
        ProceduralTowerProfile result = new ProceduralTowerProfile();
        // keep name
        result.seedName = profile.seedName + "-F";

        // start from current profile values
        result.tint = profile.tint;
        result.scaleMultiplier = profile.scaleMultiplier;
        result.damageMultiplier = profile.damageMultiplier;
        result.attackRateMultiplier = profile.attackRateMultiplier;
        result.rangeMultiplier = profile.rangeMultiplier;
        result.healthMultiplier = profile.healthMultiplier;
        result.slowChance = profile.slowChance;
        result.poisonChance = profile.poisonChance;

        // combine neighbors, averaging with some random variance
        foreach (var n in neighbors)
        {
            if (n == null || n.profile == null) continue;
            var p = n.profile;

            // color blend
            result.tint = Color.Lerp(result.tint, p.tint, 0.5f);

            // average multipliers (weighted average)
            result.scaleMultiplier = (result.scaleMultiplier + p.scaleMultiplier) * 0.5f;
            result.damageMultiplier = (result.damageMultiplier + p.damageMultiplier) * 0.5f;
            result.attackRateMultiplier = (result.attackRateMultiplier + p.attackRateMultiplier) * 0.5f;
            result.rangeMultiplier = (result.rangeMultiplier + p.rangeMultiplier) * 0.5f;
            result.healthMultiplier = (result.healthMultiplier + p.healthMultiplier) * 0.5f;

            result.slowChance = Mathf.Max(result.slowChance, p.slowChance);
            result.poisonChance = Mathf.Max(result.poisonChance, p.poisonChance);
        }

        // add slight random mutation
        result.damageMultiplier *= Random.Range(0.95f, 1.12f);
        result.attackRateMultiplier *= Random.Range(0.95f, 1.08f);
        result.rangeMultiplier *= Random.Range(0.97f, 1.05f);
        result.scaleMultiplier *= Random.Range(0.98f, 1.06f);

        // pick a particle from a neighbor if none set
        foreach (var n in neighbors)
        {
            if (n != null && n.profile != null && n.profile.particlePrefab != null)
            {
                result.particlePrefab = n.profile.particlePrefab;
                break;
            }
        }

        // assign and apply
        profile = result;
        ApplyProfileVisuals();
        ApplyProfileStats();
    }

    /// <summary>
    /// Called when the tower receives a conventional upgrade (so we recalc to keep multipliers correct)
    /// </summary>
    public void OnBaseStatsChanged()
    {
        // Update the cached originals in case TowerUpgrade changed them (make sure we interpret TowerUpgrade as modifying tower original bases)
        // We intentionally keep 'originalX' values as the pre-profile base so upgrades remain additive. Do not overwrite originals here.
        ApplyProfileStats();
    }

    // Helpers for external queries
    public ProceduralTowerProfile GetProfile()
    {
        return profile;
    }
}
