using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerTeamSync : NetworkBehaviour
{
    /* 
        SinEquipo,
        Espectador,
        Policias,
        Ladrones,
    */
    public enum Team
    {
        SinEquipo,
        Espectador,
        Policias,
        Ladrones,
    }
    public class Equipo
    {
        public Team Team { get; private set;}
        public string TeamName { get; private set;}
        public string TeamPrefix { get; private set; }
        public Color TeamColor { get; private set;}

        public Equipo(Team team, string teamName, string teamPrefix, Color teamColor)
        {
            Team = team;
            TeamName = teamName;
            TeamPrefix = teamPrefix;
            TeamColor = teamColor;
        }
    }

    public static class Equipos
    {
        public static readonly Dictionary<Team, Equipo> equipos =
            new Dictionary<Team, Equipo>
            {
                { Team.SinEquipo, new Equipo(Team.SinEquipo, "Sin Equipo", "N/A", Color.white) },
                { Team.Espectador, new Equipo(Team.Espectador, "Espectador", "ES", Color.gray) },
                { Team.Policias, new Equipo(Team.Policias, "Policias", "P", Color.blue) },
                { Team.Ladrones, new Equipo(Team.Ladrones, "Ladrones", "L", Color.red) },
            };
    }

    public NetworkVariable<Team> networkPlayerTeam = new NetworkVariable<Team>(
        Team.SinEquipo,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);

    [SerializeField]
    public TextMeshPro nombreEquipoText;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AssignTeam();
        }
        networkPlayerTeam.OnValueChanged += OnTeamChanged;
        UpdateTeamUI(networkPlayerTeam.Value);
    }
    private void OnDestroy()
    {
        // Desuscribirse del evento
        networkPlayerTeam.OnValueChanged -= OnTeamChanged;
    }

    private void AssignTeam()
    {
        networkPlayerTeam.Value = GetAssignedTeam();
    }

    private Team GetAssignedTeam()
    {
        // Implementar la lógica para asignar un equipo al jugador
        if (OwnerClientId % 2 == 0)
        {
            return Team.Policias;
        }
        else
        {
            return Team.Ladrones;
        }
    }

    private void OnTeamChanged(Team previousTeam, Team newTeam)
    {
        UpdateTeamUI(newTeam);
    }

    private void UpdateTeamUI(Team team)
    {
        // Implementar la lógica para actualizar la UI con el equipo del jugador
        
        if (nombreEquipoText != null)
        {
            nombreEquipoText.text = team.ToString();
            switch (team)
            {
                case Team.Policias:
                    nombreEquipoText.color = Equipos.equipos[Team.Policias].TeamColor;
                    break;
                case Team.Ladrones:
                    nombreEquipoText.color = Equipos.equipos[Team.Ladrones].TeamColor;
                    break;
                case Team.Espectador:
                    nombreEquipoText.color = Equipos.equipos[Team.Espectador].TeamColor;
                    break;
                default:
                    nombreEquipoText.color = Equipos.equipos[Team.SinEquipo].TeamColor;
                    break;
            }
        }else
        {
            Debug.LogWarning("nombreEquipoText no está asignado en " + gameObject.name);
        }
    }

}
