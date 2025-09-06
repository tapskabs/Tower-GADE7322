using UnityEngine;

public class Defender : MonoBehaviour
{
    public int maxHealth = 80;
    public float attackRate = 1f;
    public float attackRange = 8f;
    public int damage = 20;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private int currentHealth;
    private float timer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackRate)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                FireAt(target);
                timer = 0f;
            }
        }
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float minDist = Mathf.Infinity;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d <= attackRange && d < minDist)
            {
                minDist = d;
                closest = e;
            }
        }
        return closest;
    }

    void FireAt(Enemy target)
    {
        if (projectilePrefab == null)
        {
            // fallback: direct damage
            target.ReceiveDamage(damage);
            return;
        }

        var projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        var proj = projGO.GetComponent<Projectile>();
        if (proj != null) proj.Launch(target, damage);
    }

    public void ReceiveDamage(int d)
    {
        currentHealth -= d;
        if (currentHealth <= 0) Destroy(gameObject);
    }
}
