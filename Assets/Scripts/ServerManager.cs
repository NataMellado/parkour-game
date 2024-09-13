using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
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
        Debug.Log(idConexion + " se ha conectado");
        Debug.Log(idConexion + " se ha conectado");
        Debug.Log(idConexion + " se ha conectado");
    }
    public IEnumerator PingearClientes()
    {
        yield return new WaitForSeconds(5f);

        while (NetworkManager.Singleton.IsServer)
        {
            yield return new WaitForSeconds(5f);
            Debug.Log("Enviando ping....");
            PingClientRpc();
        }
    }

    [ClientRpc]
    public void PingClientRpc()
    {
        Debug.Log("Ping recibido desde el servidor!");
        Debug.Log("Ping recibido desde el servidor!");
        Debug.Log("Ping recibido desde el servidor!");
    }

}
