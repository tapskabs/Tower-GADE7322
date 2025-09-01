using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public float maxLife = 3f;
    private Transform target;


    public void Init(Transform target, float damage)
    {
        this.target = target;
        this.damage = damage;
        Destroy(gameObject, maxLife);
    }


    private void Update()
    {
        if (target == null) { Destroy(gameObject); return; }
        Vector3 dir = (target.position - transform.position);
        float distThisFrame = speed * Time.deltaTime;
        if (dir.magnitude <= distThisFrame)
        {
            Hit();
            return;
        }
        transform.position += dir.normalized * distThisFrame;
        transform.forward = dir.normalized;
    }


    private void Hit()
    {
        var h = target.GetComponent<Health>();
        if (h != null) h.TakeDamage(damage);
        Destroy(gameObject);
    }
}