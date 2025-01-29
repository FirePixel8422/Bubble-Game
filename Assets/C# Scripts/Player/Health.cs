using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : MonoBehaviour, IHealable, IDamagable
{
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth;

    public void OnDamaged(int damageTaken)
    {
        health.Value -= damageTaken;
        if (health.Value <= 0)
        {
            print("dead");
            Destroy(gameObject);
        }
    }

    public void OnHeal(int healingFactor)
    {
        var overflow = healingFactor + health.Value - maxHealth;
        if (overflow > 0)
        {
            health.Value += healingFactor - overflow;
        }
        else if (overflow < 0) 
        {
            health.Value = maxHealth;
        }
        print(overflow);
    }
    
}
