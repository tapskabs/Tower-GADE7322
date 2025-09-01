using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public int gold = 100;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    public void Add(int amount) { gold += amount; }


    public bool TrySpend(int amount)
    {
        if (gold >= amount) { gold -= amount; return true; }
        return false;
    }
}