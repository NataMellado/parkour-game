using System.Collections;
using System.Collections.Generic;
using Tbvl.GameManager.Gameplay;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkTeamSync : NetworkBehaviour
{
    [Header("Componentes UI")]
    [SerializeField] public TextMeshPro equipoJugadorText;

    public NetworkVariable<Team> playerTeam = new NetworkVariable<Team>(Team.SinEquipo);
    public override void OnNetworkSpawn()
    {
        Debug.Log("Network spawn");
        if (!IsServer)
        {
            ActualizarUIJugador(Team.SinEquipo);
        }

        playerTeam.OnValueChanged += OnTeamChanged;
        EstablecerEquipoJugadorServerRpc(Team.SinEquipo);
        //AsignarTeam();
        //Debug.Log("El jugador " + OwnerClientId + " se unió al equipo " + playerTeam.Value);
    }

    private void OnDestroy()
    {
        playerTeam.OnValueChanged -= OnTeamChanged;
    }

    private void AsignarTeam()
    {
        // Actualizar el equipo del jugador desde el servidor
        playerTeam.Value = OwnerClientId % 2 == 0 ? Team.Policias : Team.Ladrones;
    }

    private void OnTeamChanged(Team oldTeam, Team newTeam)
    {
        ActualizarUIJugador(newTeam);
    }

    public void ActualizarUIJugador(Team team)
    {
        Debug.Log("Actualizando UI jugador");
        // Obtener las propiedades del equipo
        TeamProperties teamProperties = TeamsManager.Instance.GetTeamProperties(team);

        if (teamProperties != null)
        {
            if (equipoJugadorText != null)
            {
                equipoJugadorText.text = teamProperties.TeamPrefix;
                equipoJugadorText.color = teamProperties.TeamColor;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EstablecerEquipoJugadorServerRpc(Team newTeam, ServerRpcParams rpcParams = default)
    {
        // Actualizar el equipo del jugador en el servidor
        playerTeam.Value = newTeam;
    }

}   
