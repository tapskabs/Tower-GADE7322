using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Defender currentDefenderTarget;
    public float speed = 3f;
    public int maxHealth = 40;
    public int damage = 10;
    public float attackRate = 1f;
    public float reachRadius = 1.2f;

    private int currentHealth;
    private Vector3[] route;
    private int index = 0;
    private float attackTimer = 0f;
    private bool isAtTarget = false;
    private Tower towerTarget;

    public void InitRoute(Vector3[] waypoints, Tower tower)
    {
        route = waypoints;
        towerTarget = tower;
        index = 0;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (route == null || route.Length == 0) return;

        if (!isAtTarget)
        {
            Vector3 target = route[index];
            Vector3 dir = (target - transform.position);
            dir.y = 0;
            if (dir.magnitude < 0.2f)
            {
                index++;
                if (index >= route.Length)
                {
                    // reached tower area
                    isAtTarget = true;
                    return;
                }
            }
            else
            {
                Vector3 move = dir.normalized * speed * Time.deltaTime;
                transform.position += move;
                transform.forward = Vector3.Slerp(transform.forward, dir.normalized, 0.2f);
            }
        }
        else
        {
            // ✅ Check for defender in attack radius
            Collider[] hits = Physics.OverlapSphere(transform.position, reachRadius);
            foreach (var h in hits)
            {
                Defender d = h.GetComponent<Defender>();
                if (d != null)
                {
                    currentDefenderTarget = d;
                    break;
                }
            }

            if (currentDefenderTarget != null)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackRate)
                {
                    currentDefenderTarget.ReceiveDamage(damage);
                    attackTimer = 0f;
                }
            }
            else
            {
                // ✅ Fallback to tower attack
                if (towerTarget != null)
                {
                    float dist = Vector3.Distance(transform.position, towerTarget.transform.position);
                    if (dist <= reachRadius)
                    {
                        attackTimer += Time.deltaTime;
                        if (attackTimer >= attackRate)
                        {
                            towerTarget.TakeDamage(damage);
                            attackTimer = 0f;
                        }
                    }
                    else
                    {
                        // move closer
                        Vector3 move = (towerTarget.transform.position - transform.position).normalized * speed * Time.deltaTime;
                        transform.position += move;
                    }
                }
            }
        }
    }

    public void ReceiveDamage(int d)
    {
        currentHealth -= d;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
        // optionally reward player resources here by calling GameManager
        GameManager.Instance?.AddResources(10);
    }
}
