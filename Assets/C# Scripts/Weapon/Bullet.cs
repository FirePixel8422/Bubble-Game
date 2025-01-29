using UnityEngine;
using Unity.Netcode;
using System.Collections;


public class Bullet : NetworkBehaviour
{
    public GameObject owner { get; private set; }
    private float _speed;
    public int damage { get; private set; }
    public void SetVariables(GameObject player, int damage, float speed)
    {
        owner = player;
        this.damage = damage;
        _speed = speed;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        StartCoroutine(Delay(10));
    }

    private IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);

        NetworkObject.Despawn(true);
    }


    private void Update()
    {
        if (!IsServer) return;

        transform.Translate(Vector3.forward * (Time.deltaTime * _speed));
        SyncMovement_ClientRPC(transform.position, transform.rotation);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncMovement_ClientRPC(Vector3 position, Quaternion rotation)
    {
        //bullets get updated through server, updating these values on the server after sending them would flicker the bullet back and fourth.
        if (IsServer) return;

        transform.position = position;
        transform.rotation = rotation;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.gameObject == owner || other.transform.root == owner.transform) return;
        if (other.TryGetComponent(out IDamagable damagable))
        {
            damagable.OnDamaged(damage, owner);
        }

        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}
