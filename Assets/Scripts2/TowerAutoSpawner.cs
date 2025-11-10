using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TowerAutoSpawner : MonoBehaviour
{
    [Header("References")]
    public Tower mainTower;
    public GameObject towerPrefab;
    public TowerFusionManager fusionManager;

    [Header("Spawn Settings")]
    public int baseTowersPerWave = 3;
    public float baseRadius = 8f;
    public float spawnDelay = 0.3f;

    private List<Tower> spawnedTowers = new List<Tower>();

    void Start()
    {
        if (mainTower == null) mainTower = FindObjectOfType<Tower>();
        if (fusionManager == null) fusionManager = FindObjectOfType<TowerFusionManager>();
    }

    public void SpawnTowersForWave(int waveIndex)
    {
        StartCoroutine(SpawnTowersRoutine(waveIndex));
    }

    private IEnumerator SpawnTowersRoutine(int waveIndex)
    {
        int towerCount = baseTowersPerWave + Mathf.FloorToInt(waveIndex * 0.5f);
        float radius = baseRadius + waveIndex * 0.6f;

        for (int i = 0; i < towerCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 pos = mainTower.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            GameObject tObj = Instantiate(towerPrefab, pos, Quaternion.identity);
            Tower t = tObj.GetComponent<Tower>();

            if (t != null)
            {
                spawnedTowers.Add(t);
                fusionManager?.RegisterTower(t);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
