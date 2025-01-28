using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerDisplayManager : NetworkBehaviour
{
    public static PlayerDisplayManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;

        _savedFixedPlayerNames = new FixedString32Bytes[4];

        ClientManager.OnClientDisconnectedCallback += RemovePlayer_OnServer;
    }



    [SerializeField] private TextMeshProUGUI[] playerNameField;
    private FixedString32Bytes[] _savedFixedPlayerNames;





    public override void OnNetworkSpawn()
    {
        string userName = AuthenticationService.Instance.PlayerInfo.Username;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (string.IsNullOrEmpty(userName))
        {
            string[] funnyNames = new string[]
            {
                "Retard123",
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
            userName = funnyNames[r];
        }
#endif

        AddPlayer_ServerRPC(new FixedString32Bytes(userName), NetworkManager.LocalClientId);
    }

    private void RemovePlayer_OnServer(ulong clientNetworkId, int clientGameId, int playerCount)
    {
        print(clientGameId);

        for (int i = clientGameId; i < playerCount; i++)
        {
            //move down all the networkIds in the array by 1.
            _savedFixedPlayerNames[i] = _savedFixedPlayerNames[i + 1];
        }

        SyncPlayerNames_ClientRPC(_savedFixedPlayerNames, playerCount);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddPlayer_ServerRPC(FixedString32Bytes fixedPlayerName, ulong clientGameId)
    {
        playerNameField[clientGameId].text = fixedPlayerName.ToString();

        _savedFixedPlayerNames[clientGameId] = fixedPlayerName;


        int playerCount = NetworkManager.ConnectedClientsIds.Count;

        SyncPlayerNames_ClientRPC(_savedFixedPlayerNames, playerCount);
    }


    [ClientRpc(RequireOwnership = false)]
    public void SyncPlayerNames_ClientRPC(FixedString32Bytes[] fixedPlayerNames, int playerCount)
    {
        for (int i = 0; i < playerCount; i++)
        {
            playerNameField[i].transform.parent.gameObject.SetActive(true);

            playerNameField[i].text = fixedPlayerNames[i].ToString();
        }

        for (int i = 3; i >= playerCount ; i--)
        {
            playerNameField[i].transform.parent.gameObject.SetActive(false);
        }
    }
}
