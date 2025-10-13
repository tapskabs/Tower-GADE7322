using UnityEngine;

public class BuildNode : MonoBehaviour
{
    public bool isOccupied = false;
    public GameObject placedStructure;

    public void PlaceStructure(GameObject structurePrefab)
    {
        if (isOccupied) return;

        placedStructure = Instantiate(structurePrefab, transform.position, Quaternion.identity);
        isOccupied = true;
    }

    public void RemoveStructure()
    {
        if (placedStructure)
            Destroy(placedStructure);
        isOccupied = false;
    }
}
