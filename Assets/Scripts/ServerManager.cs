using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : NetworkBehaviour
{
    [SerializeField] TeamsManager teamsManager;

    public static ServerManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("Awake servidor");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //StartCoroutine(CheckConnectedPlayers());

    }


    public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

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


                StartCoroutine(PingearClientes());


            }
        }
        if (IsServer)
        {

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            Debug.Log("Server started");
            HNSMain.Instance.StartHNSMain();
        }
    }

    public void ServerStartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        Debug.Log("Server started as HOST MODE");
        HNSMain.Instance.StartHNSMain();
    }

    public void OnClientConnected(ulong idConexion)
    {
        Debug.Log(idConexion + " se ha conectado");
        connectedPlayersCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
    public void OnClientDisconnected(ulong idConexion)
    {
        Debug.Log(idConexion + " se ha desconectado");
        connectedPlayersCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
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
            //AlternarEquipos();
        }
    }
    //si funciona
    //public IEnumerator CheckConnectedPlayers()
    //{
    //    yield return new WaitForSeconds(10f);
        
    //    while (true) {
    //        yield return new WaitForSeconds(10f);
    //        // Get connected player objects
    //        Debug.Log("Imprimiendo connected players servermanager...");
    //    }
    //}

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
