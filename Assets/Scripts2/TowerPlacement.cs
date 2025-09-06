using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    public ProceduralMap map;
    public GameObject towerPrefab;

    private void Start()
    {
        PlaceTowerAtCenter();
    }

    void PlaceTowerAtCenter()
    {
        if (map == null || towerPrefab == null) return;

        Vector3 towerPos = map.centerPoint;
        // Adjust Y to match terrain height
        towerPos.y = map.GetHeightAt(towerPos.x, towerPos.z);

        Instantiate(towerPrefab, towerPos, Quaternion.identity);
    }
}
