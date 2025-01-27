using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIPanel : MonoBehaviour
{
    private GameObject mainUI;

    private Button button;

    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI gameModeName;
    [SerializeField] private TextMeshProUGUI amountOfPlayersInLobby;
    
    private string lobbyId;



    private void Start()
    {
        mainUI = transform.GetChild(0).gameObject;

        button = GetComponentInChildren<Button>(true);
    }

    public void UpdatePanel(Lobby lobby)
    {
        mainUI.SetActive(true);

        lobbyName.text = lobby.Name;
        gameModeName.text = lobby.Data["gameMode"].Value;

        lobbyId = lobby.Id;

        bool full = lobby.AvailableSlots == 0;

        amountOfPlayersInLobby.text = (lobby.MaxPlayers - lobby.AvailableSlots).ToString() + "/" + lobby.MaxPlayers.ToString() + (full ? "Full!" : "");

        button.enabled = !full;
    }

    public void Disable()
    {
        mainUI.SetActive(false);
    }



    public void JoinLobby()
    {
        MatchMaker.Instance.JoinLobbyById(lobbyId);
    }
}