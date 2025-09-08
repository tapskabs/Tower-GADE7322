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

    // Reference to the procedural terrain
    private ProceduralMap terrain;

    [Header("UI")]
    public GameObject healthBarPrefab;
    private EnemyHealthBar healthBar;

    // Initialize enemy with path and tower reference
    public void InitRoute(Vector3[] waypoints, Tower tower)
    {
        route = waypoints;
        towerTarget = tower;
        currentIndex = 0;
        currentHealth = maxHealth;

        // cache terrain once
        terrain = FindObjectOfType<ProceduralMap>();

        // ✅ Spawn health bar above enemy
        if (healthBarPrefab != null && healthBar == null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false); // assumes a World Space Canvas
            healthBar = hb.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                // initialize full health bar
                healthBar.SetHealth(currentHealth, maxHealth);
            }
        }
    }

    void Update()
    {
        if (route == null || route.Length == 0) return;

        // ✅ Always check for defenders
        DetectNearbyDefender();

        if (currentDefenderTarget != null)
        {
            AttackDefender();
        }
        else if (!reachedTower)
        {
            MoveAlongPath();
        }
        else
        {
            AttackTower();
        }

        // ✅ keep health bar above enemy
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }

    void MoveAlongPath()
    {
        Vector3 targetPos = route[currentIndex];
        Vector3 dir = new Vector3(targetPos.x, transform.position.y, targetPos.z) - transform.position;

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
            Vector3 move = dir.normalized * speed * Time.deltaTime;
            Vector3 newPos = transform.position + move;

            // snap Y to terrain
            if (terrain != null)
            {
                float groundY = terrain.GetHeightAt(newPos.x, newPos.z);
                newPos.y = groundY + 0.1f; // small offset so feet don’t clip
            }

            transform.position = newPos;
            transform.forward = Vector3.Slerp(transform.forward, dir.normalized, 0.2f);
        }
    }

    // ✅ Defender detection
    void DetectNearbyDefender()
    {
        currentDefenderTarget = null;
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
    }

    // ✅ Attack defender
    void AttackDefender()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate && currentDefenderTarget != null)
        {
            currentDefenderTarget.ReceiveDamage(damage);
            attackTimer = 0f;
        }
    }

    // ✅ Attack tower if in range
    void AttackTower()
    {
        attackTimer += Time.deltaTime;
        if (towerTarget == null) return;

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
            Vector3 newPos = transform.position + moveDir * speed * Time.deltaTime;

            if (terrain != null)
            {
                float groundY = terrain.GetHeightAt(newPos.x, newPos.z);
                newPos.y = groundY + 0.1f;
            }

            transform.position = newPos;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, 0.2f);
        }
    }

    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;

        // ✅ Update bar properly
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        GameManager.Instance?.AddResources(10);

        // ✅ remove health bar when enemy dies
        if (healthBar != null)
            Destroy(healthBar.gameObject);

        Destroy(gameObject);
    }
}
