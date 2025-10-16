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

    // made protected so subclasses can read/modify them
    protected float currentSpeed;
    protected int currentHealth;
    protected float attackTimer = 0f;
    protected bool reachedTower = false;

    // route and towerTarget need to be accessible to subclasses (e.g. SplitterEnemy)
    protected Vector3[] route;
    protected int currentIndex = 0;

    protected Tower towerTarget;
    //protected Defender currentDefenderTarget;
    private ProceduralMap terrain; // terrain can remain private if not needed by subclasses

    [Header("UI")]
    public GameObject healthBarPrefab;
    protected EnemyHealthBar healthBar; // protected so subclasses can destroy or update it

    // 🧊 Status Effects
    protected bool isSlowed = false;
    protected Coroutine slowRoutine;
    protected Coroutine poisonRoutine;
    private bool initialized = false;
    private DefenderBase currentDefenderBaseTarget;
    private IDamageableDefender currentDefenderTarget;
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

        initialized = true; // ✅ mark as ready

        // health bar spawn moved here from Start()
        if (healthBarPrefab != null && healthBar == null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false);
            healthBar = hb.GetComponent<EnemyHealthBar>();
            healthBar?.SetHealth(currentHealth, maxHealth);
        }
    }

    protected virtual void Update()
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
                // smooth height interpolation to avoid jitter and clipping
                newPos.y = Mathf.Lerp(transform.position.y, groundY + 0.2f, 10f * Time.deltaTime);
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
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Defender"))
            {
                IDamageableDefender dmgDef = hit.GetComponent<IDamageableDefender>();
                if (dmgDef != null)
                {
                    float dist = Vector3.Distance(transform.position, dmgDef.GetPosition());
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        currentDefenderTarget = dmgDef;
                    }
                }
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

    //Damage
    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
        healthBar?.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    //Apply Slow
    public virtual void ApplySlow(float slowFactor, float duration)
    {
        if (slowRoutine != null) StopCoroutine(slowRoutine);
        slowRoutine = StartCoroutine(SlowEffect(slowFactor, duration));
    }

    protected virtual IEnumerator SlowEffect(float factor, float duration)
    {
        isSlowed = true;
        currentSpeed = baseSpeed * factor;

        yield return new WaitForSeconds(duration);

        currentSpeed = baseSpeed;
        isSlowed = false;
    }

    //Apply Poison
    public virtual void ApplyPoison(int tickDamage, float duration, float tickRate)
    {
        if (poisonRoutine != null) StopCoroutine(poisonRoutine);
        poisonRoutine = StartCoroutine(PoisonEffect(tickDamage, duration, tickRate));
    }

    protected virtual IEnumerator PoisonEffect(int dmg, float duration, float tickRate)
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
    protected virtual void Die()
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
