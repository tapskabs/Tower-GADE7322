using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int currentResources = 200;

    public bool SpendResources(int amount)
    {
        if (currentResources >= amount)
        {
            currentResources -= amount;
            return true;
        }
        return false;
    }

    public void AddResources(int amount)
    {
        currentResources += amount;
    }
}
