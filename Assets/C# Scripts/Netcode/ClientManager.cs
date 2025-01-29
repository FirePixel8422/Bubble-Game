using JetBrains.Annotations;
using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;



public class ClientManager : NetworkBehaviour
{
    public static ClientManager Instance;

    private static NetworkVariable<PlayerIdDataArray> _playerIdDataArray;

    private void Awake()
    {
        Instance = this;

        _playerIdDataArray = new NetworkVariable<PlayerIdDataArray>(new PlayerIdDataArray(4));
    }




    [Tooltip("Turn GameId into NetworkId")]
    public static ulong GetClientNetworkIdFromGameId(int gameId) => _playerIdDataArray.Value.GetPlayerNetworkId(gameId);

    [Tooltip("Turn NetworkId into GameId")]
    public static int GetClientGameIdFromNetworkId(ulong networkId) => _playerIdDataArray.Value.GetPlayerGameId(networkId);



    [Tooltip("After NetworkManager.ClientDisconnected, before updating ClientManager gameId logic")]
    public static Action<ulong, int, int> OnClientConnectedCallback;

    [Tooltip("After NetworkManager.OnClientDisconnected, before updating ClientManager gameId logic")]
    public static Action<ulong, int, int> OnClientDisconnectedCallback;


    [Tooltip("Local Client gameId, the number equal to the playerCount when this client joined the lobby")]
    public static int LocalClientGameId { get; private set; }


    [Tooltip("Local Client userName, value is set after PlayerDisplayManager's OnNetworkSpawn")]
    public static string LocalUserName { get; private set; }


    public static void SetLocalUserName(string newname)
    {
        LocalUserName = newname;
    }




#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public PlayerIdDataArray debug;
#endif



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected_OnServer;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnServer;
        }

        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnClient;

        _playerIdDataArray.OnValueChanged += (PlayerIdDataArray before, PlayerIdDataArray after) =>
        {
            LocalClientGameId = after.GetPlayerGameId(NetworkManager.LocalClientId);
        };
    }

    private void OnClientConnected_OnServer(ulong clientId)
    {
        OnClientConnectedCallback?.Invoke(clientId, _playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClientsIds.Count);

        PlayerIdDataArray updatedDataArray = _playerIdDataArray.Value;

        updatedDataArray.AddPlayer(clientId);

        _playerIdDataArray.Value = updatedDataArray;
    }

    private void OnClientDisconnected_OnServer(ulong clientId)
    {
        //if the diconnecting client is the server dont update data, the server is shut down anyways.
        if (clientId == 0)
        {
            return;
        }

        OnClientDisconnectedCallback?.Invoke(clientId, _playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClientsIds.Count);


        PlayerIdDataArray updatedDataArray = _playerIdDataArray.Value;

        updatedDataArray.RemovePlayer(clientId);

        _playerIdDataArray.Value = updatedDataArray;
    }

    private void OnClientDisconnected_OnClient(ulong clientId)
    {
        //call function only on client who disconnected
        if (clientId != NetworkManager.LocalClientId)
        {
            return;
        }


        Destroy(gameObject);
        Destroy(MatchMaker.Instance.gameObject);


        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected_OnServer;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnServer;
        }

        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnClient;

        //when kicked from the server, load this scene
        SceneManager.LoadScene("Setup Network");
    }




    #region Kick Client and kill Server code

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient_ServerRPC(ulong clientNetworkId)
    {
        GetKicked_ClientRPC(clientNetworkId);

        NetworkManager.DisconnectClient(clientNetworkId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient_ServerRPC(int clientGameId)
    {
        ulong clientNetworkId = GetClientNetworkIdFromGameId(clientGameId);

        GetKicked_ClientRPC(clientNetworkId);

        NetworkManager.DisconnectClient(clientNetworkId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectAllClients_ServerRPC()
    {
        KickAllClients_ClientRPC();
    }


    [ClientRpc(RequireOwnership = false)]
    private void GetKicked_ClientRPC(ulong clientNetworkId)
    {
        //destroy the rejoin reference on the kicked client
        if (clientNetworkId == NetworkManager.LocalClientId)
        {
            FileManager.DeleteFile("RejoinData.json");
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void KickAllClients_ClientRPC()
    {
        //destroy the rejoin reference on the kicked client
        FileManager.DeleteFile("RejoinData.json");
    }

    #endregion



#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        debug = _playerIdDataArray.Value;
    }
#endif
}