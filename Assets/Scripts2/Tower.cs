using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    public int maxHealth = 200;
    public float attackRate = 1f;
    public float attackRange = 10f;
    public int damage = 15;
    public GameObject impactVFX;

    [Header("UI")]
    public Slider healthSlider; 

    private int currentHealth;
    private float attackTimer;

    void Start()
    {
        currentHealth = maxHealth;
        GameManager.Instance?.UpdateTowerHealth(currentHealth, maxHealth);

        if (healthSlider != null)
            healthSlider.value = 1f; 
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            Enemy target = FindClosestEnemyInRange();
            if (target != null)
            {
               
                target.ReceiveDamage(damage);
                if (impactVFX) Instantiate(impactVFX, target.transform.position, Quaternion.identity);
                attackTimer = 0f;
            }
        }
    }

    Enemy FindClosestEnemyInRange()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float minDist = Mathf.Infinity;
        foreach (Enemy e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist && d <= attackRange)
            {
                minDist = d;
                closest = e;
            }
        }
        return closest;
    }
    public void ApplyHealthMultiplier(float multiplier)
    {
        // increase maxHealth
        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * multiplier));

        // restore to full health and update UI
        RestoreFullHealth();
    }
    public void RestoreFullHealth()
    {
        // access the private currentHealth field which exists in this class
        // (we are inside Tower so this is allowed)
        // set it to maxHealth and update UI
        // note: currentHealth is already in your class; this method uses it.
        var currentHealthField = typeof(Tower).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (currentHealthField != null)
        {
            currentHealthField.SetValue(this, maxHealth);
        }
        // Update GameManager UI if available
        GameManager.Instance?.UpdateTowerHealth(maxHealth, maxHealth);

        // Update local slider if present
        if (healthSlider != null)
            healthSlider.value = 1f;
    }
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        GameManager.Instance?.UpdateTowerHealth(currentHealth, maxHealth);

        if (healthSlider != null)
            healthSlider.value = (float)currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            GameManager.Instance?.OnGameOver();
        }
    }
}
