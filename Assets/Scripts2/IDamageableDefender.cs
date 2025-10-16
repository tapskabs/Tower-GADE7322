using UnityEngine;

public interface IDamageableDefender
{
    void ReceiveDamage(int dmg);
    Vector3 GetPosition();
}
