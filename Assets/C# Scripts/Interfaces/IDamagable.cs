using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable 
{
    public void OnDamaged(int damageTaken, GameObject owner);
}
