using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 3f;
    public int maxHealth = 40;
    public int damage = 10;
    public float attackRate = 1f;
    public float reachRadius = 1.2f;

    private int currentHealth;
    private Vector3[] route;
    private int currentIndex = 0;
    private bool reachedTower = false;
    private float attackTimer = 0f;

    private Tower towerTarget;
    private Defender currentDefenderTarget;

    // Initialize enemy with path and tower reference
    public void InitRoute(Vector3[] waypoints, Tower tower)
    {
        route = waypoints;
        towerTarget = tower;
        currentIndex = 0;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (route == null || route.Length == 0) return;

        if (!reachedTower)
        {
            MoveAlongPath();
        }
        else
        {
            AttackTargets();
        }
    }

    void MoveAlongPath()
    {
        Vector3 targetPos = route[currentIndex];
        Vector3 flatTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        Vector3 dir = flatTarget - transform.position;

        if (dir.magnitude < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= route.Length)
            {
                reachedTower = true;
                return;
            }
        }
        else
        {
            transform.position += dir.normalized * speed * Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, dir.normalized, 0.2f);
        }
    }

    void AttackTargets()
    {
        // Reset current defender target
        currentDefenderTarget = null;

        // Check for nearby defenders
        Collider[] hits = Physics.OverlapSphere(transform.position, reachRadius);
        foreach (var hit in hits)
        {
            Defender d = hit.GetComponent<Defender>();
            if (d != null)
            {
                currentDefenderTarget = d;
                break;
            }
        }

        attackTimer += Time.deltaTime;

        if (currentDefenderTarget != null)
        {
            if (attackTimer >= attackRate)
            {
                currentDefenderTarget.ReceiveDamage(damage);
                attackTimer = 0f;
            }
        }
        else if (towerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, towerTarget.transform.position);
            if (dist <= reachRadius)
            {
                if (attackTimer >= attackRate)
                {
                    towerTarget.TakeDamage(damage);
                    attackTimer = 0f;
                }
            }
            else
            {
                // Move toward tower if not in range
                Vector3 moveDir = (towerTarget.transform.position - transform.position).normalized;
                transform.position += moveDir * speed * Time.deltaTime;
                transform.forward = Vector3.Slerp(transform.forward, moveDir, 0.2f);
            }
        }
    }

    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        GameManager.Instance?.AddResources(10);
        Destroy(gameObject);
    }
}
