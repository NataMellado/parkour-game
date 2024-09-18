using System.Collections;
using System.Collections.Generic;
using Tbvl.GameManager.Gameplay;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkTeamSync : NetworkBehaviour
{

    [Header("Componentes UI")]
    [SerializeField] public TextMeshPro nombreJugadorText;
    [SerializeField] public TextMeshPro equipoJugadorText;

    public NetworkVariable<Team> playerTeam = new NetworkVariable<Team>();
    public override void OnNetworkSpawn()
    {
        Debug.Log("Network spawn");
        if (IsServer)
        {
            AsignarTeam();
            Debug.Log("El jugador " + OwnerClientId + " se unió al equipo " + playerTeam.Value);
        }

        playerTeam.OnValueChanged += OnTeamChanged;
        ActualizarUIJugador(playerTeam.Value);
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

    private void ActualizarUIJugador(Team team)
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

}   
