using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int health;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            print("dead");
            Destroy(gameObject);
        }
    }
    
}
