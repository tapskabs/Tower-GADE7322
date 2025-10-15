using UnityEngine;

public abstract class DefenderBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    protected int currentHealth;

    [Header("Attack")]
    public float attackRange = 5f;
    public float attackRate = 1f;
    public int attackDamage = 10;

    protected float attackTimer;
    protected Transform targetEnemy;

    [Header("UI")]
    public GameObject healthBarPrefab;
    protected EnemyHealthBar healthBar;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        // Spawn health bar
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false);
            healthBar = hb.GetComponent<EnemyHealthBar>();
            healthBar?.SetHealth(currentHealth, maxHealth);
        }
    }

    protected virtual void Update()
    {
        attackTimer -= Time.deltaTime;

        if (targetEnemy == null)
            FindTarget();

        if (targetEnemy != null && attackTimer <= 0f)
        {
            Attack();
            attackTimer = 1f / attackRate;
        }

        // Update health bar position
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }

    protected void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float closest = Mathf.Infinity;
        Transform nearest = null;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < closest && dist <= attackRange)
            {
                closest = dist;
                nearest = e.transform;
            }
        }

        targetEnemy = nearest;
    }

    // ✅ New: defenders can take damage
    public virtual void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
        healthBar?.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            if (healthBar != null)
                Destroy(healthBar.gameObject);
            Destroy(gameObject);
        }
    }

    protected abstract void Attack();
}
