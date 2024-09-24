using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject playerPrefab;
    public int clientID;

    private GameObject mPlayer;
    bool playerExists = false;
    NetworkObject[] networkObjects;

    private void Awake()
    {
        networkObjects = FindObjectsOfType<NetworkObject>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        clientID = (int)OwnerClientId;

        // Check if playerPrefab exists with clientID ownership:
        foreach (NetworkObject networkObject in networkObjects)
        {
            if (networkObject.OwnerClientId == (ulong) clientID)
            {
                playerExists = true;
                break;
            }
        }



        if (IsServer && !playerExists)
        {
            mPlayer = SpawnPlayer();
        }
    }

    public GameObject SpawnPlayer()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        GameObject go = Instantiate(playerPrefab,
            spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length - 1)].transform.position,
            Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        Debug.Log("SpawnPlayer ejecutado");
        return go;
    }
}
