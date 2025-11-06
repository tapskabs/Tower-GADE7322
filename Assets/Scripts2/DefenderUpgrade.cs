using UnityEngine;

public class DefenderUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 2;

    public int[] upgradeCosts = { 50, 100 }; // Cost per upgrade
    public float[] healthMultipliers = { 1.2f, 1.5f };
    public float[] attackRateMultipliers = { 0.9f, 0.75f };
    public float[] damageMultipliers = { 1.3f, 1.6f };

    [Header("Visuals")]
    public Material[] upgradeMaterials; // Drag materials for each upgrade stage
    private Renderer rend;

    private DefenderBase defenderBase;
    private ResourceManager resourceManager;

    void Start()
    {
        defenderBase = GetComponent<DefenderBase>();
        rend = GetComponentInChildren<Renderer>();
        resourceManager = FindObjectOfType<ResourceManager>();
    }

    public void Upgrade()
    {
        if (upgradeLevel >= maxUpgradeLevel)
        {
            Debug.Log("Already at max upgrade.");
            return;
        }

        int cost = upgradeCosts[upgradeLevel];
        if (resourceManager.SpendResources(cost))
        {
            upgradeLevel++;

            // Apply stat boosts
            defenderBase.maxHealth = Mathf.RoundToInt(defenderBase.maxHealth * healthMultipliers[upgradeLevel - 1]);
            defenderBase.attackDamage = Mathf.RoundToInt(defenderBase.attackDamage * damageMultipliers[upgradeLevel - 1]);
            defenderBase.attackRate *= attackRateMultipliers[upgradeLevel - 1];

            defenderBase.currentHealth = defenderBase.maxHealth;

            // Change appearance
            if (rend != null && upgradeMaterials.Length >= upgradeLevel)
                rend.material = upgradeMaterials[upgradeLevel - 1];

            Debug.Log($"{gameObject.name} upgraded to level {upgradeLevel}");
        }
        else
        {
            Debug.Log("Not enough resources to upgrade!");
        }
    }
}
