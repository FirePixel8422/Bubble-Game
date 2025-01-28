using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;



public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker Instance;
    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public string _lobbyId;

    [SerializeField] private GameObject invisibleScreenCover;



    public async void CreateLobbyAsync()
    {
        invisibleScreenCover.SetActive(true);
        int maxPlayers = 4;

        try
        {
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers - 1, "europe-west4");
            RelayHostData _hostData = new RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            _hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);


            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                IsLocked = false,

                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "joinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: _hostData.JoinCode)
                    },
                },
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Unnamed Lobby", maxPlayers, options);


            _lobbyId = lobby.Id;

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData);

            NetworkManager.StartHost();

            //load next scene
            SceneManager.LoadSceneOnNetwork("Pre-Main Game");
        }
        catch (LobbyServiceException e)
        {
            print(e);

            invisibleScreenCover.SetActive(false);
        }
    }


    public async void AutoJoinLobbyAsync()
    {
        invisibleScreenCover.SetActive(true);

        try
        {
            (bool lobbyFound, List<Lobby> lobbies) = await FindLobbiesAsync();

            if (lobbyFound == false)
            {
                CreateLobbyAsync();

                return;
            }

            string joinCode = lobbies[0].Data["joinCode"].Value;
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);


            RelayJoinData _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.StartClient();
        }
        catch (LobbyServiceException e)
        {
            invisibleScreenCover.SetActive(false);

            print(e);
        }
    }


    public async Task<(bool, List<Lobby>)> FindLobbiesAsync()
    {
        try
        {
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    // Only include open lobbies in the pages
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "-1")
                },
                Order = new List<QueryOrder>
                {
                    // Show the newest lobbies first
                    new QueryOrder(false, QueryOrder.FieldOptions.Created),
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            return (response.Results.Count > 0, response.Results);
        }
        catch (LobbyServiceException e)
        {
            print(e);

            return (false, null);
        }
    }


    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);

            //localPlayerId = lobby.Players[^1].Id;

            string joinCode = lobby.Data["joinCode"].Value;
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            RelayJoinData _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.StartClient();
        }
        catch (LobbyServiceException e)
        {
            print(e);
            throw;
        }
    }



    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}