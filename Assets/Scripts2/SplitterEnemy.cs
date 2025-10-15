using UnityEngine;

public class SplitterEnemy : Enemy
{
    [Header("Splitter Enemy Settings")]
    public GameObject miniEnemyPrefab;  // assign a small enemy prefab in Unity
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
                spawnPos.y = map.GetHeightAt(spawnPos.x, spawnPos.z) + 0.2f;
            }

            GameObject clone = Instantiate(miniEnemyPrefab, spawnPos, Quaternion.identity);
            Enemy mini = clone.GetComponent<Enemy>();
            if (mini != null)
            {
                // give the mini the same path and tower target
                mini.InitRoute(route, towerTarget);
            }
        }
    }

    // override Die so splitting happens before cleanup
    protected override void Die()
    {
        SpawnMiniEnemies();
        // then run base cleanup (adds resources, destroys healthbar, destroys gameobject)
        base.Die();
    }
}
