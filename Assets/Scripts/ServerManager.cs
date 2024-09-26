using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : NetworkBehaviour
{
    [SerializeField] TeamsManager teamsManager;
    public static ServerManager Instance { get; private set; }

    public List<ulong> connectedPlayers; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Initialize the list of connected players
        connectedPlayers = new List<ulong>();
        
        // Subscribe to the events of player connection and disconnection
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;

    }

    private void OnPlayerConnected(ulong clientId)
    {
        //Debug.Log("Player connected: " + clientId);
        // Add to connectedPlayers the connected player clientId
        connectedPlayers.Add(clientId);
    }

    private bool IsPlayerConnected(ulong clientId)
    {
        return connectedPlayers.Contains(clientId);
        
    }

    private void OnPlayerDisconnected(ulong clientId)
    {

        //Debug.Log("Player disconnected: " + clientId);
        // Remove from connectedPlayers the disconnected player clientId
        try
        {
            connectedPlayers.Remove(clientId);
        }catch(System.Exception e)
        {
            Debug.LogError("Error al desconectar jugador: " + e.Message);
        }
    }

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-s")
            {
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");
                Debug.Log("======================= MODO SERVIDOR INICIADO ========================");

                NetworkManager.Singleton.StartServer();

                NetworkManager.Singleton.OnClientConnectedCallback += MensajeConexionCliente;

                StartCoroutine(PingearClientes());    

            }
        } 
    }

    public void MensajeConexionCliente(ulong idConexion)
    {
        Debug.Log(idConexion + " se ha conectado");
    }
    public IEnumerator PingearClientes()
    {
        yield return new WaitForSeconds(10f);

        while (NetworkManager.Singleton.IsServer)
        {
            yield return new WaitForSeconds(10f);
            Debug.Log("Enviando ping....");
            PingClientRpc();
            Debug.Log("Alterando equipos!");
            AlternarEquipos();
        }
    }   

    [ClientRpc]
    public void PingClientRpc()
    {
        Debug.Log($"[Cliente {NetworkManager.Singleton.LocalClientId}] Ping recibido desde el servidor!");
    }

    public void AlternarEquipos()
    {
        // Usar SwitchTeam() en cada jugador para cambiar su equipo

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            teamsManager.SwitchTeam(client.ClientId);
        }
    }
}
