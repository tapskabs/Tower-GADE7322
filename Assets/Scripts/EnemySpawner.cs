using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public float spawnInterval = 2.5f;
    public List<Transform> waypoints; // assigned by TerrainGenerator


    private Coroutine routine;


    public void Begin()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SpawnLoop());
    }
    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            if (enemyPrefab != null && waypoints != null && waypoints.Count > 0)
            {
                Enemy e = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                e.InitPath(waypoints);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}