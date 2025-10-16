using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterEnemy : Enemy
{
    [Header("Splitter Enemy Settings")]
    public GameObject miniEnemyPrefab;   // assign small enemy prefab in Unity
    public int splitCount = 2;           // how many spawn on death
    public float miniSpawnSpread = 1.5f;
    public float miniLifespan = 8f;      // how long minis last before despawning
    public float miniDamageMultiplier = 0.5f; // minis do 50% of normal damage
    public float miniSpeedMultiplier = 1.3f;  // minis move faster
    private IDamageableDefender currentDefenderTarget;
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
                // Initialize movement route and target tower
                mini.InitRoute(route, towerTarget);

                // Adjust stats to make them unique
                mini.maxHealth = Mathf.RoundToInt(this.maxHealth * 0.4f);
                mini.ReceiveDamage(0); // refresh health bar visuals
                mini.baseSpeed *= miniSpeedMultiplier;

                // Reduce attack damage if minis can attack
                if (mini is SplitMiniEnemy miniScript)
                {
                    miniScript.SetDamageMultiplier(miniDamageMultiplier);
                    miniScript.StartSelfDestruct(miniLifespan);
                }
                else
                {
                    // fallback for normal Enemy-derived minis
                    mini.StartCoroutine(DespawnAfterTime(mini, miniLifespan));
                }
            }
        }
    }
    private void DetectNearbyDefender()
    {
        currentDefenderTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, reachRadius);
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Defender"))
            {
                IDamageableDefender dmgDef = hit.GetComponent<IDamageableDefender>();
                if (dmgDef != null)
                {
                    float dist = Vector3.Distance(transform.position, dmgDef.GetPosition());
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        currentDefenderTarget = dmgDef;
                    }
                }
            }
        }
    }
    private void AttackDefender()
    {
        if (currentDefenderTarget == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            currentDefenderTarget.ReceiveDamage(damage);
            attackTimer = 0f;
        }
    }

    private IEnumerator DespawnAfterTime(Enemy e, float time)
    {
        yield return new WaitForSeconds(time);

        if (e != null)
        {
            e.ReceiveDamage(e.maxHealth); // effectively kills it cleanly
        }
    }

    // --- Death Handling ---
    protected override void Die()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(0.15f);
        SpawnMiniEnemies();
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
