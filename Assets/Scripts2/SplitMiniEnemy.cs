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
        base.Update();

        attackTimer += Time.deltaTime;

        // Try to attack any defender within range while moving
        if (attackTimer >= attackInterval)
        {
            Defender target = FindClosestDefender();
            if (target != null)
            {
                int dealtDamage = Mathf.RoundToInt(damage * damageMultiplier);
                target.ReceiveDamage(dealtDamage);
            }

            attackTimer = 0f;
        }

        // Keep health bar following
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }

    private Defender FindClosestDefender()
    {
        Defender[] defenders = GameObject.FindObjectsOfType<Defender>();
        Defender closest = null;
        float minDist = Mathf.Infinity;

        foreach (Defender d in defenders)
        {
            float dist = Vector3.Distance(transform.position, d.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                closest = d;
            }
        }

        return closest;
    }



}
