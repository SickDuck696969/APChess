using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class ClientConnector : MonoBehaviour
{
    public string relayJoinCode;
    public LobbyInfo currentlobby;
    public Lobby joinedLobby;

    public async Task<List<LobbyInfo>> BrowseLobbies()
    {
        List<LobbyInfo> lobbyList = new List<LobbyInfo>();

        try
        {
            var options = new QueryLobbiesOptions
            {
                Count = 20,
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            if (response.Results.Count == 0)
            {
                Debug.Log("[Client] No lobbies found.");
                return lobbyList;
            }

            Debug.Log($"[Client] Found {response.Results.Count} lobbies:");

            foreach (var lobby in response.Results)
            {
                string hostName = lobby.Data.ContainsKey("hostName")
                    ? lobby.Data["hostName"].Value
                    : "Unknown";

                string hostimage = lobby.Data.ContainsKey("hostImage")
                    ? lobby.Data["hostImage"].Value
                    : "Unknown";

                LobbyInfo info = new LobbyInfo
                {
                    lobbyId = lobby.Id,
                    lobbyName = lobby.Name,
                    playerCount = lobby.Players.Count,
                    maxPlayers = lobby.MaxPlayers,
                    hostName = hostName,
                    hostimage = hostimage
                };

                lobbyList.Add(info);

                Debug.Log($" - LobbyID: {info.lobbyId}, Name: {info.lobbyName}, Players: {info.playerCount}/{info.maxPlayers}, Host: {info.hostName}, imagebase64: {info.hostimage}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] Failed to browse lobbies: " + ex);
        }

        return lobbyList;
    }


    public async Task JoinLobby(string lobbyId, string playerName)
    {
        try
        {
            var options = new JoinLobbyByIdOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                {
                    { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                }
                }
            };

            // Join the lobby with player data
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

            string hostName = lobby.Data.ContainsKey("hostName")
                ? lobby.Data["hostName"].Value
                : "Unknown";
            string hostimage = lobby.Data.ContainsKey("hostImage")
                    ? lobby.Data["hostImage"].Value
                    : "Unknown";

            joinedLobby = lobby;
            currentlobby = new LobbyInfo
            {
                lobbyId = lobby.Id,
                lobbyName = lobby.Name,
                playerCount = lobby.Players.Count,
                maxPlayers = lobby.MaxPlayers,
                hostName = hostName,
                hostimage = hostimage
            };

            Debug.Log($"[Client] Joined lobby '{lobby.Name}' (ID: {lobby.Id})");

            if (!lobby.Data.ContainsKey("joinCode"))
            {
                Debug.LogError("[Client] Lobby has no joinCode stored!");
                return;
            }

            relayJoinCode = lobby.Data["joinCode"].Value;
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] Failed to join lobby: " + ex);
        }
    }



    public async Task ConnectToHost()
    {
        Debug.Log($"[Client] relayJoinCode BEFORE join call = '{relayJoinCode}'");

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (string.IsNullOrWhiteSpace(relayJoinCode))
        {
            Debug.LogError("[Client] No join code!");
            return;
        }

        try
        {
            var join = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            Debug.Log($"[Client] Joined relay allocation with JoinCode={relayJoinCode}");

            transport.SetRelayServerData(
                join.RelayServer.IpV4,
                (ushort)join.RelayServer.Port,
                join.AllocationIdBytes,
                join.Key,
                join.ConnectionData,
                join.HostConnectionData,
                false
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"[Client] Relay join failed: {e}");
            return;
        }

        Debug.Log("[Client] Connecting...");
        NetworkManager.Singleton.StartClient();
    }

    public async Task LeaveLobby()
    {
        if (joinedLobby == null)
        {
            Debug.LogWarning("[Client] No lobby to leave.");
            return;
        }

        try
        {
            string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

            Debug.Log("[Client] Successfully left lobby: " + joinedLobby.Name);

            joinedLobby = null;
            currentlobby = default;
            relayJoinCode = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] Failed to leave lobby: " + ex);
        }
    }

}

[Serializable]
public struct LobbyInfo
{
    public string lobbyId;
    public string lobbyName;
    public int playerCount;
    public int maxPlayers;
    public string hostName;
    public string hostimage;
}
