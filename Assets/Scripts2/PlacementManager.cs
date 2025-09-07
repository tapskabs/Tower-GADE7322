using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    public ProceduralMap map;
    // nodePrefab is no longer required because ProceduralMap instantiates nodes.
    // public GameObject nodePrefab;         
    public GameObject defenderPrefab;     // the defender GameObject to place
    public int defenderCost = 50;
    public TextMeshProUGUI resourceText;

    private List<DefenderNode> nodes = new List<DefenderNode>();

    void Start()
    {
        CreateNodesFromMap();
        UpdateResourceText();
    }

    // Collect the DefenderNode instances that the ProceduralMap already created
    void CreateNodesFromMap()
    {
        nodes.Clear();
        if (map == null)
        {
            Debug.LogWarning("PlacementManager: ProceduralMap reference not set.");
            return;
        }

        if (map.defenderNodes != null && map.defenderNodes.Count > 0)
        {
            foreach (var dn in map.defenderNodes)
            {
                if (dn != null) nodes.Add(dn);
            }
        }
        else
        {
            Debug.LogWarning("PlacementManager: No defender nodes found on the map.");
        }
    }

    void Update()
    {
        // click detection for placement
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                DefenderNode node = hit.collider.GetComponent<DefenderNode>();
                if (node != null && !node.isOccupied)
                {
                    if (GameManager.Instance != null && GameManager.Instance.CurrentResources >= defenderCost)
                    {
                        node.PlaceDefender(defenderPrefab);
                        GameManager.Instance.SpendResources(defenderCost);
                        UpdateResourceText();
                    }
                    else
                    {
                        Debug.Log("Not enough resources");
                    }
                }
            }
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void UpdateResourceText()
    {
        if (resourceText)
            resourceText.text = GameManager.Instance != null ? GameManager.Instance.CurrentResources.ToString() : "0";
    }
}
