using UnityEngine;

public class Defender : MonoBehaviour, IDamageableDefender
{
    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Combat")]
    public float attackRange = 8f;
    public float attackRate = 1.2f;
    public int damage = 10;
    public GameObject impactVFX;

    private float attackTimer = 0f;

    [Header("UI")]
    public GameObject healthBarPrefab;   
    private EnemyHealthBar healthBar;    

    void Start()
    {
        currentHealth = maxHealth;

        //  Spawn health bar
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false); 
            healthBar = hb.GetComponent<EnemyHealthBar>(); 
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth, maxHealth);
            }
        }
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                target.ReceiveDamage(damage);
                if (impactVFX) Instantiate(impactVFX, target.transform.position, Quaternion.identity);
                attackTimer = 0f;
            }
        }

        
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }

    Enemy FindClosestEnemy()
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
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;

       
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            if (healthBar != null)
                Destroy(healthBar.gameObject);

            Destroy(gameObject);
        }
    }
}
