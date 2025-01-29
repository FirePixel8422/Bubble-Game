using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;


public class PlayerLobbyMenu : NetworkBehaviour
{
    public static PlayerLobbyMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        ClientManager.OnClientDisconnectedCallback += OnClientDisconnected_OnServer;
    }



    [SerializeField] private TextMeshProUGUI[] playerNameField;
    [SerializeField] private GameObject[] kickButtonObjs;

    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject invisibleScreenCover;


    private FixedString32Bytes[] _savedFixedPlayerNames;

#if UNITY_EDITOR
    public string[] names;
#endif


    public override void OnNetworkSpawn()
    {
        _savedFixedPlayerNames = new FixedString32Bytes[4];

        Invoke(nameof(RecieveLocalClientGameId), 0.5f);
    }


    private void RecieveLocalClientGameId()
    {
        string userName = AuthenticationService.Instance.PlayerInfo.Username;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (string.IsNullOrEmpty(userName))
        {
            string[] funnyNames = new string[]
            {
                "Retard",
                "PaarseBlobvis",
                "JohnDoe",
                "WillowWilson",
                "BijnaMichael",
                "Yi-Long-Ma",
                "Loading4Ever",
                "OutOfNames",
                "WhyIsThisHere",
                "TheFrenchLikeBaguette",
                "Moe-Lester"
            };

            int r = Random.Range(0, funnyNames.Length);
            userName = funnyNames[r] + NetworkManager.LocalClientId.ToString();
        }
#endif

        if (IsServer)
        {
            startGameButton.SetActive(true);
        }

        ClientManager.SetLocalUserName(userName);

        int localClientGameId = ClientManager.GetClientGameIdFromNetworkId(NetworkManager.LocalClientId);

        AddPlayer_ServerRPC(new FixedString32Bytes(userName), localClientGameId);
    }




    public async void KickPlayerOrLeaveAsync(int gameId)
    {
        if (IsServer)
        {
            if (gameId == 0)
            {
                ClientManager.Instance.DisconnectAllClients_ServerRPC();

                //terminate lobby and shutdown network.
                await LobbyManager.DeleteLobbyAsync();

                NetworkManager.Shutdown();
            }
            else
            {
                //disconect client
                ClientManager.Instance.DisconnectClient_ServerRPC(gameId);
            }
        }
        else
        {
            //diconnect self
            ClientManager.Instance.DisconnectClient_ServerRPC(gameId);
        }
    }

    public async void StartMatchAsync()
    {
        invisibleScreenCover.SetActive(true);

        await LobbyManager.SetLobbyLockStateAsync(true);

        SceneManager.LoadSceneOnNetwork("Patrick");
    }


    private void OnClientDisconnected_OnServer(ulong clientNetworkId, int clientGameId, int newPlayerCount)
    {
        print(clientGameId + " left, " + newPlayerCount + " Player left");

        for (int i = clientGameId; i < newPlayerCount; i++)
        {
            //move down all the networkIds in the array by 1.
            _savedFixedPlayerNames[i] = _savedFixedPlayerNames[i + 1];
        }
        _savedFixedPlayerNames[newPlayerCount] = "";

        SyncPlayerNames_ClientRPC(_savedFixedPlayerNames, newPlayerCount);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddPlayer_ServerRPC(FixedString32Bytes fixedPlayerName, int clientGameId)
    {
        playerNameField[clientGameId].text = fixedPlayerName.ToString();

        _savedFixedPlayerNames[clientGameId] = fixedPlayerName;


        int playerCount = NetworkManager.ConnectedClientsIds.Count;

        SyncPlayerNames_ClientRPC(_savedFixedPlayerNames, playerCount);
    }


    [ClientRpc(RequireOwnership = false)]
    public void SyncPlayerNames_ClientRPC(FixedString32Bytes[] fixedPlayerNames, int playerCount)
    {
        string targetUserName;

        for (int i = 0; i < playerCount; i++)
        {
            targetUserName = fixedPlayerNames[i].ToString();


            playerNameField[i].transform.parent.gameObject.SetActive(true);

            if (targetUserName == ClientManager.LocalUserName)
            {
                playerNameField[i].text = targetUserName + " (You)";

                kickButtonObjs[i].SetActive(true);
            }
            else
            {
                playerNameField[i].text = targetUserName;
            }

            if (IsServer)
            {
                kickButtonObjs[i].SetActive(true);
            }
        }

        for (int i = 3; i >= playerCount ; i--)
        {
            playerNameField[i].transform.parent.gameObject.SetActive(false);
            kickButtonObjs[i].SetActive(false);
        }
    }


    public override void OnDestroy()
    {
        ClientManager.OnClientDisconnectedCallback -= OnClientDisconnected_OnServer;
    }


#if UNITY_EDITOR
    private void Update()
    {
        names = new string[_savedFixedPlayerNames.Length];

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = _savedFixedPlayerNames[i].ToString();
        }
    }
#endif
}
