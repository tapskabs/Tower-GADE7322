using UnityEngine;

public class Defender : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1.5f;
    public float damage = 6f;
    public Projectile projectilePrefab;
    public Transform firePoint;


    private float fireCooldown;


    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Transform t = FindNearestEnemy();
            if (t != null)
            {
                var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                proj.Init(t, damage);
                fireCooldown = 1f / fireRate;
            }
        }
    }


    private Transform FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Transform best = null; float bestDist = Mathf.Infinity; Vector3 p = transform.position;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(p, e.transform.position);
            if (d < range && d < bestDist) { best = e.transform; bestDist = d; }
        }
        return best;
    }
}