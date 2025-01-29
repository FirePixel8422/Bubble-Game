using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour, IHealable, IDamagable
{
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth;



    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        health.OnValueChanged += (int prevHealth, int newHealth) =>
        {
            HUDUpdater.Instance.UpdateHealth(Math.Clamp(newHealth, 0, maxHealth));

            if (newHealth <= 0)
            {
                PlayerSpawner.Instance.StartCoroutine(RespawnDelay());
            }
        };
    }

    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(PlayerSpawner.Instance.respawnTime - 0.25f);

        HUDUpdater.Instance.UpdateHealth(100);
    }

    private bool dead;

    public void OnDamaged(int damageTaken, GameObject owner)
    {
        health.Value -= damageTaken;
        if (health.Value <= 0)
        {
            if (dead == true)
            {
                return;
            }

            dead = true;

            PlayerSpawner.Instance.StartCoroutine(PlayerSpawner.Instance.KillClientOnServer(NetworkObject));

            HUDUpdater.Instance.AddKill_ServerRPC(owner.GetComponent<NetworkObject>().OwnerClientId);
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
    }
    
}
