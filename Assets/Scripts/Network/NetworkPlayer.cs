using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject playerPrefab;
    public int clientID;


    public override void OnNetworkSpawn()
    {
        if(IsServer){
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
        }
    }

    private void SpawnPlayerForClient(ulong clientId){
        
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length - 1)].transform.position;
        GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public override void OnDestroy()
    {
        if (IsServer){
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerForClient;
        }
    }

    /*private GameObject mPlayer;
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        clientID = (int)OwnerClientId;

        if (IsServer && mPlayer == null){
            mPlayer = SpawnPlayer();
        }

    }
    public GameObject SpawnPlayer(){
        Debug.Log("SpawnPlayer en curso...");
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        GameObject go = Instantiate(playerPrefab,
        spawnPoints[Random.Range(0, spawnPoints.Length - 1)].transform.position,
        Quaternion.identity);

        Debug.Log("Spawneando con ownership: " + OwnerClientId);
        Debug.Log($"{go.name}");

        // Revisar si existe un objeto con NetworkObjectId en la escena
        // Si no existe, spawnea el objeto con ownership
        go.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        Debug.Log("SpawnPlayer ejecutado");
        return go;
        
    }
        */

}
