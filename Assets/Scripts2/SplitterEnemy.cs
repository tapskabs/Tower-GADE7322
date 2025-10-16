using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterEnemy : Enemy
{
    [Header("Splitter Enemy Settings")]
    public GameObject miniEnemyPrefab;      // Prefab for the mini enemies
    public int splitCount = 2;              // How many minis spawn on death
    public float miniSpawnSpread = 1.5f;    // Random spawn offset
    public float miniLifespan = 8f;         // How long minis exist before despawning
    public float miniDamageMultiplier = 0.5f; // Minis do half normal damage
    public float miniSpeedMultiplier = 1.3f;  // Minis move slightly faster

    private IDamageableDefender currentDefenderTarget;

    // Slightly faster than base enemies
    protected override void Start()
    {
        base.Start();
        baseSpeed *= 1.2f;
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

    // --- Spawn Minis ---
    private void SpawnMiniEnemies()
    {
        if (miniEnemyPrefab == null || route == null || towerTarget == null)
            return;

        for (int i = 0; i < splitCount; i++)
        {
            // Spawn position with small random offset
            Vector3 offset = new Vector3(
                Random.Range(-miniSpawnSpread, miniSpawnSpread),
                0,
                Random.Range(-miniSpawnSpread, miniSpawnSpread)
            );

            Vector3 spawnPos = transform.position + offset;

            // Align Y with terrain
            ProceduralMap map = FindObjectOfType<ProceduralMap>();
            if (map != null)
            {
                spawnPos.y = map.GetHeightAt(spawnPos.x, spawnPos.z) + 0.5f;
            }

            GameObject clone = Instantiate(miniEnemyPrefab, spawnPos, Quaternion.identity);
            Enemy mini = clone.GetComponent<Enemy>();

            if (mini != null)
            {
                // Align to route and tower
                Vector3 safeStart = GetClosestPointOnRoute(route, spawnPos);
                mini.transform.position = safeStart;
                mini.InitRoute(route, towerTarget);

                // Adjust stats for minis
                mini.maxHealth = Mathf.RoundToInt(this.maxHealth * 0.4f);
                mini.baseSpeed *= miniSpeedMultiplier;
                mini.damage = Mathf.RoundToInt(this.damage * miniDamageMultiplier);
                mini.ReceiveDamage(0); // refresh health UI

                // Add self-despawn after lifespan
                mini.StartCoroutine(DespawnAfterTime(mini, miniLifespan));
            }
        }
    }

    // --- Helper: Find Closest Route Point ---
    private Vector3 GetClosestPointOnRoute(IEnumerable<Vector3> routePoints, Vector3 fromPos)
    {
        if (routePoints == null) return fromPos;

        Vector3 closest = fromPos;
        float minDist = float.MaxValue;

        foreach (var point in routePoints)
        {
            float d = Vector3.SqrMagnitude(point - fromPos);
            if (d < minDist)
            {
                minDist = d;
                closest = point;
            }
        }

        // Snap to terrain height
        ProceduralMap map = FindObjectOfType<ProceduralMap>();
        if (map != null)
            closest.y = map.GetHeightAt(closest.x, closest.z) + 0.4f;

        return closest;
    }

    // --- Despawn Mini after timer ---
    private IEnumerator DespawnAfterTime(Enemy e, float time)
    {
        yield return new WaitForSeconds(time);
        if (e != null)
        {
            e.ReceiveDamage(e.maxHealth); // kills mini cleanly
        }
    }

    // --- Defender Detection & Attack ---
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

    // --- Update keeps health bar aligned ---
    protected override void Update()
    {
        base.Update();

        // Keep health bar following enemy
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }
}
