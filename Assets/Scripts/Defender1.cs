using UnityEngine;

public class Defender1 : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float attackRate = 1f;
    public float attackRange = 10f;

    private float attackTimer;

    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target);
                attackTimer = 0;
            }
        }
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float minDist = Mathf.Infinity;
        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                closest = e;
            }
        }
        return closest;
    }

    void Shoot(Enemy target)
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - transform.position).normalized * 10f;
    }
}
