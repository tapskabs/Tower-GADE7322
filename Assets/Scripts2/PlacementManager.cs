using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    public enum BuildMode { None, Defender, Mine, SlowDefender, PoisonDefender }
    public BuildMode currentMode = BuildMode.None;

    [Header("References")]
    public ProceduralMap map;
    public TextMeshProUGUI resourceText;

    [Header("Prefabs & Costs")]
    public GameObject defenderPrefab;
    public int defenderCost = 50;
    public GameObject minePrefab;
    public int mineCost = 60;
    public GameObject slowDefenderPrefab;
    public int slowDefenderCost = 80;
    public GameObject poisonDefenderPrefab;
    public int poisonDefenderCost = 100;

    [Header("UI Buttons")]
    public Button defenderButton;
    public Button mineButton;
    public Button slowDefenderButton;
    public Button poisonDefenderButton;
    public Button cancelButton;

    private readonly List<BuildNode> nodes = new List<BuildNode>();

    void Start()
    {
        CacheNodesFromMap();
        UpdateResourceText();
        ResetButtonStates();
    }

    void CacheNodesFromMap()
    {
        nodes.Clear();

        if (map == null)
        {
            Debug.LogWarning("PlacementManager: No ProceduralMap assigned.");
            return;
        }

        if (map.defenderNodes != null)
        {
            foreach (var dn in map.defenderNodes)
                if (dn != null) nodes.Add(dn.GetComponent<BuildNode>());
        }

        if (map.miningNodes != null)
        {
            foreach (var mn in map.miningNodes)
                if (mn != null) nodes.Add(mn.GetComponent<BuildNode>());
        }

        Debug.Log($"Cached {nodes.Count} build nodes from map.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            TryPlaceOnNode();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ResetBuildMode();
        }
    }

    void TryPlaceOnNode()
    {
        if (Camera.main == null || currentMode == BuildMode.None) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

        BuildNode node = hit.collider.GetComponent<BuildNode>();
        if (node == null || node.isOccupied) return;

        switch (currentMode)
        {
            case BuildMode.Defender:
                TryPlaceStructure(node, defenderPrefab, defenderCost);
                break;
            case BuildMode.Mine:
                TryPlaceStructure(node, minePrefab, mineCost);
                break;
            case BuildMode.SlowDefender:
                TryPlaceStructure(node, slowDefenderPrefab, slowDefenderCost);
                break;
            case BuildMode.PoisonDefender:
                TryPlaceStructure(node, poisonDefenderPrefab, poisonDefenderCost);
                break;
        }
    }

    void TryPlaceStructure(BuildNode node, GameObject prefab, int cost)
    {
        if (GameManager.Instance.CurrentResources < cost)
        {
            Debug.Log("Not enough resources.");
            return;
        }

        // Place structure on the node
        GameObject placed = node.PlaceStructure(prefab);

        if (placed == null)
        {
            Debug.LogWarning("Placement failed: prefab not instantiated.");
            return;
        }

        GameManager.Instance.SpendResources(cost);
        UpdateResourceText();
        ResetBuildMode();

        // --- Tower fusion & upgrades ---
        ProceduralTower p = placed.GetComponentInChildren<ProceduralTower>();
        if (p != null)
        {
            // Ensure tower has a profile
            if (p.GetProfile() == null) p.GenerateRandomProfile();

            // Notify fusion manager (does NOT assign void to variable)
            TowerFusionManager.Instance?.OnTowerPlaced(p);

            // Optional: preserve tower upgrades if you have an UpgradeManager
            // UpgradeManager.Instance?.InitializeUpgrades(p);
        }
    }

    // --- UI Button Methods ---
    public void SelectDefenderMode()
    {
        currentMode = BuildMode.Defender;
        Debug.Log("Defender Build Mode Activated");
        UpdateButtonStates();
    }

    public void SelectMineMode()
    {
        currentMode = BuildMode.Mine;
        Debug.Log("Mine Build Mode Activated");
        UpdateButtonStates();
    }

    public void SelectSlowDefenderMode()
    {
        currentMode = BuildMode.SlowDefender;
        Debug.Log("Slow Defender Build Mode Activated");
        UpdateButtonStates();
    }

    public void SelectPoisonDefenderMode()
    {
        currentMode = BuildMode.PoisonDefender;
        Debug.Log("Poison Defender Build Mode Activated");
        UpdateButtonStates();
    }

    public void CancelBuildMode()
    {
        ResetBuildMode();
    }

    void ResetBuildMode()
    {
        currentMode = BuildMode.None;
        UpdateButtonStates();
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void UpdateResourceText()
    {
        if (resourceText == null) return;
        resourceText.text = GameManager.Instance != null
            ? GameManager.Instance.CurrentResources.ToString()
            : "0";
    }

    void UpdateButtonStates()
    {
        if (defenderButton != null)
            defenderButton.interactable = (currentMode != BuildMode.Defender);
        if (mineButton != null)
            mineButton.interactable = (currentMode != BuildMode.Mine);
        if (slowDefenderButton != null)
            slowDefenderButton.interactable = (currentMode != BuildMode.SlowDefender);
        if (poisonDefenderButton != null)
            poisonDefenderButton.interactable = (currentMode != BuildMode.PoisonDefender);
        if (cancelButton != null)
            cancelButton.interactable = (currentMode != BuildMode.None);
    }

    void ResetButtonStates()
    {
        if (defenderButton != null) defenderButton.interactable = true;
        if (mineButton != null) mineButton.interactable = true;
        if (slowDefenderButton != null) slowDefenderButton.interactable = true;
        if (poisonDefenderButton != null) poisonDefenderButton.interactable = true;
        if (cancelButton != null) cancelButton.interactable = false;
    }
}
