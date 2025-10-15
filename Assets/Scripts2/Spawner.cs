using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject splitterEnemyPrefab;

    [Header("References")]
    public ProceduralMap map;
    public Tower tower;

    [Header("Spawning Settings")]
    public float spawnInterval = 2.5f;
    [Range(0f, 1f)] public float tankSpawnChance = 0.25f;
    [Range(0f, 1f)] public float splitterSpawnChance = 0.15f;

    private int nextPathIndex = 0;

    void Start()
    {
        if (map == null)
        {
            map = FindObjectOfType<ProceduralMap>();
            if (map == null)
            {
                Debug.LogError("Spawner: No ProceduralMap found in scene!");
                return;
            }
        }

        if (tower == null)
        {
            tower = FindObjectOfType<Tower>();
            if (tower == null)
            {
                Debug.LogError("Spawner: No Tower found in scene!");
                return;
            }
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (map == null || map.spawnPoints.Count == 0)
        {
            Debug.LogWarning("Spawner: No spawn points found.");
            return;
        }

        // Choose path and spawn position
        int p = nextPathIndex % map.paths.Count;
        nextPathIndex++;

        Vector3 spawnPos = map.spawnPoints[p];

        // --- Align to terrain height to avoid clipping ---
        float terrainY = map.GetHeightAt(spawnPos.x, spawnPos.z);
        spawnPos.y = terrainY + 0.2f;

        // --- Choose enemy type ---
        GameObject prefabToSpawn = ChooseEnemyType();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Spawner: No prefab assigned to spawn!");
            return;
        }

        // --- Instantiate enemy ---
        GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // --- Initialize enemy route ---
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            List<Vector3> path = map.paths[p];
            enemy.InitRoute(path.ToArray(), tower);
        }
        else
        {
            Debug.LogWarning($"Spawner: {prefabToSpawn.name} has no Enemy component!");
        }
    }

    // ---------------------------------------------------
    // Randomly choose which enemy type to spawn
    // ---------------------------------------------------
    GameObject ChooseEnemyType()
    {
        float roll = Random.value;

        if (roll < splitterSpawnChance)
        {
            return splitterEnemyPrefab;
        }
        else if (roll < splitterSpawnChance + tankSpawnChance)
        {
            return tankEnemyPrefab;
        }
        else
        {
            return basicEnemyPrefab;
        }
    }
}
