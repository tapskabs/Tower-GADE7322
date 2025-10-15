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
        if (miniEnemyPrefab == null) return;

        for (int i = 0; i < splitCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-miniSpawnSpread, miniSpawnSpread),
                0,
                Random.Range(-miniSpawnSpread, miniSpawnSpread)
            );

            GameObject clone = Instantiate(miniEnemyPrefab, transform.position + offset, Quaternion.identity);
            Enemy mini = clone.GetComponent<Enemy>();
            if (mini != null && towerTarget != null)
            {
                mini.InitRoute(route, towerTarget);
            }
        }
    }

    // Override death behavior
    private new void Die()
    {
        SpawnMiniEnemies();
        GameManager.Instance?.AddResources(10);
        if (healthBar != null) Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }
}
