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

        // Cleanly die after lifespan
        if (this != null)
            ReceiveDamage(maxHealth);
    }

    protected override void Update()
    {
        base.Update(); // keep movement and other logic from Enemy

        attackTimer += Time.deltaTime;

        // Detect defenders in range
        DetectNearbyDefender();

        if (currentDefenderTarget != null && attackTimer >= attackInterval)
        {
            AttackDefender();
            attackTimer = 0f;
        }
    }

    // Detect any defender (DefenderBase or legacy Defender) within attackRange
    private void DetectNearbyDefender()
    {
        currentDefenderTarget = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hit in hits)
        {
            DefenderBase db = hit.GetComponent<DefenderBase>();
            if (db != null)
            {
                currentDefenderTarget = hit.GetComponent<Defender>(); // keep legacy reference
                break;
            }

            Defender d = hit.GetComponent<Defender>();
            if (d != null)
            {
                currentDefenderTarget = d;
                break;
            }
        }
    }

    // Attack the detected defender
    private void AttackDefender()
    {
        if (currentDefenderTarget == null) return;

        int dealtDamage = Mathf.RoundToInt(damage * damageMultiplier);

        // Prefer DefenderBase (supports Slow/Poison)
        DefenderBase baseDef = currentDefenderTarget.GetComponent<DefenderBase>();
        if (baseDef != null)
        {
            baseDef.ReceiveDamage(dealtDamage);
        }
        else
        {
            // Legacy Defender fallback
            currentDefenderTarget.ReceiveDamage(dealtDamage);
        }
    }

}
