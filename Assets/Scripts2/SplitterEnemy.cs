using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterEnemy : Enemy
{
    [Header("Splitter Enemy Settings")]
    public GameObject miniEnemyPrefab;  // assign small enemy prefab in Unity
    public int splitCount = 2;          // how many spawn on death
    public float miniSpawnSpread = 1.5f;

    

    protected override void Start()
    {
        base.Start();
        baseSpeed *= 1.2f; // slightly faster than default enemy

        
    }

    private void SpawnMiniEnemies()
    {
        if (miniEnemyPrefab == null || route == null || towerTarget == null) return;

        for (int i = 0; i < splitCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-miniSpawnSpread, miniSpawnSpread),
                0,
                Random.Range(-miniSpawnSpread, miniSpawnSpread)
            );

            Vector3 spawnPos = transform.position + offset;

            // Align to map height if map exists
            ProceduralMap map = FindObjectOfType<ProceduralMap>();
            if (map != null)
            {
                spawnPos.y = map.GetHeightAt(spawnPos.x, spawnPos.z) + 0.5f;
            }

            GameObject clone = Instantiate(miniEnemyPrefab, spawnPos, Quaternion.identity);
            Enemy mini = clone.GetComponent<Enemy>();
            if (mini != null)
            {
                mini.InitRoute(route, towerTarget);
            }
        }
    }
    protected override void Die()
    {
        // Start the coroutine that handles delayed splitting and cleanup
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        // Optional short pause for visual timing (adjust as needed)
        yield return new WaitForSeconds(0.15f);

        // Spawn mini enemies
        SpawnMiniEnemies();

        // Now call the base Die() to remove health bar, add resources, and destroy this enemy
        base.Die();
    }

    // --- Update keeps health bar correctly positioned ---
    protected override void Update()
    {
        base.Update();

        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }
}
