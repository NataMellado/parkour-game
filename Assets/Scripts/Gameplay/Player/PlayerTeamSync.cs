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
        public Team Team { get; private set; }
        public string TeamName { get; private set; }
        public string TeamPrefix { get; private set; }
        public Color TeamColor { get; private set; }

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

    private NameLayerAssigner nameLayerAssigner;

    public override void OnNetworkSpawn()
    {
        nameLayerAssigner = GetComponent<NameLayerAssigner>();
        networkPlayerTeam.OnValueChanged += OnTeamChanged;
        //if (IsOwner)
        //    //{
        //    //    Debug.Log("Asignando equipo");
        //    //    AssignTeamServerRpc();
        //    StartCoroutine(CheckMyTeam());
        //}
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
        if (IsOwner)
            StartCoroutine(UpdateNameLayers());
    }

    private void OnEnable()
    {
        networkPlayerTeam.OnValueChanged += OnTeamChanged;
    }

    private void OnDisable()
    {
        networkPlayerTeam.OnValueChanged -= OnTeamChanged;
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

        //if (networkPlayerTeam.Value != Team.SinEquipo)
        //{
        //    UpdateTeamUI(networkPlayerTeam.Value);
        //}
        UpdateTeamUI(networkPlayerTeam.Value);
    }
    private IEnumerator CheckMyTeam()
    {
        while (true)
        {
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
        }
        catch (System.Exception e)
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
        //NotifyTeamChangedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyTeamChangedServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerTeamSync playerTeamSync = player.GetComponent<PlayerTeamSync>();
            if (playerTeamSync != null)
            {
                playerTeamSync.UpdateMyTeamLayers();
            }
        }
    }

    public void UpdateMyTeamLayers()
    {
        if (IsOwner)
            StartCoroutine(UpdateNameLayers());
    }

    public IEnumerator UpdateNameLayers()
    {
        yield return null;
        Debug.Log("Actualizando capas de nombres...");
        Debug.Log("Mi cliente ID: " + OwnerClientId);
        // esperar un frame para asegurar que los objetos de texto estén sincronizados
        foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
            {
                // Obtener el equipo del jugador actual
                PlayerTeamSync.Team playerTeam = playerTeamSync.networkPlayerTeam.Value;
                // Obtener el equipo del jugador local dado su LocalClientId:
                
                Team localPlayerTeam = GetComponent<PlayerTeamSync>().networkPlayerTeam.Value;
                Debug.Log($"Mi equipo: {localPlayerTeam}");
                // Obtener los objetos de texto
                GameObject playerTeamText = playerTeamSync.nombreEquipoText.gameObject;
                GameObject playerNameText = playerObject.GetComponent<PlayerNameSync>().nombreJugadorText.gameObject;

                if (playerObject.IsLocalPlayer)
                {
                    // Nombre del propio jugador
                    SetLayer(playerTeamText, LayerMask.NameToLayer("OwnName"));
                    SetLayer(playerNameText, LayerMask.NameToLayer("OwnName"));
                    Debug.Log(playerObject.GetComponent<PlayerNameSync>().networkPlayerName.Value + $" soy yo jaja");
                }
                else if (playerTeam == localPlayerTeam)
                {
                    // Compañero de equipo
                    SetLayer(playerTeamText, LayerMask.NameToLayer("TeammateNames"));
                    SetLayer(playerNameText, LayerMask.NameToLayer("TeammateNames"));
                    Debug.Log(playerObject.GetComponent<PlayerNameSync>().networkPlayerName.Value + $" es compañero");
                }
                else
                {
                    // Jugador del equipo contrario
                    SetLayer(playerTeamText, LayerMask.NameToLayer("EnemyNames"));
                    SetLayer(playerNameText, LayerMask.NameToLayer("EnemyNames"));
                    Debug.Log(playerObject.GetComponent<PlayerNameSync>().networkPlayerName.Value + $" es enemigo");
                }
            }
        }

        // Opcional: Esperar un tiempo antes de volver a actualizar
        yield return new WaitForSeconds(0.2f);
    }

    void SetLayer(GameObject obj, int newLayer)
    {
        if (obj == null)
            return;

        obj.layer = newLayer;

    }
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
            return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null)
                continue;

            SetLayerRecursively(child.gameObject, newLayer);
        }
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
        }
        else
        {
            Debug.LogWarning("nombreEquipoText no está asignado en " + gameObject.name);
        }
    }

}
