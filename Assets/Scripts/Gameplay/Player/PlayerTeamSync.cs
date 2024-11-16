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
        networkPlayerTeam.OnValueChanged += OnTeamChanged;
        if (IsOwner)
        {
            Debug.Log("Asignando equipo");
            AssignTeamServerRpc();
            //StartCoroutine(CheckMyTeam());
        }
        /*
        else
        {
            // Si no somos el propietario, podemos verificar si el equipo ya está asignado
            if (networkPlayerTeam.Value != Team.SinEquipo)
            {
                OnTeamChanged(Team.SinEquipo, networkPlayerTeam.Value);
            }
        }*/
        StartCoroutine(WaitAndInitializeUI());
    }
    private void OnDestroy()
    {
        // Desuscribirse del evento
        networkPlayerTeam.OnValueChanged -= OnTeamChanged;
    }

    private IEnumerator WaitAndInitializeUI()
    {
        // Esperar un frame para asegurar que las NetworkVariables estén sincronizadas
        yield return null;

        if (networkPlayerTeam.Value != Team.SinEquipo)
        {
            UpdateTeamUI(networkPlayerTeam.Value);
        }
    }
    private IEnumerator CheckMyTeam()
    {
        while (true) {
            //Debug.LogWarning("CheckMyTeam: " + OwnerClientId + " -> " + networkPlayerTeam.Value);
            yield return new WaitForSeconds(2);
        };
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignTeamServerRpc(ServerRpcParams rpcParams = default)
    {
        try
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            Team assignedTeam = GetAssignedTeam(clientId);
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    var playerTeamSync = playerObject.GetComponent<PlayerTeamSync>();
                    if (playerTeamSync != null)
                    {
                        playerTeamSync.networkPlayerTeam.Value = assignedTeam;
                        Debug.Log($"[SERVER] (AssignTeamServerRpc) Team asignado a {clientId}: {assignedTeam.ToString()}");
                    }
                    else
                    {
                        Debug.LogError($"[SERVER] PlayerTeamSync no encontrado en el objeto del jugador para clientId: {clientId}");
                    }
                }
                else
                {
                    Debug.LogError($"[SERVER] PlayerObject es null para clientId: {clientId}");
                }
            }
            else
            {
                Debug.LogError($"[SERVER] ConnectedClients no contiene clientId: {clientId}");
            }
            //Debug.Log("[SERVER] Equipo asignado" + " a ID[" + clientId + "]: " + networkPlayerTeam.Value);
        } catch (System.Exception e)
        {
            Debug.LogError($"[SERVER] Error en AssignTeamServerRpc(): {e.Message}");
        }
    }

    private Team GetAssignedTeam(ulong clientId)
    {
        // Implementar la lógica para asignar un equipo al jugador
        if (clientId % 2 == 0)
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
