using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Mathematics;

public class LobbyUIMananager : MonoBehaviour
{
    private LobbyUIPanel[] lobbyUISlots;



    private void Start()
    {
        lobbyUISlots = GetComponentsInChildren<LobbyUIPanel>(true);
    }


    public void CreateLobbyUI(List<Lobby> lobbies)
    {
        int toUpdatePanelCount = math.min(lobbies.Count, lobbyUISlots.Length);

        for (int i = 0; i < toUpdatePanelCount; i++)
        {
            lobbyUISlots[i].UpdatePanel(lobbies[i]);
        }

        for (int i = toUpdatePanelCount; i > lobbyUISlots.Length; i++)
        {
            lobbyUISlots[i].Disable();
        }
    }
}



