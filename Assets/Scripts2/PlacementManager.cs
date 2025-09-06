using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacementManager : MonoBehaviour
{
    public ProceduralMap map;
    public GameObject nodePrefab;         // a small transparent disc to show node
    public GameObject defenderPrefab;     // the defender GameObject to place
    public int defenderCost = 50;
    public Text resourceText;

    private List<DefenderNode> nodes = new List<DefenderNode>();

    void Start()
    {
        CreateNodesFromMap();
        UpdateResourceText();
    }

    void CreateNodesFromMap()
    {
        foreach (var pos in map.defenderNodes)
        {
            GameObject g = Instantiate(nodePrefab, pos, Quaternion.identity);
            DefenderNode node = g.AddComponent<DefenderNode>();
            nodes.Add(node);
            // you can also add a collider on nodePrefab and a NodeUI visual
        }
    }

    void Update()
    {
        // click detection for placement
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                DefenderNode node = hit.collider.GetComponent<DefenderNode>();
                if (node != null && !node.isOccupied)
                {
                    if (GameManager.Instance.CurrentResources >= defenderCost)
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
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    void UpdateResourceText()
    {
        if (resourceText) resourceText.text = GameManager.Instance.CurrentResources.ToString();
    }
}
