using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChangeTeamScript : NetworkBehaviour
{
    public static ChangeTeamScript Instance { get; private set; }
    private void Awake()
    {
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

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarCambioEquipoServerRpc(PlayerTeamSync.Team newTeam, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        IReadOnlyDictionary<ulong, NetworkClient> jugadores = NetworkManager.Singleton.ConnectedClients;
        PlayerTeamSync.Team clientTeam = jugadores[clientId].PlayerObject.GetComponent<PlayerTeamSync>().networkPlayerTeam.Value;

        int ladronesPlayers = HNSMain.Instance.ladronesPlayers;
        int policiasPlayers = HNSMain.Instance.policiasPlayers;
        
        if (clientTeam == newTeam)
        {
            Debug.LogWarning("El jugador ya está en ese equipo.");
            return;
        }
        Debug.Log("Solicitando cambio de equipo...");
        Debug.Log("Ladrones : " + ladronesPlayers);
        Debug.Log("Policias : " + policiasPlayers);

        if (ladronesPlayers == policiasPlayers)
        {
            // Si puede cambiar ya que están parejos
        } else if (ladronesPlayers > policiasPlayers)
        {
            // Si hay más ladrones

        } else if (policiasPlayers > ladronesPlayers)
        {
            // Si hay más policias
        }
        jugadores[clientId].PlayerObject.GetComponent<PlayerTeamSync>().networkPlayerTeam.Value = newTeam;
    }
}
