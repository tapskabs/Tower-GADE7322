using UnityEngine;

public class DefenderUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 2;

    public int[] upgradeCosts = { 50, 100 };
    public float[] healthMultipliers = { 1.2f, 1.5f };
    public float[] attackRateMultipliers = { 0.9f, 0.75f };
    public float[] damageMultipliers = { 1.3f, 1.6f };

    [Header("Visuals")]
    public Material[] upgradeMaterials;
    private Renderer rend;

    // These references will handle either system
    private DefenderBase defenderBase;
    private Defender legacyDefender;
    private GameManager gameManager;

    void Start()
    {
        defenderBase = GetComponent<DefenderBase>();
        legacyDefender = GetComponent<Defender>();
        rend = GetComponentInChildren<Renderer>();
        gameManager = GameManager.Instance;

        if (defenderBase == null && legacyDefender == null)
            Debug.LogError($"No DefenderBase or Defender component found on {gameObject.name}");
    }

    public void Upgrade()
    {
        if (upgradeLevel >= maxUpgradeLevel)
        {
            Debug.Log("Already at maximum upgrade level.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Upgrade failed: GameManager instance missing.");
            return;
        }

        int cost = upgradeCosts[upgradeLevel];
        if (gameManager.CurrentResources < cost)
        {
            Debug.Log("Not enough resources to upgrade.");
            return;
        }

        // Spend resources first
        gameManager.SpendResources(cost);
        upgradeLevel++;

        // Handle DefenderBase version
        if (defenderBase != null)
        {
            defenderBase.maxHealth = Mathf.RoundToInt(defenderBase.maxHealth * healthMultipliers[upgradeLevel - 1]);
            defenderBase.attackDamage = Mathf.RoundToInt(defenderBase.attackDamage * damageMultipliers[upgradeLevel - 1]);
            defenderBase.attackRate *= attackRateMultipliers[upgradeLevel - 1];
            defenderBase.currentHealth = defenderBase.maxHealth;
        }

        // Handle Legacy Defender version
        else if (legacyDefender != null)
        {
            legacyDefender.maxHealth = Mathf.RoundToInt(legacyDefender.maxHealth * healthMultipliers[upgradeLevel - 1]);
            legacyDefender.damage = Mathf.RoundToInt(legacyDefender.damage * damageMultipliers[upgradeLevel - 1]);
            legacyDefender.attackRate *= attackRateMultipliers[upgradeLevel - 1];
        }

        // Change appearance
        if (rend != null && upgradeMaterials.Length >= upgradeLevel)
        {
            rend.material = upgradeMaterials[upgradeLevel - 1];
        }

        Debug.Log($"{gameObject.name} upgraded to level {upgradeLevel}");
    }
}
