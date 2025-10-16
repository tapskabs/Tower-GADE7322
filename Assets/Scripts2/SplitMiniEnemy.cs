using UnityEngine;
using System.Collections;

public class SplitMiniEnemy : Enemy
{
    [Header("Mini Behaviour Settings")]
    public float attackRange = 1.8f;        // how close to defenders they attack
    public float attackInterval = 1.2f;     // time between attacks
    private float attackTimer = 0f;

    private float damageMultiplier = 1f;
    private bool isSelfDestructing = false;

    private IDamageableDefender currentDefenderTarget;

    public void SetDamageMultiplier(float mult)
    {
        damageMultiplier = Mathf.Max(0.1f, mult);
    }

    public void StartSelfDestruct(float lifetime)
    {
        if (!isSelfDestructing)
            StartCoroutine(SelfDestructTimer(lifetime));
    }

    private IEnumerator SelfDestructTimer(float lifetime)
    {
        isSelfDestructing = true;
        yield return new WaitForSeconds(lifetime);

        if (this != null)
            ReceiveDamage(maxHealth);
    }

    protected override void Update()
    {
        base.Update(); // keeps movement & tower logic

        attackTimer += Time.deltaTime;

        // Detect closest defender in range
        DetectNearbyDefender();

        if (currentDefenderTarget != null && attackTimer >= attackInterval)
        {
            AttackDefender();
            attackTimer = 0f;
        }
    }

    private void DetectNearbyDefender()
    {
        currentDefenderTarget = null;
        float closestDist = Mathf.Infinity;

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Defender")) continue;

            IDamageableDefender dmgDef = hit.GetComponent<IDamageableDefender>();
            if (dmgDef == null) continue;

            float dist = Vector3.Distance(transform.position, dmgDef.GetPosition());
            if (dist < closestDist)
            {
                closestDist = dist;
                currentDefenderTarget = dmgDef;
            }
        }
    }

    private void AttackDefender()
    {
        if (currentDefenderTarget == null) return;

        int dealtDamage = Mathf.RoundToInt(damage * damageMultiplier);
        currentDefenderTarget.ReceiveDamage(dealtDamage);
    }
}
