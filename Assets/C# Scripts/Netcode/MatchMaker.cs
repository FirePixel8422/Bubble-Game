using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker Instance;
    private void Awake()
    {
        Instance = this;
    }



    public string toLoadScene;

    public string _lobbyId;



    private async void Start()
    {
        await SignInAnonymouslyAsync();

        DontDestroyOnLoad(gameObject);
    }


    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.ActiveSceneSynchronizationEnabled = true;
    }


    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignOut(true);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception ex)
        {
            print(ex);
        }
    }



    public async void CreateLobby()
    {
        int maxPlayers = 4;

        try
        {
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers - 1);
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

                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "joinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: _hostData.JoinCode)
                    },
                },
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            AuthenticationService.Instance.PlayerInfo.Username, new PlayerDataObject(
                                PlayerDataObject.VisibilityOptions.Public)
                        }
                    }
                }
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);

            //localPlayerId = lobby.Players[0].Id;


            _lobbyId = lobby.Id;

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData);

            NetworkManager.Singleton.StartHost();
            NetworkManager.SceneManager.LoadScene(toLoadScene, LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            print(e);
        }
    }


    public async void AutoJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinOptions = new QuickJoinLobbyOptions
            {
                Filter = new List<QueryFilter>
                {
                     //Only include lobbies that have atleast 1 spot left.
                     new QueryFilter(
                         field: QueryFilter.FieldOptions.AvailableSlots,
                         op: QueryFilter.OpOptions.GT,
                         value: "0"),
                },
            };

            await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinOptions);
        }
        catch (LobbyServiceException e)
        {
            print(e);
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

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch(LobbyServiceException e)
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