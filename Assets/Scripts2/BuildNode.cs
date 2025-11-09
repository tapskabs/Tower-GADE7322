using UnityEngine;

public class BuildNode : MonoBehaviour
{
     public bool isOccupied = false;
    public GameObject placedStructure;

    public GameObject PlaceStructure(GameObject structurePrefab)
    {
        if (isOccupied || structurePrefab == null)
            return null;

        placedStructure = Instantiate(structurePrefab, transform.position, Quaternion.identity);
        isOccupied = true;

        return placedStructure;
    }

    public void RemoveStructure()
    {
        if (placedStructure)
            Destroy(placedStructure);
        isOccupied = false;
    }
}
