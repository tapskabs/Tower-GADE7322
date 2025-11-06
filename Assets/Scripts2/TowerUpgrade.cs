using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgrade : MonoBehaviour
{
    [Header("References")]
    public Tower tower;                      // Reference to your tower script
    public Button upgradeButton;             // Button in the UI to trigger upgrade
    public TextMeshProUGUI upgradeText;      // Optional TMP text to show cost/level

    [Header("Upgrade Settings")]
    public int[] upgradeCosts = { 100, 200 };   // Cost for Level 1 and Level 2 upgrades
    public int[] healthIncreases = { 100, 150 };
    public int[] damageIncreases = { 10, 20 };
    public float[] attackRateBoosts = { -0.2f, -0.3f }; // Reduces attack cooldown (faster attacks)
    public Material[] upgradeMaterials;          // Different visuals for each upgrade level

    private int currentUpgradeLevel = 0;
    private Renderer towerRenderer;

    void Start()
    {
        if (tower == null) tower = GetComponent<Tower>();
        towerRenderer = tower.GetComponentInChildren<Renderer>();

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeTower);

        UpdateUpgradeUI();
    }

    void UpgradeTower()
    {
        // Check if we can upgrade
        if (currentUpgradeLevel >= upgradeCosts.Length)
        {
            Debug.Log("Tower is already fully upgraded!");
            return;
        }

        int cost = upgradeCosts[currentUpgradeLevel];
        if (GameManager.Instance.CurrentResources < cost)
        {
            Debug.Log("Not enough resources to upgrade!");
            return;
        }

        // Spend resources
        GameManager.Instance.SpendResources(cost);

        // Apply upgrade
        tower.maxHealth += healthIncreases[currentUpgradeLevel];
        tower.damage += damageIncreases[currentUpgradeLevel];
        tower.attackRate = Mathf.Max(0.1f, tower.attackRate + attackRateBoosts[currentUpgradeLevel]);

        // Update visuals if materials exist
        if (upgradeMaterials != null && currentUpgradeLevel < upgradeMaterials.Length && towerRenderer != null)
        {
            towerRenderer.material = upgradeMaterials[currentUpgradeLevel];
        }

        currentUpgradeLevel++;
        Debug.Log("Tower upgraded to level " + currentUpgradeLevel);

        UpdateUpgradeUI();
    }

    void UpdateUpgradeUI()
    {
        if (upgradeText == null) return;

        if (currentUpgradeLevel >= upgradeCosts.Length)
            upgradeText.text = "Tower Max Level";
        else
            upgradeText.text = $"Upgrade ({upgradeCosts[currentUpgradeLevel]} Resources)";
    }
}
