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
    public static TeamsManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("Awake TeamsManager");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Switches the team.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public void SwitchTeam(ulong clientId)
    {
        try
        {

            if (!IsServer) return;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
            {
                //Debug.Log($"Cambiando equipo de jugador...");
                var playerObject = networkClient.PlayerObject;
                if (playerObject != null)
                {
                    var playerTeamSync = playerObject.GetComponent<PlayerTeamSync>();
                    if (playerTeamSync != null)
                    {
                        var currentTeam = playerTeamSync.networkPlayerTeam.Value;
                        //Debug.LogWarning("Current team: " + currentTeam.ToString());
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
                        //Debug.Log($"[SERVER] (TeamsManager.SwitchTeam) equipo cambiado de {clientId}: {newTeam}");
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
        }catch(System.Exception e)
        {
            Debug.LogError($"Error en TeamsManager.SwitchTeam(): {e.Message}");
        }
    }

}
