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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

                teamsManager = FindObjectOfType<TeamsManager>();
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
