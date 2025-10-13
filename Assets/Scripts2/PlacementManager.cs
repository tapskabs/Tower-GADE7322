using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlacementManager;

public class PlacementManager : MonoBehaviour
{
    public enum BuildMode { None, Defender, Mine }
    public BuildMode currentMode = BuildMode.None;

    [Header("References")]
    public ProceduralMap map;
    public TextMeshProUGUI resourceText;

    [Header("Prefabs & Costs")]
    public GameObject defenderPrefab;
    public int defenderCost = 50;
    public GameObject minePrefab;
    public int mineCost = 60;

    private readonly List<BuildNode> nodes = new List<BuildNode>();

    void Start()
    {
        CacheNodesFromMap();
        UpdateResourceText();
    }

    void CacheNodesFromMap()
    {
        nodes.Clear();

        if (map == null)
        {
            Debug.LogWarning("PlacementManager: No ProceduralMap assigned.");
            return;
        }

        if (map.defenderNodes != null && map.defenderNodes.Count > 0)
        {
            foreach (var dn in map.defenderNodes)
                if (dn != null) nodes.Add(dn.GetComponent<BuildNode>());
        }

        if (map.miningNodes != null && map.miningNodes.Count > 0)
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
                TryPlaceDefender(node);
                break;
            case BuildMode.Mine:
                TryPlaceMine(node);
                break;
        }
    }

    void TryPlaceDefender(BuildNode node)
    {
        if (GameManager.Instance.CurrentResources >= defenderCost)
        {
            node.PlaceStructure(defenderPrefab);
            GameManager.Instance.SpendResources(defenderCost);
            UpdateResourceText();
            currentMode = BuildMode.None;
        }
        else
        {
            Debug.Log("Not enough resources for defender.");
        }
    }

    void TryPlaceMine(BuildNode node)
    {
        if (GameManager.Instance.CurrentResources >= mineCost)
        {
            node.PlaceStructure(minePrefab);
            GameManager.Instance.SpendResources(mineCost);
            UpdateResourceText();
            currentMode = BuildMode.None;
        }
        else
        {
            Debug.Log("Not enough resources for mine.");
        }
    }

    public void SelectDefenderMode()
    {
        currentMode = BuildMode.Defender;
        Debug.Log("Defender Build Mode Activated");
    }

    public void SelectMineMode()
    {
        currentMode = BuildMode.Mine;
        Debug.Log("Mine Build Mode Activated");
    }

    public void CancelBuildMode()
    {
        currentMode = BuildMode.None;
        Debug.Log("Build Mode Cancelled");
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
}
