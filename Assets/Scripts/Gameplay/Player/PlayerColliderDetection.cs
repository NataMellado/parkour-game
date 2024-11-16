using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerColliderDetection : NetworkBehaviour
{
    private GameObject player;
    public NetworkVariable<bool> estaCapturado = new NetworkVariable<bool>(false);

    private List<ulong> isTargetingTo = new List<ulong>();
    private List<ulong> isTargetedBy = new List<ulong>();

    public GameObject explosionPrefab;

    private void Start()
    {
        player = transform.parent.gameObject;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("Valor inicial isTargetingTo: " + string.Join(", ", isTargetingTo));
    }

    private void OnCapturadoChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("Me han capturado! (mi id es " + OwnerClientId + ")");
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;
    }

    public void Interact()
    {
        if (isTargetingTo != null && isTargetingTo.Count != 0)
        {
            Debug.Log("Interactuando con el jugador: " + string.Join(", ", isTargetingTo));
            InteraccionServerRpc(isTargetingTo[0]);
        }
        else
        {
            Debug.Log("No hay jugadores targeteados");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
            return;

        ulong otherClientId = other.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId;

        Debug.Log(Equals(otherClientId, OwnerClientId) ? "Colision conmigo mismo" : "Colision con otro jugador");

        //// Verificar si ya está en la lista antes de agregarlo
        if (!isTargetingTo.Contains(otherClientId) && !otherClientId.Equals(OwnerClientId))
        {
            isTargetingTo.Add(otherClientId);
        }

        // aplicar lógica de si isTargetingTo es null
        //TODO: en caso de que hayan varios isTargettingTo, que se setee el más cercano


        string otherPlayerName = other.gameObject.GetComponentInParent<PlayerNameSync>().networkPlayerName.Value.ToString();
        PlayerTeamSync.Team ownerTeam = player.GetComponent<PlayerTeamSync>().networkPlayerTeam.Value;
        PlayerTeamSync.Team otherPlayerTeam = other.gameObject.GetComponentInParent<PlayerTeamSync>().networkPlayerTeam.Value;

        if (otherPlayerTeam == PlayerTeamSync.Team.Ladrones)
        {
            Debug.Log("Colision con ladron");
            //player.GetComponent<PlayerPresentation>().HandleEnemyColissionServerRpc(other.GetComponent<NetworkObject>().OwnerClientId);
        }

        if (otherPlayerTeam == PlayerTeamSync.Team.Policias)
        {
            Debug.Log("Colision con policia");
            //player.GetComponent<PlayerPresentation>().HandleEnemyColissionServerRpc(other.GetComponent<NetworkObject>().OwnerClientId);
        }
        Debug.Log("Mi equipo: " + ownerTeam.ToString() + " Equipo del otro: " + otherPlayerTeam.ToString());
        //if (playerTeam == PlayerTeamSync.Team.Policias)
        //{
        //    if (other.CompareTag("Ladron"))
        //    {
        //        Debug.Log("Colision con ladron");
        //        var otherPlayer = other.GetComponent<PlayerTeamSync>();
        //        if (otherPlayer != null && otherPlayer.networkPlayerTeam.Value == PlayerTeamSync.Team.Ladrones)
        //        {
        //            CaptuarLadronServerRpc(otherPlayer.OwnerClientId);
        //        }
        //    }

        //}
    }

    // método para verificar si el jugador está en el target de otro jugador
    private bool IsTarget(ulong targetClientId)
    {
        if (isTargetingTo != null && isTargetingTo.Contains(targetClientId))
        {
            Debug.Log("Estoy targeteando a " + targetClientId);
            return true;
        }
        return false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        ulong otherClientId = other.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId;
        if (IsTarget(otherClientId))
        {
            isTargetingTo.Remove(otherClientId);
        }
    }

    [ServerRpc]
    private void InteraccionServerRpc(ulong interactedPlayerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(interactedPlayerId, out var interactedPlayer))
        {
            NetworkObject interactedPlayerObject = interactedPlayer.PlayerObject;
            Vector3 targetPosition = interactedPlayerObject.transform.position;

            Debug.Log($"[SERVER] Jugador {clientId} interactuando con {interactedPlayer}");

            // sumar una unidad de altura para que la explosión no esté en el suelo
            targetPosition.y += 1;

            GameObject explosion = Instantiate(explosionPrefab, targetPosition, Quaternion.identity);

            NetworkObject explosionNetworkObject = explosion.GetComponent<NetworkObject>();
            explosionNetworkObject.Spawn();
        }
        else
        {
            Debug.Log($"[SERVER] No se encontró al player con el clientId {interactedPlayerId}");
        }
    }


    //[ServerRpc]
    //private void CaptuarLadronServerRpc(ulong ladronClientId)
    //{
    //    if (playerTeam != PlayerTeamSync.Team.Policias)
    //        return;

    //    // Obtener jugador ladrón
    //    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(ladronClientId, out var client))
    //    {
    //        var ladronPlayer = client.PlayerObject.GetComponent<PlayerTeamSync>();

    //        if (ladronPlayer != null && ladronPlayer.networkPlayerTeam.Value == PlayerTeamSync.Team.Ladrones)
    //        {
    //            string ladronNombre = client.PlayerObject.GetComponent<PlayerNameSync>().networkPlayerName.Value.ToString();
    //            Debug.Log("Se ha capturado al player: " + ladronNombre);
    //        }
    //    }
    //}

}
