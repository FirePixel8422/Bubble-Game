using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour, IHealable, IDamagable
{
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth;
    public int killAmount;

    public void OnDamaged(int damageTaken, GameObject owner)
    {
        health.Value -= damageTaken;
        if (health.Value <= 0)
        {
            print("dead");
            NetworkObject.Despawn(gameObject);
            if (owner.TryGetComponent(out Health h))
            {
                h.killAmount++;
            }
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
