using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float baseSpeed = 3f;
    public int maxHealth = 40;
    public int damage = 10;
    public float attackRate = 1f;
    public float reachRadius = 1.2f;

    private float currentSpeed;
    private int currentHealth;
    private float attackTimer = 0f;
    private bool reachedTower = false;

    private Vector3[] route;
    private int currentIndex = 0;

    private Tower towerTarget;
    private Defender currentDefenderTarget;
    private ProceduralMap terrain;

    [Header("UI")]
    public GameObject healthBarPrefab;
    private EnemyHealthBar healthBar;

    // 🧊 Status Effects
    private bool isSlowed = false;
    private Coroutine slowRoutine;
    private Coroutine poisonRoutine;

    protected virtual void Start()
    {
        currentSpeed = baseSpeed;
    }

    public void InitRoute(Vector3[] waypoints, Tower tower)
    {
        route = waypoints;
        towerTarget = tower;
        currentIndex = 0;
        currentHealth = maxHealth;

        terrain = FindObjectOfType<ProceduralMap>();

        if (healthBarPrefab != null && healthBar == null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false);
            healthBar = hb.GetComponent<EnemyHealthBar>();
            healthBar?.SetHealth(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (route == null || route.Length == 0) return;

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

        UpdateHealthBarPosition();
    }

    //  Movement
    private void MoveAlongPath()
    {
        if (currentIndex >= route.Length) return;

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
            Vector3 move = dir.normalized * currentSpeed * Time.deltaTime;
            Vector3 newPos = transform.position + move;

            if (terrain != null)
            {
                float groundY = terrain.GetHeightAt(newPos.x, newPos.z);
                newPos.y = groundY + 0.1f;
            }

            transform.position = newPos;
            transform.forward = Vector3.Slerp(transform.forward, dir.normalized, 0.2f);
        }
    }

    //  Combat
    private void DetectNearbyDefender()
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

    private void AttackDefender()
    {
        if (currentDefenderTarget == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            currentDefenderTarget.ReceiveDamage(damage);
            attackTimer = 0f;
        }
    }

    private void AttackTower()
    {
        if (towerTarget == null) return;

        attackTimer += Time.deltaTime;
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
            Vector3 moveDir = (towerTarget.transform.position - transform.position).normalized;
            Vector3 newPos = transform.position + moveDir * currentSpeed * Time.deltaTime;

            if (terrain != null)
            {
                float groundY = terrain.GetHeightAt(newPos.x, newPos.z);
                newPos.y = groundY + 0.1f;
            }

            transform.position = newPos;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, 0.2f);
        }
    }

    // ❤️ Damage
    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
        healthBar?.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    //  Apply Slow
    public virtual void ApplySlow(float slowFactor, float duration)
    {
        if (slowRoutine != null) StopCoroutine(slowRoutine);
        slowRoutine = StartCoroutine(SlowEffect(slowFactor, duration));
    }

    private IEnumerator SlowEffect(float factor, float duration)
    {
        isSlowed = true;
        currentSpeed = baseSpeed * factor;

        yield return new WaitForSeconds(duration);

        currentSpeed = baseSpeed;
        isSlowed = false;
    }

    //  Apply Poison
    public virtual void ApplyPoison(int tickDamage, float duration, float tickRate)
    {
        if (poisonRoutine != null) StopCoroutine(poisonRoutine);
        poisonRoutine = StartCoroutine(PoisonEffect(tickDamage, duration, tickRate));
    }

    private IEnumerator PoisonEffect(int dmg, float duration, float tickRate)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            ReceiveDamage(dmg);
            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }
    }

    //Death
    private void Die()
    {
        GameManager.Instance?.AddResources(10);

        if (healthBar != null)
            Destroy(healthBar.gameObject);

        Destroy(gameObject);
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBar == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        healthBar.transform.position = screenPos;
    }
}
