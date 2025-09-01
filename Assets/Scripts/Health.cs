using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    public float Current { get; private set; }


    public UnityEvent onDeath;
    public UnityEvent<float, float> onDamage; // (current, max)


    private void Awake()
    {
        Current = maxHealth;
        if (onDeath == null) onDeath = new UnityEvent();
        if (onDamage == null) onDamage = new UnityEvent<float, float>();
    }


    public void TakeDamage(float amount)
    {
        if (Current <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        onDamage.Invoke(Current, maxHealth);
        if (Current <= 0)
        {
            onDeath.Invoke();
        }
    }


    public void Heal(float amount)
    {
        if (Current <= 0) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        onDamage.Invoke(Current, maxHealth);
    }
}