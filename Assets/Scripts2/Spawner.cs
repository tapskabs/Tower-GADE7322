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

    //private int nextPathIndex = 0;

    void Start()
    {
        if (map == null) map = FindObjectOfType<ProceduralMap>();
        if (tower == null) tower = FindObjectOfType<Tower>();
    }

    // --- Used by ProceduralWaveManager ---
    public Enemy SpawnEnemyDirect(float healthMult = 1f, float damageMult = 1f)
    {
        if (map == null || map.spawnPoints.Count == 0) return null;

        int p = Random.Range(0, map.paths.Count);
        Vector3 spawnPos = map.spawnPoints[p];
        spawnPos.y = map.GetHeightAt(spawnPos.x, spawnPos.z) + 0.2f;

        GameObject prefab = ChooseEnemyType();
        if (prefab == null) return null;

        GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.InitRoute(map.paths[p].ToArray(), tower);
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * healthMult);
            enemy.damage = Mathf.RoundToInt(enemy.damage * damageMult);
        }

        return enemy;
    }

    GameObject ChooseEnemyType()
    {
        float roll = Random.value;

        if (roll < splitterSpawnChance)
            return splitterEnemyPrefab;
        else if (roll < splitterSpawnChance + tankSpawnChance)
            return tankEnemyPrefab;
        else
            return basicEnemyPrefab;
    }
}
