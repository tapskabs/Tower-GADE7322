using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place on a singleton object in the scene.
/// Manages fusion attempts between towers and exposure of a small API.
/// </summary>
public class TowerFusionManager : MonoBehaviour
{
    public static TowerFusionManager Instance;

    [Header("Fusion Settings")]
    public float fusionRadius = 4f;
    public GameObject fusionParticlePrefab;
    public float fusionPulseDuration = 1.2f;
    public float attackRateMultiplier = 0.5f;

    private readonly List<Tower> activeTowers = new List<Tower>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- PUBLIC API ---
    public void RegisterTower(Tower tower)
    {
        if (tower != null && !activeTowers.Contains(tower))
            activeTowers.Add(tower);
    }

    public void UnregisterTower(Tower tower)
    {
        if (tower != null && activeTowers.Contains(tower))
            activeTowers.Remove(tower);
    }

    public List<Tower> GetActiveTowers()
    {
        return activeTowers;
    }

    public bool FuseDestructive(Tower a, Tower b, float boost)
    {
        if (a == null || b == null) return false;

        Vector3 midpoint = (a.transform.position + b.transform.position) * 0.5f;
        if (fusionParticlePrefab != null)
        {
            GameObject fx = Instantiate(fusionParticlePrefab, midpoint + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(fx, fusionPulseDuration);
        }

        // Merge logic: improve one, destroy the other
        a.attackRate *= attackRateMultiplier / Mathf.Max(1f, boost * 0.3f);
        a.attackRate = Mathf.Clamp(a.attackRate, 0.1f, 10f);
        a.attackRange *= 1.05f;

        UnregisterTower(b);
        Destroy(b.gameObject);

        return true;
    }

    // --- AUTO FUSION LOOP ---
    void Update()
    {
        DetectFusions();
    }

    private void DetectFusions()
    {
        Tower[] towers = activeTowers.ToArray();
        for (int i = 0; i < towers.Length; i++)
        {
            Tower a = towers[i];
            if (a == null) continue;

            for (int j = i + 1; j < towers.Length; j++)
            {
                Tower b = towers[j];
                if (b == null) continue;

                float dist = Vector3.Distance(a.transform.position, b.transform.position);
                if (dist <= fusionRadius)
                {
                    FuseDestructive(a, b, 2f);
                    return; // prevent chain reactions same frame
                }
            }
        }
    }
}
