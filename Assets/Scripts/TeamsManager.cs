using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Class for managing the teams of the players.
/// Changing teams.
/// </summary>
public class TeamsManager : NetworkBehaviour
{

    /// <summary>
    /// Changes the player team.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="newTeam">The new team.</param>
    public void ChangePlayerTeam(ulong clientId, PlayerTeamSync.Team newTeam)
    {

        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            var playerObject = networkClient.PlayerObject;
            if (playerObject != null)
            {
                var playerTeamSync = playerObject.GetComponent<PlayerTeamSync>();
                if (playerTeamSync != null)
                {
                    playerTeamSync.networkPlayerTeam.Value = newTeam;
                    Debug.Log($"El equipo del jugador con clientId {clientId} ha sido cambiado a {newTeam}");
                    return;
                }else
                {
                    Debug.Log($"PlayerTeamSync not found in player with clientId {clientId}");
                }
            }else
            {
                Debug.Log($"PlayerObject is null for clientId {clientId}");
            }
        }else
        {
            Debug.LogError($"ClientId {clientId} no encontrado en ConnectedClients");
        }
    }

    /// <summary>
    /// Switches the team.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public void SwitchTeam(ulong clientId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            var playerObject = networkClient.PlayerObject;
            if (playerObject != null)
            {
                var playerTeamSync = playerObject.GetComponent<PlayerTeamSync>();
                if (playerTeamSync != null)
                {
                    var currentTeam = playerTeamSync.networkPlayerTeam.Value;
                    PlayerTeamSync.Team newTeam = PlayerTeamSync.Team.SinEquipo;
                    switch (currentTeam)
                    {
                        case PlayerTeamSync.Team.Policias:
                            newTeam = PlayerTeamSync.Team.Ladrones;
                            break;
                        case PlayerTeamSync.Team.Ladrones:
                            newTeam = PlayerTeamSync.Team.Policias;
                            break;
                    }
                    playerTeamSync.networkPlayerTeam.Value = newTeam;
                    Debug.Log($"El equipo del jugador con clientId {clientId} ha sido cambiado a {newTeam}");
                    return;
                }
                else
                {
                    Debug.Log($"PlayerTeamSync not found in player with clientId {clientId}");
                }
            }
            else
            {
                Debug.Log($"PlayerObject is null for clientId {clientId}");
            }
        }
    }

}
