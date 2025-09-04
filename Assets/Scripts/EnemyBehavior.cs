using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 2f;
    public float attackRate = 1f;

    private Tower tower;
    private NavMeshAgent agent;
    private float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        tower = Object.FindFirstObjectByType<Tower>();
        agent.SetDestination(tower.transform.position);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, tower.transform.position) <= attackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackRate)
            {
                tower.SendMessage("TakeDamage", damage);
                attackTimer = 0;
            }
        }
    }
}
