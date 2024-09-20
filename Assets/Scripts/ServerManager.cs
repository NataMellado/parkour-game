using System.Collections;
using Tbvl.GameManager.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    UnityTransport transport;
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

                transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport != null)
                {
                    transport.MaxSendQueueSize = 8192;
                    // ajustar timeouts
                    //NetworkManager.Singleton.NetworkConfig.timeout �? �? = 10000;
                    NetworkManager.Singleton.NetworkConfig.TickRate = 30;

                    NetworkManager.Singleton.StartServer();

                    NetworkManager.Singleton.OnClientConnectedCallback += MensajeConexionCliente;

                    StartCoroutine(PingearClientes());            
                }
            }
        }


        //StartCoroutine(CambiosEquipo(5));

    }

    public void MensajeConexionCliente(ulong idConexion)
    {
        Debug.Log(idConexion + " se ha conectado");
    }
    public IEnumerator PingearClientes()
    {
        yield return new WaitForSeconds(20f);

        while (NetworkManager.Singleton.IsServer)
        {
            yield return new WaitForSeconds(20f);
            Debug.Log("Enviando ping....");
            PingClientRpc();
        }
    }
    public IEnumerator CambiosEquipo(float n)
    {
        yield return new WaitForSeconds(n);

        while (NetworkManager.Singleton.IsServer)
        {
            yield return new WaitForSeconds(n);
            Debug.Log("Cambiando equipos...");
            IntercambiarEquipos();
        }
    }

    [ClientRpc]
    public void PingClientRpc()
    {
        Debug.Log("Ping recibido desde el servidor!");
    }

    private void ActualizarEquipoJugador(Team team)
    {
        
        // Tomar su NetworkVariable playerTeam y actualizarla
        NetworkVariable<Team> playerTeam = GetComponent<NetworkTeamSync>().playerTeam;

        playerTeam.Value = team;
    }

    private void IntercambiarEquipos()
    {
        // Asignar equipos a los jugadores
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Obtener el NetworkObject del cliente
            var playerObject = client.PlayerObject;
            // Obtener NetworkTeamSync del jugador
            var playerTeamSync = playerObject.GetComponent<NetworkTeamSync>();

            if (playerTeamSync != null)
            {
                // Obtener equipo actual del jugador
                Team currentTeam = playerTeamSync.playerTeam.Value;
                // Cambiar el equipo del jugador
                Team newTeam = currentTeam == Team.Policias ? Team.Ladrones : Team.Policias;
                // Actualizar el equipo del jugador
                playerTeamSync.playerTeam.Value = newTeam;
                NotificarCambioEquipoJugadorClientRpc(newTeam);
            }
        }
    }

    //[ClientRpc]
    //public void ActualizarEquipoClientRpc(Team team) 
    //{
    //    Debug.Log("Actualizando equipo a: " + team);
    //    // ActualizarUIJugador en el jugador
    //    GetComponent<NetworkTeamSync>().ActualizarUIJugador(team);
    //}

    public void AsignarEquipoJugadores(Team newTeam)
    {
        // Asignar equipos a los jugadores
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Obtener el NetworkObject del cliente
            var playerObject = client.PlayerObject;
            // Obtener NetworkTeamSync del jugador
            var playerTeamSync = playerObject.GetComponent<NetworkTeamSync>();

            if (playerTeamSync != null)
            {
                // Asignar el equipo al jugador
                playerTeamSync.playerTeam.Value = newTeam;
            }
        }
        NotificarCambioEquipoJugadorClientRpc(newTeam);
    }

    [ClientRpc]
    public void NotificarCambioEquipoJugadorClientRpc(Team newTeam)
    {
        // Notificar a los jugadores el cambio de equipo
        Debug.Log("Se ha actualizado tu equipo a: " + newTeam.ToString());
    }


}
