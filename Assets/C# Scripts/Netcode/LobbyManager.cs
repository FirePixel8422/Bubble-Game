using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public static class LobbyManager
{
    public static Lobby CurrentLobby { get; private set; }
    public static string LobbyId => CurrentLobby.Id;



    //Set the lobby reference for host and clients here and start heartbeat coroutine if called from the host
    public static async void SetLobbyData(Lobby lobby, bool calledFromHost = false)
    {
        CurrentLobby = lobby;

        if (calledFromHost)
        {
            NetworkManager.Singleton.StartCoroutine(HeartbeatLobbyCoroutine(CurrentLobby.Id, 25));
        }

        await FileManager.SaveInfo(new ValueWrapper<string>(LobbyId), "RejoinData.json");
    }


    public async static Task DeleteLobbyAsync()
    {
        await Lobbies.Instance.DeleteLobbyAsync(LobbyId);
    }

    public static async Task SetLobbyLockStateAsync(bool isLocked)
    {
        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
        {
            IsLocked = isLocked,
        };

        try
        {
            // Update the lobby with the new field value
            CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating lobby: {e.Message}");
        }
    }



    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}
