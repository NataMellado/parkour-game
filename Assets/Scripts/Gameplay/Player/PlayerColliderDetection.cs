using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerColliderDetection : NetworkBehaviour
{
    private GameObject player;
    public NetworkVariable<bool> estaCapturado = new NetworkVariable<bool>(false);

    private List<ulong> isTargetingTo = new List<ulong>();
    private List<ulong> isTargetedBy = new List<ulong>();
    private Dictionary<ulong, NetworkObject> playerObjects = new Dictionary<ulong, NetworkObject>();

    public GameObject explosionPrefab;
    public GameObject bloodPrefab;

    private void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
            {
                if (!playerObjects.ContainsKey(playerObject.OwnerClientId))
                {
                    playerObjects.Add(playerObject.OwnerClientId, playerObject);
                }
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Quitar el jugador del dictionary playerObjects
        if (playerObjects.ContainsKey(clientId))
        {
            playerObjects.Remove(clientId);
        }
    }

    private void Start()
    {
        player = transform.parent.gameObject;
    }

    public override void OnNetworkSpawn()
    {
        // Obtener todos los jugadores
        foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
            {
                ulong clientId = playerObject.OwnerClientId;

                playerObjects.Add(clientId, playerObject);
            }
        }
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
            try
            {

                InteraccionServerRpc(isTargetingTo[0]);
            }catch (System.Exception e)
            {
                Debug.LogError("Error en Interact(): " + e.Message);
            }
            // Obtener objeto del jugador targeteado

            if (playerObjects.ContainsKey(isTargetingTo[0]))
            {

                NetworkObject jugadorTargeteado = playerObjects[isTargetingTo[0]];
                Vector3 targetPosition = jugadorTargeteado.transform.position;
                targetPosition.y += 1;


                GameObject blood = Instantiate(bloodPrefab, targetPosition, Quaternion.identity);
                Destroy(blood, 2f);
            }
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

        if (other.gameObject.GetComponentInParent<NetworkObject>() == null
            || other.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId.Equals(OwnerClientId))
            return;

        ulong otherClientId = other.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId;

        string otherPlayerName = other.gameObject.GetComponentInParent<PlayerNameSync>().networkPlayerName.Value.ToString();
        PlayerTeamSync.Team ownerTeam = player.GetComponent<PlayerTeamSync>().networkPlayerTeam.Value;
        PlayerTeamSync.Team otherPlayerTeam = other.gameObject.GetComponentInParent<PlayerTeamSync>().networkPlayerTeam.Value;

        //Debug.Log(Equals(otherClientId, OwnerClientId) ? "Colision conmigo mismo" : "Colision con otro jugador");

        //// Verificar si ya está en la lista antes de agregarlo
        if (!isTargetingTo.Contains(otherClientId)
            && !otherClientId.Equals(OwnerClientId)
            && otherPlayerTeam != ownerTeam)
        {
            isTargetingTo.Add(otherClientId);
        }

        // aplicar lógica de si isTargetingTo es null
        //TODO: en caso de que hayan varios isTargettingTo, que se setee el más cercano



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
        Vector3 targetPosition;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(interactedPlayerId, out var interactedPlayer))
        {
            NetworkObject interactedPlayerObject = interactedPlayer.PlayerObject;
            targetPosition = interactedPlayerObject.transform.position;

            Debug.Log($"[SERVER] Jugador {clientId} interactuando con {interactedPlayer}");
            targetPosition.y += 1;

            //InstanciarSangreClientRpc(targetPosition, new ClientRpcParams
            //{
            //    Send = new ClientRpcSendParams
            //    {
            //        TargetClientIds = NetworkManager.Singleton.ConnectedClients.Keys.Where(id => id != clientId).ToArray()
            //    }
            //});
            GameObject blood = Instantiate(bloodPrefab, targetPosition, Quaternion.identity);

            NetworkObject bloodNetworkObject = blood.GetComponent<NetworkObject>();
            bloodNetworkObject.Spawn();

            interactedPlayerObject.GetComponent<PlayerHealthSync>().
                networkPlayerHealth.Value -= PlayerHealthSync.Damage;
        }
        else
        {
            Debug.Log($"[SERVER] No se encontró al player con el clientId {interactedPlayerId}");
        }
    }

    [ClientRpc]
    private void InstanciarSangreClientRpc(Vector3 position, ClientRpcParams clientRpcParams = default)
    {
        GameObject blood = Instantiate(bloodPrefab, position, Quaternion.identity);
        Destroy(blood, 2f);
    }
    private void DisminuirVidaJugador(int cantidad, ulong interactedPlayerId)
    {

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(interactedPlayerId, out var interactedPlayer))
        {
            Debug.Log("Disminuyendo vida del jugador " + interactedPlayerId);
            NetworkObject interactedPlayerObject = interactedPlayer.PlayerObject;
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
