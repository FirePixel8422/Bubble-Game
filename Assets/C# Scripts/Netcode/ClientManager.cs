using System;
using Unity.Netcode;
using UnityEngine;



public class ClientManager : NetworkBehaviour
{
    private static NetworkVariable<PlayerIdDataArray> _playerIdDataArray = new NetworkVariable<PlayerIdDataArray>(new PlayerIdDataArray(4));


    public static ulong GetClientNetworkIdFromGameId(int gameId) => _playerIdDataArray.Value.GetPlayerNetworkId(gameId);


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



    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        _playerIdDataArray.OnValueChanged += (PlayerIdDataArray before, PlayerIdDataArray after) =>
        {
            LocalClientGameId = after.GetPlayerGameId(NetworkManager.LocalClientId);
        };
    }

    private void OnClientConnected(ulong clientId)
    {
        OnClientConnectedCallback?.Invoke(clientId, _playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClientsIds.Count);


        PlayerIdDataArray updatedDataArray = _playerIdDataArray.Value;

        updatedDataArray.AddPlayer(clientId);

        _playerIdDataArray.Value = updatedDataArray;
    }

    private void OnClientDisconnected(ulong clientId)
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


#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        debug = _playerIdDataArray.Value;
    }
#endif
}