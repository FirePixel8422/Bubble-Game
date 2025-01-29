using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    private float[] _spawnPointsCooldown;

    [SerializeField] private float spawnPointCooldown;

    [SerializeField] private float playerHeight;

    public NetworkObject playerPrefab;


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

        print(ownerClientId);

        NetworkObject playerNetwork = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, ownerClientId, true, false, false, spawnPoints[r].position, spawnPoints[r].rotation);
       

        //keep feet on ground with ray
        if (Physics.Raycast(transform.position + Vector3.up * 3, Vector3.down, out RaycastHit hit, 100))
        {
            playerNetwork.transform.position = new Vector3(spawnPoints[r].position.x, hit.point.y + playerHeight, spawnPoints[r].position.z);
        }

        SetupPlayer_ClientRPC(ownerClientId, playerNetwork.NetworkObjectId);
    }


    [ClientRpc(RequireOwnership = false)]
    private void SetupPlayer_ClientRPC(ulong ownerClientId, ulong networkObjectId)
    {
        NetworkObject playerNetwork = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];

        //my player only
        if (ownerClientId == NetworkManager.LocalClientId)
        {
            Camera cam = playerNetwork.GetComponentInChildren<Camera>(true);

            cam.enabled = true;

            cam.GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            //every other player
            foreach (Collider coll in playerNetwork.GetComponentsInChildren<Collider>(true))
            {
                coll.enabled = false;
            }
        }
    }
}
