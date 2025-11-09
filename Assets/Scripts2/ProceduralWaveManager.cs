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
    public TowerRingSpawner towerRingSpawner; // NEW: assign in inspector

    [Header("UI")]
    public TextMeshProUGUI waveText;

    [Header("Wave Settings")]
    public int baseEnemiesPerWave = 5;
    public float baseSpawnInterval = 2.5f;
    public float waveDelay = 5f;

    [Header("Regulation Targets")]
    public float targetClearTime = 15f;
    public float targetTowerHealth = 0.9f;

    [Header("Regulation Strengths")]
    [Range(0f, 1f)] public float adjustmentSpeed = 0.2f;
    [Range(0.5f, 3f)] public float minDifficulty = 0.8f;
    [Range(0.5f, 3f)] public float maxDifficulty = 3f;

    private int currentWave = 0;
    private float currentDifficulty = 1f;
    private float lastWaveTime = 0f;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private float waveStartTime;

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
            Debug.Log($"🌊 Wave {currentWave} starting. Difficulty = {currentDifficulty:F2}");

            // NEW: spawn ring of towers for this wave (if a spawner is assigned)
            if (towerRingSpawner != null)
            {
                towerRingSpawner.SpawnRingForWave(currentDifficulty, currentWave);
            }

            waveStartTime = Time.time;
            yield return StartCoroutine(SpawnWaveRoutine(
                Mathf.RoundToInt(baseEnemiesPerWave * currentDifficulty),
                Mathf.Lerp(baseSpawnInterval, 0.8f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty)),
                GetHealthMult(),
                GetDamageMult(),
                GetSpeedMult()
            ));

            yield return new WaitUntil(() => activeEnemies.Count == 0);

            lastWaveTime = Time.time - waveStartTime;
            RegulateDifficulty();
        }
    }

    void UpdateWaveUI()
    {
        if (waveText != null) waveText.text = $"Wave: {currentWave}";
    }

    IEnumerator SpawnWaveRoutine(int enemiesToSpawn, float spawnInterval, float healthMult, float damageMult, float speedMult)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (spawner == null) { Debug.LogError("[WaveManager] Spawner not assigned/found."); yield break; }

            Enemy e = spawner.SpawnEnemyDirect(healthMult, damageMult);
            if (e != null)
            {
                try
                {
                    var bsField = e.GetType().GetField("baseSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (bsField != null)
                    {
                        float bs = (float)bsField.GetValue(e);
                        bsField.SetValue(e, bs * speedMult);
                    }
                }
                catch { }

                activeEnemies.Add(e);
            }

            if (tower != null && GetTowerHealthRatio() <= 0f) { Debug.Log("[WaveManager] Tower destroyed mid-wave, stopping spawning."); yield break; }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void RegulateDifficulty()
    {
        float towerHealthRatio = GetTowerHealthRatio();
        float clearTimeFactor = targetClearTime / Mathf.Max(lastWaveTime, 1f);
        float performanceScore = (clearTimeFactor * 0.6f) + (towerHealthRatio / targetTowerHealth * 0.4f);
        float error = performanceScore - 1f;

        currentDifficulty += error * adjustmentSpeed;
        currentDifficulty = Mathf.Clamp(currentDifficulty, minDifficulty, maxDifficulty);
        spawner.spawnInterval = Mathf.Lerp(3f, 0.8f, Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty));

        Debug.Log($"Wave {currentWave} ended. ⏱ {lastWaveTime:F1}s | ❤️ TowerHP={towerHealthRatio:P0} | ⚙️ Performance={performanceScore:F2} | 🔺 Difficulty={currentDifficulty:F2}");
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
