using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed = 3.5f;
    [Header("Combat")] public float damage = 5f; public float attackInterval = 1.2f;


    private List<Transform> path;
    private int currentIndex = 0;
    private float attackTimer = 0f;


    public void InitPath(List<Transform> waypoints)
    {
        path = waypoints;
        currentIndex = 0;
    }


    private void Update()
    {
        if (path == null || path.Count == 0) return;


        // Move along path
        Vector3 target = path[currentIndex].position;
        Vector3 dir = (target - transform.position);
        float step = moveSpeed * Time.deltaTime;
        if (dir.magnitude <= step)
        {
            currentIndex++;
            if (currentIndex >= path.Count)
            {
                // Reached tower node — attempt to attack tower
                TryAttackTower();
                return;
            }
        }
        else
        {
            transform.position += dir.normalized * step;
            transform.forward = dir.normalized;
        }


        attackTimer -= Time.deltaTime;
    }


    private void TryAttackTower()
    {
        var tower = FindObjectOfType<Tower>();
        if (tower == null) { Destroy(gameObject); return; }
        if (attackTimer <= 0f)
        {
            var h = tower.GetComponent<Health>();
            if (h != null) h.TakeDamage(damage);
            attackTimer = attackInterval;
        }
    }
}