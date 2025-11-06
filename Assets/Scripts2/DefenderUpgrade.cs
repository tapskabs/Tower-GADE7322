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

    private DefenderBase defenderBase;
    private GameManager gameManager;

    void Start()
    {
        // Try to get DefenderBase (main inheritance root)
        defenderBase = GetComponent<DefenderBase>();

        //  Safety net: if not found, try parent or derived types
        if (defenderBase == null)
        {
            defenderBase = GetComponentInParent<DefenderBase>();
        }

        rend = GetComponentInChildren<Renderer>();
        gameManager = GameManager.Instance;

        if (defenderBase == null)
            Debug.LogError($" No DefenderBase found on {gameObject.name}");
    }

    public void Upgrade()
    {
        if (defenderBase == null)
        {
            Debug.LogError(" Cannot upgrade — defenderBase reference missing!");
            return;
        }

        if (upgradeLevel >= maxUpgradeLevel)
        {
            Debug.Log("Already at max upgrade.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        int cost = upgradeCosts[upgradeLevel];
        if (gameManager.CurrentResources >= cost)
        {
            gameManager.SpendResources(cost);
            upgradeLevel++;

            //  Apply stat boosts
            defenderBase.maxHealth = Mathf.RoundToInt(defenderBase.maxHealth * healthMultipliers[upgradeLevel - 1]);
            defenderBase.attackDamage = Mathf.RoundToInt(defenderBase.attackDamage * damageMultipliers[upgradeLevel - 1]);
            defenderBase.attackRate *= attackRateMultipliers[upgradeLevel - 1];
            defenderBase.currentHealth = defenderBase.maxHealth;

            //  Change material
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
