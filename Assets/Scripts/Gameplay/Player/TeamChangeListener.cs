using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class TeamChangeListener : NetworkBehaviour
{
    private List<PlayerTeamSync> playerTeamSyncs = new List<PlayerTeamSync>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            StartCoroutine(SubscribeToPlayers());
        }
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        StartCoroutine(UpdateNameLayers());
    }


    public void OnClientConnected(ulong clientId)
    {
        if (IsClient)
        {
            StartCoroutine(SubscribeToPlayers());
        }
    }

    private IEnumerator SubscribeToPlayers()
    {
        // Esperar a que todos los objetos estén instanciados
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Suscribiendo a los jugadores...");
        // Suscribirse al evento OnValueChanged de networkPlayerTeam de todos los jugadores
        foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
            {
                if (!playerTeamSyncs.Contains(playerTeamSync))
                {
                    playerTeamSync.networkPlayerTeam.OnValueChanged += OnAnyPlayerTeamChanged;
                    playerTeamSyncs.Add(playerTeamSync);
                }
            }
        }
    }

    private void OnAnyPlayerTeamChanged(PlayerTeamSync.Team previousTeam, PlayerTeamSync.Team newTeam)
    {
        // Ejecutar UpdateNameLayers cuando cualquier jugador cambie de equipo
        StartCoroutine(UpdateNameLayers());
        Debug.Log("Equipo de un jugador cambiado");
    }

    public IEnumerator UpdateNameLayers()
    {
        // Esperar un frame para asegurar que los objetos están sincronizados
        yield return null;

        Debug.Log("Actualizando capas de nombres...");

        // Obtener el equipo del jugador local
        PlayerTeamSync.Team localPlayerTeam = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerTeamSync>().networkPlayerTeam.Value;

        // Obtener todos los jugadores
        foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
            {
                // Obtener el equipo del jugador actual
                PlayerTeamSync.Team playerTeam = playerTeamSync.networkPlayerTeam.Value;

                // Obtener los objetos de texto
                GameObject playerTeamText = playerTeamSync.nombreEquipoText.gameObject;
                GameObject playerNameText = playerObject.GetComponent<PlayerNameSync>().nombreJugadorText.gameObject;

                if (playerObject.IsLocalPlayer)
                {
                    // Nombre del propio jugador
                    SetLayer(playerTeamText, LayerMask.NameToLayer("OwnName"));
                    SetLayer(playerNameText, LayerMask.NameToLayer("OwnName"));
                }
                else if (playerTeam == localPlayerTeam)
                {
                    // Compañero de equipo
                    SetLayer(playerTeamText, LayerMask.NameToLayer("TeammateNames"));
                    SetLayer(playerNameText, LayerMask.NameToLayer("TeammateNames"));
                }
                else
                {
                    // Jugador del equipo contrario
                    SetLayer(playerTeamText, LayerMask.NameToLayer("EnemyNames"));
                    SetLayer(playerNameText, LayerMask.NameToLayer("EnemyNames"));
                }
            }
        }
    }

    private void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de los eventos al destruir el objeto
        foreach (var playerTeamSync in playerTeamSyncs)
        {
            playerTeamSync.networkPlayerTeam.OnValueChanged -= OnAnyPlayerTeamChanged;
        }
    }
}