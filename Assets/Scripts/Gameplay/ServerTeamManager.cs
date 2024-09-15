using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Tbvl.GameManager.Gameplay;

// Clase que se encarga de gestionar el equipo del servidor
public class ServerTeamManager : NetworkBehaviour
{
    PlayerTeam playerTeam;

    Teams.Team teamPolicias = Teams.Team.Policias;
    Teams.Team teamLadrones = Teams.Team.Ladrones;

    [ClientRpc]
    public void SetTeamClientRpc(Teams.Team team)
    {
        // Obtener id del jugador
        ulong id = NetworkManager.Singleton.LocalClientId;
        Debug.Log("Cambiando equipo del jugador: " + id + " al team: " + team);
        playerTeam.SwitchToTeam(teamPolicias);
    }
}
