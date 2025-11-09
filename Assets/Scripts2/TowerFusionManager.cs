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
    public float fusionRadius = 4f;                   // Distance threshold for fusion
    public GameObject fusionParticlePrefab;           // FX for fusion event
    public float fusionPulseDuration = 1.2f;          // FX lifetime
    [Tooltip("How much faster the tower attacks after fusion (smaller = faster)")]
    public float attackRateMultiplier = 0.5f;         // 0.5f = twice as fast

    private readonly List<Tower> activeTowers = new List<Tower>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterTower(Tower tower)
    {
        if (tower != null && !activeTowers.Contains(tower))
            activeTowers.Add(tower);
    }

    void Update()
    {
        DetectFusions();
    }

    void DetectFusions()
    {
        for (int i = 0; i < activeTowers.Count; i++)
        {
            Tower a = activeTowers[i];
            if (a == null) continue;

            for (int j = i + 1; j < activeTowers.Count; j++)
            {
                Tower b = activeTowers[j];
                if (b == null) continue;

                float dist = Vector3.Distance(a.transform.position, b.transform.position);

                bool close = dist <= fusionRadius;
                a.SetGlow(close);
                b.SetGlow(close);

                if (close)
                {
                    FuseTowers(a, b);
                    return; // Prevent chain fusions in the same frame
                }
            }
        }
    }

    void FuseTowers(Tower t1, Tower t2)
    {
        if (t1 == null || t2 == null) return;

        // Play particle effect
        if (fusionParticlePrefab)
        {
            GameObject fx = Instantiate(fusionParticlePrefab, t1.transform.position, Quaternion.identity);
            Destroy(fx, fusionPulseDuration);
        }

        // Boost t1's attack rate (lower value = faster)
        t1.attackRate *= attackRateMultiplier;
        t1.attackRate = Mathf.Clamp(t1.attackRate, 0.1f, 10f); // Prevent unrealistic values

        // Destroy the later-spawned tower
        Destroy(t2.gameObject);
        activeTowers.Remove(t2);

        Debug.Log($"[Fusion] {t1.name} fused with {t2.name}. New attack rate: {t1.attackRate}");
    }


    public bool FuseDestructive(Tower a, Tower b, float boost)
    {
        if (a == null || b == null) return false;
        if (fusionParticlePrefab == null)
            Debug.LogWarning("No fusion particle prefab assigned.");

        // --- play fusion visual ---
        Vector3 midpoint = (a.transform.position + b.transform.position) * 0.5f;
        if (fusionParticlePrefab != null)
        {
            GameObject fx = Instantiate(fusionParticlePrefab, midpoint + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(fx, fusionPulseDuration);
        }

        // --- choose survivor ---
        Tower survivor = a;   // earlier tower survives
        Tower toDestroy = b;  // later tower gets removed

        // --- apply fusion effect ---
        float appliedBoost = Mathf.Max(1f, boost); // ensure it’s never below 1
        survivor.attackRate = Mathf.Max(0.1f, survivor.attackRate / appliedBoost); // faster rate
        survivor.attackRange *= 1.05f; // slight range bonus

        // optional: pulse glow to indicate fusion success
        survivor.SetGlow(true);
        survivor.Invoke(nameof(DisableGlowSafely), 0.6f);

        // --- destroy the weaker one ---
        if (toDestroy != null)
            Destroy(toDestroy.gameObject);

        Debug.Log($"[Fusion] {a.name} fused with {b.name}. Boost {appliedBoost:F2}, attack rate now {survivor.attackRate:F2}");

        return true;
    }

    // Helper for glow disable
    private void DisableGlowSafely()
    {
        Tower[] towers = FindObjectsOfType<Tower>();
        foreach (var t in towers)
            t.SetGlow(false);
    }
}
