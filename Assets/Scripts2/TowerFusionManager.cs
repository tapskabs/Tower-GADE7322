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
    public float fusionRadius = 4f;          // how far to search neighbours
    [Range(0f, 1f)] public float fusionChance = 0.9f; // chance fusion triggers when requested
    public int maxNeighborsConsidered = 4;
    public bool autoFuseOnPlacement = true;

    [Header("Visual")]
    public GameObject fusionParticlePrefab;
    public float fusionPulseDuration = 1.2f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Attempt to fuse the nearest tower cluster around a given tower.
    /// Returns true if fusion applied.
    /// </summary>
    public bool TryFuse(ProceduralTower centre)
    {
        if (centre == null) return false;
        if (Random.value > fusionChance) return false;

        // find neighbors
        Collider[] hits = Physics.OverlapSphere(centre.transform.position, fusionRadius);
        List<ProceduralTower> neighbors = new List<ProceduralTower>();
        foreach (var h in hits)
        {
            if (h == null) continue;
            if (h.gameObject == centre.gameObject) continue;

            ProceduralTower pt = h.GetComponentInParent<ProceduralTower>();
            if (pt != null) neighbors.Add(pt);
        }

        if (neighbors.Count == 0) return false;

        // limit neighbor count
        if (neighbors.Count > maxNeighborsConsidered)
            neighbors = neighbors.GetRange(0, maxNeighborsConsidered);

        centre.FuseWithNeighbors(neighbors.ToArray());

        // play visual pulse
        if (fusionParticlePrefab != null)
        {
            var fx = Instantiate(fusionParticlePrefab, centre.transform.position + Vector3.up * 0.6f, Quaternion.identity);
            if (fx != null) Destroy(fx, fusionPulseDuration + 0.2f);
        }

        Debug.Log($"[Fusion] Fused {centre.name} with {neighbors.Count} neighbors. New profile: {centre.GetProfile()?.seedName}");
        return true;
    }

    /// <summary>
    /// Helper for UI to auto-fuse nearest cluster to screen center or to selected tower.
    /// </summary>
    public bool TryFuseNearest(Vector3 worldPos)
    {
        // find nearest ProceduralTower in scene
        ProceduralTower[] all = FindObjectsOfType<ProceduralTower>();
        ProceduralTower best = null;
        float bestDist = float.MaxValue;
        foreach (var t in all)
        {
            float d = Vector3.Distance(t.transform.position, worldPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        if (best == null) return false;
        return TryFuse(best);
    }

    // Called by PlacementManager after placing a new tower (if autoFuseOnPlacement enabled)
    public void OnTowerPlaced(ProceduralTower tower)
    {
        if (autoFuseOnPlacement)
        {
            TryFuse(tower);
        }
    }
}
