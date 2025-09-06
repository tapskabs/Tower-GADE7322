using UnityEngine;

public class DefenderNode : MonoBehaviour
{
    public bool isOccupied = false;
    public GameObject placedDefender;

    public void PlaceDefender(GameObject defenderPrefab)
    {
        if (isOccupied) return;
        placedDefender = Instantiate(defenderPrefab, transform.position, Quaternion.identity);
        isOccupied = true;
    }

    public void RemoveDefender()
    {
        if (placedDefender) Destroy(placedDefender);
        isOccupied = false;
    }
}
