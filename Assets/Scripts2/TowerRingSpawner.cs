using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TowerRingSpawner : MonoBehaviour
{
    [Header("References")]
    public Tower mainTower; // central tower
    public GameObject towerPrefab; // prefab with Tower + ProceduralTower
    public Transform[] ringNodes; // optional fixed nodes

    [Header("Spawn Settings")]
    public float baseRadius = 3.5f;
    public int baseCount = 2;
    public float radiusPerDifficulty = 1.2f;
    public int extraCountPerDifficultyStep = 1;
    public float spawnDelay = 0.2f;

    [Header("Fusion Settings")]
    public bool attemptDestructiveFusionOnSpawn = true;
    [Range(2, 3)] public int lowWaveBoost = 2;
    [Range(2, 3)] public int highWaveBoost = 3;
    public GameObject spawnParticlePrefab;

    private ProceduralMap map;

    void Start()
    {
        if (mainTower == null)
            mainTower = FindObjectOfType<Tower>();
        map = FindObjectOfType<ProceduralMap>();
    }

    public void SpawnRingForWave(float difficulty, int waveIndex)
    {
        if (mainTower == null || towerPrefab == null)
        {
            Debug.LogError("[TowerRingSpawner] Missing main tower or prefab!");
            return;
        }

        int count = baseCount + Mathf.FloorToInt((difficulty - 1f) * extraCountPerDifficultyStep);
        float radius = baseRadius + (difficulty - 1f) * radiusPerDifficulty;

        StartCoroutine(SpawnRingRoutine(count, radius, difficulty, waveIndex));
    }

    private IEnumerator SpawnRingRoutine(int count, float radius, float difficulty, int waveIndex)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos;
            if (ringNodes != null && ringNodes.Length > 0)
            {
                Transform node = ringNodes[i % ringNodes.Length];
                pos = node.position;
            }
            else
            {
                float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
                pos = mainTower.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            }

            if (map != null)
                pos.y = map.GetHeightAt(pos.x, pos.z) + 0.5f; // spawn slightly above terrain

            GameObject towerObj = Instantiate(towerPrefab, pos, Quaternion.identity, transform);

            Tower towerComp = towerObj.GetComponent<Tower>();
            ProceduralTower proceduralComp = towerObj.GetComponent<ProceduralTower>();
            if (proceduralComp != null)
            {
                proceduralComp.SetSpawnTime(Time.time + waveIndex * 0.01f);
                if (proceduralComp.GetProfile() == null) proceduralComp.GenerateRandomProfile();
            }

            if (spawnParticlePrefab != null)
            {
                GameObject fx = Instantiate(spawnParticlePrefab, pos + Vector3.up * 0.6f, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // --- Fusion on spawn with small delay to avoid timing issues ---
            if (attemptDestructiveFusionOnSpawn && proceduralComp != null && towerComp != null)
            {
                int boost = (difficulty > 1.6f) ? highWaveBoost : lowWaveBoost;
                StartCoroutine(DelayedFusion(towerComp, boost));
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private IEnumerator DelayedFusion(Tower towerComp, int boost)
    {
        yield return new WaitForSeconds(0.05f); // short delay

        Tower nearest = null;
        float bestDist = Mathf.Infinity;

        foreach (var t in TowerFusionManager.Instance.GetActiveTowers())
        {
            if (t == towerComp) continue;
            float d = Vector3.Distance(t.transform.position, towerComp.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = t;
            }
        }

        if (nearest != null)
            TowerFusionManager.Instance.FuseDestructive(nearest, towerComp, boost);

        TowerFusionManager.Instance.RegisterTower(towerComp); // make sure this tower is tracked
    }
}