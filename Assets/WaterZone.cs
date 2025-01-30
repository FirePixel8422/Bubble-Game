using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaterZone : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;


        if (other.gameObject.TryGetComponent(out Health h))
        {
            h.OnDamaged(1000, null);
        }
    }
}
