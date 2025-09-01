using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Combat")]
    public float range = 12f;
    public float fireRate = 1f; // shots per second
    public float damage = 10f;
    public Projectile projectilePrefab;
    public Transform firePoint;


    private float fireCooldown = 0f;
    private Health health;


    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.AddListener(OnTowerDestroyed);
        }
    }


    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Transform target = FindNearestEnemy();
            if (target != null)
            {
                FireAt(target);
                fireCooldown = 1f / fireRate;
            }
        }
    }


    private Transform FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Transform best = null;
        float bestDist = Mathf.Infinity;
        Vector3 p = transform.position;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(p, e.transform.position);
            if (d < range && d < bestDist)
            {
                bestDist = d;
                best = e.transform;
            }
        }
        return best;
    }


    private void FireAt(Transform target)
    {
        if (projectilePrefab == null || firePoint == null) return;
        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.Init(target, damage);
    }


    private void OnTowerDestroyed()
    {
        Debug.Log("Tower destroyed — Game Over");
        FindObjectOfType<GameManager>()?.GameOver();
    }
}