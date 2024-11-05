using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance { get; private set; }
    public GameObject[] playerPrefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Asegurar singleton
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitCharacterSelectionServerRpc(int selectedIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Validar el índice
        if (selectedIndex < 0 || selectedIndex >= playerPrefabs.Length)
        {
            Debug.LogWarning($"Índice de personaje inválido recibido del cliente {clientId}");
            return;
        }

        // Instanciar y spawnear el prefab seleccionado
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");


        GameObject playerPrefab = playerPrefabs[selectedIndex];
        GameObject playerInstance = Instantiate(playerPrefab,
            spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length - 1)].transform.position,
            Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"Jugador spawneado para el cliente {clientId} con el prefab índice {selectedIndex}");
    }
}
