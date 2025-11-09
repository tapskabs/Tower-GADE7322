using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Tower))]
public class ProceduralTower : MonoBehaviour
{
    public ProceduralTowerProfile profile;
    [Header("Auto generation")]
    public bool generateOnAwake = true;
    public int randomSeed = 0;

    // spawn timestamp (used to decide which tower was spawned later)
    [HideInInspector] public float spawnTime = 0f;

    // references & cached originals
    private Tower tower;
    private Renderer[] renderers;
    private Vector3 originalScale;
    private float originalAttackRate;
    private int originalMaxHealth;
    private int originalDamage;
    private float originalRange;
    private GameObject activeParticleInstance;

    // glow/emission control
    private Color baseTint = Color.white;
    private float glowIntensity = 0f;
    private static readonly string EMISSION_PROP = "_EmissionColor";
    private Coroutine glowCoroutine;

    void Awake()
    {
        tower = GetComponent<Tower>();
        renderers = GetComponentsInChildren<Renderer>();
        originalScale = transform.localScale;
        originalAttackRate = tower.attackRate;
        originalMaxHealth = tower.maxHealth;
        originalDamage = tower.damage;
        originalRange = tower.attackRange;

        if (generateOnAwake)
            GenerateRandomProfile();

        // initial tint capture
        if (renderers != null && renderers.Length > 0 && renderers[0].material != null)
        {
            if (renderers[0].material.HasProperty("_Color"))
                baseTint = renderers[0].material.color;
        }
    }

    public void GenerateRandomProfile()
    {
        if (randomSeed == 0) randomSeed = Random.Range(1, 999999);
        Random.InitState(randomSeed);

        profile = new ProceduralTowerProfile();
        profile.seedName = $"Seed-{randomSeed}";
        profile.tint = Color.HSVToRGB(Random.value, Mathf.Lerp(0.4f, 1f, Random.value), Mathf.Lerp(0.6f, 1f, Random.value));
        profile.scaleMultiplier = Random.Range(0.85f, 1.25f);

        profile.damageMultiplier = Random.Range(0.95f, 1.18f);
        profile.attackRateMultiplier = Random.Range(0.9f, 1.07f); // <1 slightly faster
        profile.rangeMultiplier = Random.Range(0.95f, 1.15f);
        profile.healthMultiplier = Random.Range(0.95f, 1.2f);

        if (Random.value < 0.25f) profile.slowChance = Random.Range(0.05f, 0.25f);
        if (Random.value < 0.2f) profile.poisonChance = Random.Range(0.05f, 0.25f);

        ApplyProfileVisuals();
        ApplyProfileStats();
    }

    public void ApplyProfileVisuals()
    {
        if (profile == null) return;

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

        transform.localScale = originalScale * profile.scaleMultiplier;

        if (activeParticleInstance != null) Destroy(activeParticleInstance);
        if (profile.particlePrefab != null)
        {
            activeParticleInstance = Instantiate(profile.particlePrefab, transform);
            activeParticleInstance.transform.localPosition = Vector3.zero;
        }
    }

    public void ApplyProfileStats()
    {
        if (profile == null || tower == null) return;

        tower.maxHealth = Mathf.Max(1, Mathf.RoundToInt(originalMaxHealth * profile.healthMultiplier));
        tower.damage = Mathf.Max(1, Mathf.RoundToInt(originalDamage * profile.damageMultiplier));
        tower.attackRate = Mathf.Max(0.05f, originalAttackRate * profile.attackRateMultiplier);
        tower.attackRange = Mathf.Max(0.1f, originalRange * profile.rangeMultiplier);

        tower.RestoreFullHealth();
    }

    // Called when TowerUpgrade changed base stats, reapply profile multipliers onto new base values.
    public void OnBaseStatsChanged()
    {
        ApplyProfileStats();
    }

    // Called by our spawner or PlacementManager to set a spawn timestamp (so we know which tower is newer)
    public void SetSpawnTime(float t)
    {
        spawnTime = t;
    }

    // Tell this tower to glow (for nearby towers): intensity 0..1, duration optional
    public void SetProximityGlow(float intensity, float duration = 0.25f)
    {
        glowIntensity = Mathf.Clamp01(intensity);
        ApplyEmission(glowIntensity);

        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        if (duration > 0f)
            glowCoroutine = StartCoroutine(GlowDecay(duration));
    }

    IEnumerator GlowDecay(float duration)
    {
        float start = glowIntensity;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = 1f - (elapsed / duration);
            ApplyEmission(start * t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ApplyEmission(0f);
        glowCoroutine = null;
    }

    void ApplyEmission(float intensity)
    {
        if (renderers == null) return;
        Color emission = profile != null ? profile.tint * Mathf.Lerp(0.05f, 0.6f, intensity) : baseTint * intensity;
        foreach (var r in renderers)
        {
            if (r == null || r.material == null) continue;
            if (r.material.HasProperty(EMISSION_PROP))
            {
                r.material.SetColor(EMISSION_PROP, emission);
                r.material.EnableKeyword("_EMISSION");
            }
        }
    }

    /// <summary>
    /// Destructively fuse 'other' into this (this will keep this tower, kill other).
    /// boostAmount is integer (2 or 3) representing how many stacks of attack-rate boost to apply.
    /// Attack rate uses division (smaller attackRate => faster firing).
    /// </summary>
    public void ApplyDestructiveFusionFrom(ProceduralTower other, int boostAmount, GameObject fusionVFX = null)
    {
        if (other == null || other == this) return;

        // merge profile: simple weighted average biased to this tower
        if (profile == null) profile = new ProceduralTowerProfile();

        profile.tint = Color.Lerp(profile.tint, other.profile != null ? other.profile.tint : profile.tint, 0.4f);
        profile.damageMultiplier = Mathf.Lerp(profile.damageMultiplier, other.profile != null ? other.profile.damageMultiplier : profile.damageMultiplier, 0.35f);
        profile.attackRateMultiplier = Mathf.Lerp(profile.attackRateMultiplier, other.profile != null ? other.profile.attackRateMultiplier : profile.attackRateMultiplier, 0.35f);
        profile.rangeMultiplier = Mathf.Lerp(profile.rangeMultiplier, other.profile != null ? other.profile.rangeMultiplier : profile.rangeMultiplier, 0.35f);
        profile.scaleMultiplier = (profile.scaleMultiplier + (other.profile != null ? other.profile.scaleMultiplier : 1f)) * 0.5f;
        profile.healthMultiplier = Mathf.Lerp(profile.healthMultiplier, other.profile != null ? other.profile.healthMultiplier : profile.healthMultiplier, 0.35f);
        profile.slowChance = Mathf.Max(profile.slowChance, other.profile != null ? other.profile.slowChance : 0f);
        profile.poisonChance = Mathf.Max(profile.poisonChance, other.profile != null ? other.profile.poisonChance : 0f);

        // apply visuals and stats
        ApplyProfileVisuals();
        ApplyProfileStats();

        // Attack-rate boost: divide attackRate by boostAmount (stackable)
        tower.attackRate = Mathf.Max(0.05f, tower.attackRate / Mathf.Max(1, boostAmount));

        // small visual pulse
        SetProximityGlow(1f, 0.6f);

        // optional VFX
        if (fusionVFX != null)
        {
            var fx = Instantiate(fusionVFX, transform.position + Vector3.up * 0.6f, Quaternion.identity);
            Destroy(fx, 2.0f);
        }

        // destroy the other tower gameobject cleanly (and its healthbars / UI if they exist)
        Destroy(other.gameObject);
    }

    public ProceduralTowerProfile GetProfile()
    {
        return profile;
    }
}
