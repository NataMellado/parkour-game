using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class NameLayerAssigner : NetworkBehaviour
{
    private PlayerTeamSync.Team localTeam;
    private bool isPlayerConnected;

    void Start()
    {
        Debug.Log("Start NameLayerAssigner");
        if (IsClient && IsOwner)
        {
            Debug.Log("IsClient && IsOwner");
            // Obtener el equipo del jugador local
            localTeam = GetComponent<PlayerTeamSync>().networkPlayerTeam.Value;

            // Iniciar la corrutina para actualizar las capas
            StartCoroutine(UpdateNameLayers());
        }
    }

    IEnumerator UpdateNameLayers()
    {
        while (true)
        {
            // Obtener todos los jugadores
            foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
                {
                    // Obtener el equipo del jugador
                    PlayerTeamSync.Team playerTeam = playerTeamSync.networkPlayerTeam.Value;
                    // Obtener GameObject con el nombre PlayerNameItems
                    GameObject playerTeamText = playerTeamSync.nombreEquipoText.gameObject;
                    GameObject playerNameText = playerObject.GetComponent<PlayerNameSync>().nombreJugadorText.gameObject;

                     //Asignar la capa adecuada al nombre
                    if (playerObject.OwnerClientId == OwnerClientId)
                    {
                        // Nombre del propio jugador
                        SetLayer(playerTeamText, LayerMask.NameToLayer("OwnName"));
                        SetLayer(playerNameText, LayerMask.NameToLayer("OwnName"));
                    }
                    else if (playerTeam == localTeam)
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

            // Esperar un tiempo antes de volver a actualizar
            yield return new WaitForSeconds(1f);
        }
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
}