using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 5f;
    private Enemy target;
    private int damage;

    public void Launch(Enemy t, int dmg)
    {
        target = t;
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null) { Destroy(gameObject); return; }
        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, target.transform.position) < 0.6f)
        {
            target.ReceiveDamage(damage);
            Destroy(gameObject);
        }
    }
}
