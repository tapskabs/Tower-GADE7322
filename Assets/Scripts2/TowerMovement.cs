using UnityEngine;

public class TowerMovement : MonoBehaviour
{
    public float moveRadius = 6f;        // How far from spawn point they move
    public float moveSpeed = 1f;         // Speed of movement

    private Vector3 spawnPosition;
    private Vector3 targetPosition;

    void Start()
    {
        spawnPosition = transform.position;
        SetNewTarget();
    }

    void Update()
    {
        // Move toward target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // If reached target, pick a new target
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            SetNewTarget();
    }

    void SetNewTarget()
    {
        // Random point around spawn within radius
        Vector2 randomOffset = Random.insideUnitCircle * moveRadius;
        targetPosition = spawnPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }
}
