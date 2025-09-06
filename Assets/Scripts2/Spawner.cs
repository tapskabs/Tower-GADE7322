using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab; // assign an Enemy prefab
    public ProceduralMap map;
    public float spawnInterval = 2.5f;
    public Tower tower;

    private int nextPathIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void Spawn()
    {
        if (map == null || map.spawnPoints.Count == 0) return;
        // pick path (round robin)
        int p = nextPathIndex % map.paths.Count;
        nextPathIndex++;

        Vector3 spawnPos = map.spawnPoints[p];
        GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();

        // convert path List<Vector3> to array and pass
        List<Vector3> path = map.paths[p];
        enemy.InitRoute(path.ToArray(), tower);
    }
}
