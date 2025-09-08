using UnityEngine;

public class MiningNode : MonoBehaviour
{
    public bool isOccupied = false;

    public void PlaceMine(GameObject minePrefab)
    {
        if (isOccupied) return;
        Instantiate(minePrefab, transform.position, Quaternion.identity);
        isOccupied = true;
    }
}
