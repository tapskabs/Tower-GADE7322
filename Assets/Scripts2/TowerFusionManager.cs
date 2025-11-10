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
    [Tooltip("How much faster the tower attacks after fusion (smaller = faster)")]
    public float attackRateMultiplier = 0.5f;

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

    public void UnregisterTower(Tower tower)
    {
        if (tower != null && activeTowers.Contains(tower))
            activeTowers.Remove(tower);
    }

    void Update()
    {
        DetectFusions();
    }

    private void DetectFusions()
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
                    return; // prevent chain fusion in same frame
                }
            }
        }
    }

    private void FuseTowers(Tower t1, Tower t2)
    {
        if (t1 == null || t2 == null) return;

        if (fusionParticlePrefab != null)
        {
            GameObject fx = Instantiate(fusionParticlePrefab, t1.transform.position, Quaternion.identity);
            Destroy(fx, fusionPulseDuration);
        }

        t1.attackRate *= attackRateMultiplier;
        t1.attackRate = Mathf.Clamp(t1.attackRate, 0.1f, 10f);

        Destroy(t2.gameObject);
        UnregisterTower(t2);
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

        Tower survivor = a;
        Tower toDestroy = b;

        survivor.attackRate = Mathf.Max(0.1f, survivor.attackRate / Mathf.Max(1f, boost));
        survivor.attackRange *= 1.05f;
        survivor.SetGlow(true);
        survivor.Invoke(nameof(DisableGlowSafely), 0.6f);

        if (toDestroy != null)
        {
            Destroy(toDestroy.gameObject);
            UnregisterTower(toDestroy);
        }

        return true;
    }

    private void DisableGlowSafely()
    {
        foreach (var t in FindObjectsOfType<Tower>())
            t.SetGlow(false);
    }

    public List<Tower> GetActiveTowers() => activeTowers;
}
