using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public ProceduralMap map;
    public TextMeshProUGUI resourceText;

    [Header("Defenders")]
    public GameObject defenderPrefab;
    public int defenderCost = 50;
    private readonly List<DefenderNode> defenderNodes = new List<DefenderNode>();

    [Header("Mines")]
    public GameObject minePrefab;
    public int mineCost = 60;
    private readonly List<MiningNode> miningNodes = new List<MiningNode>();

    void Start()
    {
        CacheNodesFromMap();
        UpdateResourceText();
    }

    /// <summary>
    /// Collects both DefenderNodes and MiningNodes that the ProceduralMap generated.
    /// </summary>
    void CacheNodesFromMap()
    {
        defenderNodes.Clear();
        miningNodes.Clear();

        if (map == null)
        {
            Debug.LogWarning("PlacementManager: No ProceduralMap assigned.");
            return;
        }

        if (map.defenderNodes != null && map.defenderNodes.Count > 0)
        {
            foreach (var dn in map.defenderNodes)
                if (dn != null) defenderNodes.Add(dn);
        }
        else
        {
            Debug.Log("PlacementManager: No defender nodes found.");
        }

        if (map.miningNodes != null && map.miningNodes.Count > 0)
        {
            foreach (var mn in map.miningNodes)
                if (mn != null) miningNodes.Add(mn);
        }
        else
        {
            Debug.Log("PlacementManager: No mining nodes found.");
        }
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
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

        // Defender node check
        DefenderNode defenderNode = hit.collider.GetComponent<DefenderNode>();
        if (defenderNode != null && !defenderNode.isOccupied)
        {
            TryPlaceDefender(defenderNode);
            return;
        }

        // Mining node check
        MiningNode miningNode = hit.collider.GetComponent<MiningNode>();
        if (miningNode != null && !miningNode.isOccupied)
        {
            TryPlaceMine(miningNode);
            return;
        }
    }

    void TryPlaceDefender(DefenderNode node)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentResources >= defenderCost)
        {
            node.PlaceDefender(defenderPrefab);
            GameManager.Instance.SpendResources(defenderCost);
            UpdateResourceText();
        }
        else
        {
            Debug.Log("Not enough resources for defender.");
        }
    }

    void TryPlaceMine(MiningNode node)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentResources >= mineCost)
        {
            node.PlaceMine(minePrefab);
            GameManager.Instance.SpendResources(mineCost);
            UpdateResourceText();
        }
        else
        {
            Debug.Log("Not enough resources for mine.");
        }
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
