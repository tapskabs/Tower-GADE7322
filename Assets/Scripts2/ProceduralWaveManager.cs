using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Collections;
using System.Collections.Generic;

public class ProceduralWaveManager : MonoBehaviour
{
    [Header("References")]
    public Spawner spawner;
    public Tower tower;
    public TowerRingSpawner towerRingSpawner;

    [Header("UI")]
    public TextMeshProUGUI waveText;

    [Header("Wave Settings")]
    public int baseEnemiesPerWave = 5;
    public float baseSpawnInterval = 2.5f;
    public float waveDelay = 5f;

    [Header("Difficulty Regulation")]
    public float targetClearTime = 15f;
    public float targetTowerHealth = 0.9f;
    [Range(0f, 1f)] public float adjustmentSpeed = 0.2f;
    [Range(0.5f, 3f)] public float minDifficulty = 0.8f;
    [Range(0.5f, 3f)] public float maxDifficulty = 3f;

    private int currentWave = 0;
    private float currentDifficulty = 1f;
    private float lastWaveTime = 0f;
    private float waveStartTime;
    private List<Enemy> activeEnemies = new List<Enemy>();

    void Start()
    {
        if (spawner == null) spawner = FindObjectOfType<Spawner>();
        if (tower == null) tower = FindObjectOfType<Tower>();
        if (towerRingSpawner == null) towerRingSpawner = FindObjectOfType<TowerRingSpawner>();

        UpdateWaveUI();
        StartCoroutine(WaveLoop());
    }

    void Update()
    {
        activeEnemies.RemoveAll(e => e == null);
    }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(2f);

        while (tower != null)
        {
            yield return new WaitForSeconds(waveDelay);
            currentWave++;
            UpdateWaveUI();

            if (towerRingSpawner != null)
                towerRingSpawner.SpawnRingForWave(currentDifficulty, currentWave);

            waveStartTime = Time.time;
            yield return StartCoroutine(SpawnWaveRoutine(
                Mathf.RoundToInt(baseEnemiesPerWave * currentDifficulty),
                Mathf.Lerp(baseSpawnInterval, 0.8f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty)),
                GetHealthMult(), GetDamageMult(), GetSpeedMult()
            ));

            yield return new WaitUntil(() => activeEnemies.Count == 0);

            lastWaveTime = Time.time - waveStartTime;
            RegulateDifficulty();
        }
    }

    void UpdateWaveUI() => waveText.text = $"Wave: {currentWave}";

    IEnumerator SpawnWaveRoutine(int enemiesToSpawn, float spawnInterval, float healthMult, float damageMult, float speedMult)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (spawner == null) yield break;

            Enemy e = spawner.SpawnEnemyDirect(healthMult, damageMult);
            if (e != null)
            {
                var bsField = e.GetType().GetField("baseSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (bsField != null) bsField.SetValue(e, (float)bsField.GetValue(e) * speedMult);

                activeEnemies.Add(e);
            }

            if (tower != null && GetTowerHealthRatio() <= 0f) yield break;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void RegulateDifficulty()
    {
        float towerHealthRatio = GetTowerHealthRatio();
        float clearTimeFactor = targetClearTime / Mathf.Max(lastWaveTime, 1f);
        float performanceScore = clearTimeFactor * 0.6f + (towerHealthRatio / targetTowerHealth * 0.4f);
        float error = performanceScore - 1f;

        currentDifficulty += error * adjustmentSpeed;
        currentDifficulty = Mathf.Clamp(currentDifficulty, minDifficulty, maxDifficulty);

        if (spawner != null) spawner.spawnInterval = Mathf.Lerp(3f, 0.8f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty));
    }

    float GetTowerHealthRatio()
    {
        var field = typeof(Tower).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int currentHealth = tower.maxHealth;
        if (field != null) currentHealth = (int)field.GetValue(tower);
        return Mathf.Clamp01((float)currentHealth / tower.maxHealth);
    }

    float GetHealthMult() => Mathf.Lerp(1f, 2.5f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty));
    float GetDamageMult() => Mathf.Lerp(1f, 1.8f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty));
    float GetSpeedMult() => Mathf.Lerp(1f, 1.4f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty));
}
