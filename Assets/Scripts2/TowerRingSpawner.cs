using UnityEngine;
using System.Collections.Generic;

public class TowerRingSpawner : MonoBehaviour
{
    [Header("References")]
    public Tower mainTower; // the central main tower (your Tower instance)
    public GameObject towerPrefab; // a defender prefab that has Tower + ProceduralTower + Collider
    public Transform[] ringNodes; // optional: empty objects to snap to. Can be null.

    [Header("Spawn tuning")]
    public float baseRadius = 3.5f;
    public int baseCount = 2;
    public float radiusPerDifficulty = 1.2f;
    public int extraCountPerDifficultyStep = 1;

    [Header("Fusion tuning")]
    public bool attemptDestructiveFusionOnSpawn = true;
    [Range(2, 3)] public int lowWaveBoost = 2;
    [Range(2, 3)] public int highWaveBoost = 3;
    public GameObject spawnParticlePrefab;

    void Start()
    {
        if (mainTower == null)
            mainTower = FindObjectOfType<Tower>();
    }

    public void SpawnRingForWave(float difficulty, int waveIndex)
    {
        if (mainTower == null || towerPrefab == null)
        {
            Debug.LogError("[TowerRingSpawner] MainTower or TowerPrefab not assigned.");
            return;
        }

        float radius = baseRadius + (difficulty - 1f) * radiusPerDifficulty;
        int count = baseCount + Mathf.FloorToInt((difficulty - 1f) * extraCountPerDifficultyStep);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos;

            // Use ring nodes if provided
            if (ringNodes != null && ringNodes.Length > 0)
            {
                Transform node = ringNodes[i % ringNodes.Length];
                pos = node.position;
            }
            else
            {
                // Random circular distribution
                float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
                pos = mainTower.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            }

            // Optional: adjust Y using procedural map
            var map = FindObjectOfType<ProceduralMap>();
            if (map != null)
                pos.y = map.GetHeightAt(pos.x, pos.z) + 0.2f;

            // Instantiate tower
            GameObject go = Instantiate(towerPrefab, pos, Quaternion.identity);
            go.transform.SetParent(transform, true);

            // Get components properly
            Tower towerComponent = go.GetComponent<Tower>();
            if (towerComponent == null)
                towerComponent = go.GetComponentInChildren<Tower>();

            ProceduralTower proceduralComponent = go.GetComponent<ProceduralTower>();
            if (proceduralComponent == null)
                proceduralComponent = go.GetComponentInChildren<ProceduralTower>();

            if (towerComponent == null)
                Debug.LogError($"[TowerRingSpawner] Tower component missing on prefab {towerPrefab.name}");
            if (proceduralComponent == null)
                Debug.LogError($"[TowerRingSpawner] ProceduralTower component missing on prefab {towerPrefab.name}");

            // Configure procedural tower
            if (proceduralComponent != null)
            {
                proceduralComponent.SetSpawnTime(Time.time + waveIndex * 0.01f);
                if (proceduralComponent.GetProfile() == null)
                    proceduralComponent.GenerateRandomProfile();
            }

            // Spawn particle effect
            if (spawnParticlePrefab != null)
            {
                GameObject fx = Instantiate(spawnParticlePrefab, pos + Vector3.up * 0.6f, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // Attempt destructive fusion
            if (attemptDestructiveFusionOnSpawn && proceduralComponent != null && towerComponent != null)
            {
                int boost = (difficulty > 1.6f) ? highWaveBoost : lowWaveBoost;

                ProceduralTower[] all = FindObjectsOfType<ProceduralTower>();
                ProceduralTower nearest = null;
                float bestDist = Mathf.Infinity;

                foreach (var t in all)
                {
                    if (t == proceduralComponent) continue;
                    float d = Vector3.Distance(t.transform.position, proceduralComponent.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        nearest = t;
                    }
                }

                if (nearest != null)
                {
                    Tower t1 = nearest.GetComponent<Tower>();
                    Tower t2 = towerComponent;

                    if (t1 != null && t2 != null)
                        TowerFusionManager.Instance?.FuseDestructive(t1, t2, boost);
                }
            }

            Debug.Log($"[TowerRingSpawner] Spawned tower at {pos}. Tower={towerComponent != null}, ProceduralTower={proceduralComponent != null}");
        }
    }
}