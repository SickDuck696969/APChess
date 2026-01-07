using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;

public class HostStarter : MonoBehaviour
{
    public int maxPlayers = 2;
    public Lobby hosting;
    public string joincode;
    public string yourname = null;
    public string yourImage = null;
    public string playerName = "";
    public string playerImage = "";
    public Player player;

    public async void CreateLobby(string name, string roomname)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"[Lobby] Relay join code: {joinCode}");

            var hostPlayer = new LobbyPlayer
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, name) },
                    { "playerImage", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, player.data.profilePicBase64) }
                }
            };

            var options = new CreateLobbyOptions
            {
                Player = hostPlayer,
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                    { "hostName", new DataObject(DataObject.VisibilityOptions.Public, name) },
                    { "hostImage", new DataObject(DataObject.VisibilityOptions.Public, player.data.profilePicBase64) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomname, maxPlayers, options);

            hosting = lobby;
            joincode = joinCode;
            RefreshLobbyInfo();

            Debug.Log($"[Lobby] Created lobby '{lobby.Name}' with ID: {lobby.Id}");

            StartHost(allocation, joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Lobby] Failed to create lobby: {e}");
        }
    }

    public async void RefreshLobbyInfo()
    {
        if (hosting == null)
        {
            Debug.LogWarning("[Host] No lobby exists.");
            return;
        }

        try
        {
            hosting = await LobbyService.Instance.GetLobbyAsync(hosting.Id);

            Debug.Log($"[Host] Lobby '{hosting.Name}' refreshed.");

            string joinCode = hosting.Data["joinCode"].Value;
            string hostName = hosting.Data["hostName"].Value;
            string hostImage = hosting.Data["hostImage"].Value;

            Debug.Log($"[Host] Data -> Host Name: {hostName}, Host Image: {hostImage}, Code: {joinCode}");

            Debug.Log("[Host] Current Players:");

            playerName = "";
            playerImage = "";
            yourImage = null;
            yourname = null;

            foreach (LobbyPlayer p in hosting.Players)
            {
                if (p.Id != AuthenticationService.Instance.PlayerId)
                {
                    playerName = p.Data.TryGetValue("playerName", out var pn) ? pn.Value : "Unknown";
                    playerImage = p.Data.TryGetValue("hostImage", out var pi) ? pn.Value : "Unknown";
                }else if (p.Id == AuthenticationService.Instance.PlayerId)
                {
                    yourname = p.Data.TryGetValue("playerName", out var pn) ? pn.Value : "Unknown";
                    yourImage = p.Data.TryGetValue("hostImage", out var pi) ? pn.Value : "Unknown";
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Host] Failed to refresh lobby: " + e);
        }
    }


    public async void StartHost(Allocation? allocation, string? joinCode)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if(allocation != null && joinCode != null)
        {
            try
            {
                Debug.Log($"[Host] Relay join code: {joinCode}");

                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[Host] Relay setup failed: {e}");
                return;
            }
        }
        else
        {
            transport.SetConnectionData("0.0.0.0", 7777);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientLeft;

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("[Host] Host started.");
        }
        else
        {
            Debug.LogError("[Host] Failed to start host.");
        }
    }

    public async void ShutdownHost()
    {
        Debug.Log("[Host] Shutting down host...");

        try
        {
            if (hosting != null)
            {
                // Only the creator may delete the lobby
                if (hosting.HostId == AuthenticationService.Instance.PlayerId)
                {
                    // Refresh lobby state
                    hosting = await LobbyService.Instance.GetLobbyAsync(hosting.Id);

                    Debug.Log("[Host] Deleting lobby: " + hosting.Name);
                    await LobbyService.Instance.DeleteLobbyAsync(hosting.Id);
                }
                else
                {
                    Debug.Log("[Host] Not the lobby owner — cannot delete.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Host] Failed to delete lobby: " + e);
        }

        hosting = null;

        // Unsubscribe
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientLeft;

        // Shutdown netcode
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[Host] Netcode shutdown complete.");
        }

        // Clear transport settings
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
            transport.SetConnectionData("", 0);
        hosting = null;
        joincode = null;

        Debug.Log("[Host] Shutdown finished.");
    }



    private void OnClientJoined(ulong clientId)
    {
        Debug.Log($"[Host] Client joined | ClientID: {clientId}");
        RefreshLobbyInfo();
    }

    private void OnClientLeft(ulong clientId)
    {
        Debug.Log($"[Host] Client left | ClientID: {clientId}");
        RefreshLobbyInfo();
    }
}
