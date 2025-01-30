using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }




    [SerializeField] private Transform[] spawnPoints;
    private float[] _spawnPointsCooldown;

    [SerializeField] private float spawnPointCooldown;

    public float respawnTime;

    [SerializeField] private float playerHeight;

    public GameObject playerPrefab;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _spawnPointsCooldown = new float[spawnPoints.Length];

            StartCoroutine(EnableSpawnpointsLoop());
        }

        SpawnPlayer_ServerRPC(NetworkManager.LocalClientId);
    }


    private IEnumerator EnableSpawnpointsLoop()
    {
        while (true)
        {
            yield return null;

            float deltaTime = Time.deltaTime;

            for (int i = 0; i < _spawnPointsCooldown.Length; i++)
            {
                _spawnPointsCooldown[i] -= deltaTime;


                if (_spawnPointsCooldown[i] < 0)
                {
                    _spawnPointsCooldown[i] = 0;
                }
            }
        }
    }



    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer_ServerRPC(ulong ownerClientId)
    {
        int r = Random.Range(0, spawnPoints.Length);

        while (_spawnPointsCooldown[r] > 0)
        {
            r += 1;

            if (r == _spawnPointsCooldown.Length)
            {
                r = 0;
            }
        }

        _spawnPointsCooldown[r] = spawnPointCooldown;

        NetworkObject playerNetwork = Instantiate(playerPrefab, spawnPoints[r].position, spawnPoints[r].rotation).GetComponent<NetworkObject>();
        
        //keep feet on ground with ray
        if (Physics.Raycast(playerNetwork.transform.position + Vector3.up * 3, Vector3.down, out RaycastHit hit, 100))
        {
            playerNetwork.transform.position = new Vector3(spawnPoints[r].position.x, hit.point.y + playerHeight, spawnPoints[r].position.z);
        }

        playerNetwork.SpawnWithOwnership(ownerClientId);


        SetupPlayer_ClientRPC(ownerClientId, playerNetwork.NetworkObjectId);
    }


    [ClientRpc(RequireOwnership = false)]
    private void SetupPlayer_ClientRPC(ulong ownerClientId, ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject playerNetwork))
        {
            //on the local client, activate its local players component
            if (ownerClientId == NetworkManager.LocalClientId)
            {
                playerNetwork.GetComponentInChildren<Camera>(true).enabled = true;
                playerNetwork.GetComponentInChildren<AudioListener>().enabled = true;
                playerNetwork.GetComponent<Rigidbody>().isKinematic = false;

                Collider[] colliders = playerNetwork.GetComponents<Collider>();
                foreach (Collider coll in colliders)
                {
                    if (coll.isTrigger) continue;

                    coll.enabled = true;
                }
            }
        }
        else
        {
            print("networkobject not found");
        }
    }


    public IEnumerator KillClientOnServer(NetworkObject playerNetwork)
    {
        ulong ownerOfKilledPlayer = playerNetwork.OwnerClientId;

        playerNetwork.Despawn(true);

        yield return new WaitForSeconds(respawnTime);

        SpawnPlayer_ServerRPC(ownerOfKilledPlayer);
    }

    public IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(PlayerSpawner.Instance.respawnTime - 0.25f);

        HUDUpdater.Instance.UpdateHealth(100);
    }
}
