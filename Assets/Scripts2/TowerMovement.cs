using UnityEngine;

public class TowerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveRadius = 9f;       // Max distance from center zone
    public float moveSpeed = 1.2f;      // Movement speed
    public float retargetInterval = 2f; // How often to pick a new direction

    private Vector3 spawnPosition;
    private Vector3 targetPosition;
    private float retargetTimer;

    // Optional: shared "wander zone" — encourages towers to cross paths
    private static Vector3 globalCenter;
    private static bool centerSet = false;

    void Start()
    {
        spawnPosition = transform.position;

        // Set a shared center point (first tower sets it)
        if (!centerSet)
        {
            globalCenter = spawnPosition;
            centerSet = true;
        }

        SetNewTarget();
        retargetTimer = retargetInterval;
    }

    void Update()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            SetNewTarget();
            retargetTimer = retargetInterval + Random.Range(-0.5f, 0.5f);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void SetNewTarget()
    {
        // Random chance: move around your own area OR toward center
        bool moveTowardCenter = Random.value < 0.5f;

        Vector3 basePoint = moveTowardCenter ? globalCenter : spawnPosition;

        // Create new wander target around that base point
        Vector2 randomOffset = Random.insideUnitCircle * moveRadius;
        targetPosition = basePoint + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
