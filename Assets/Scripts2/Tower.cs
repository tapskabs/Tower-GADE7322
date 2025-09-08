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
