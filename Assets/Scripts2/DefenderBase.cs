using UnityEngine;

public abstract class DefenderBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    public int currentHealth;

    [Header("Combat")]
    public float attackRange = 8f;
    public float attackRate = 1.2f;
    public int attackDamage = 10;
    protected float attackTimer = 0f;
    protected Enemy targetEnemy;

    [Header("UI")]
    public GameObject healthBarPrefab;
    protected EnemyHealthBar healthBar;

    [Header("Visual Effects")]
    public GameObject spawnVFX;     // Plays when defender is placed
    public GameObject deathVFX;     // Plays when defender is destroyed
    public GameObject impactVFX;    // Plays when defender attacks (optional)

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        // Play placement VFX
        if (spawnVFX != null)
        {
            Instantiate(spawnVFX, transform.position, Quaternion.identity);
        }

        // Spawn a floating healthbar
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false);
            healthBar = hb.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
                healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackRate)
        {
            targetEnemy = FindClosestEnemy();
            if (targetEnemy != null)
            {
                Attack();
                attackTimer = 0f;

                // Optional attack VFX
                if (impactVFX != null)
                    Instantiate(impactVFX, targetEnemy.transform.position, Quaternion.identity);
            }
        }

        // Keep healthbar above defender
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }

    public interface IDamageableDefender
    {
        void ReceiveDamage(int dmg);
        Vector3 GetPosition();
    }

    protected abstract void Attack();

    protected Enemy FindClosestEnemy()
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

    public virtual void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            if (deathVFX != null)
                Instantiate(deathVFX, transform.position, Quaternion.identity);

            if (healthBar != null)
                Destroy(healthBar.gameObject);

            Destroy(gameObject);
        }
    }
}
