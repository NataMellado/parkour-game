using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Tbvl.GameManager.Gameplay;
using Unity.Netcode;
using UnityEngine;

public class CommandManager : NetworkBehaviour
{
    public static CommandManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Llama a este método desde el cliente que tenga permisos para enviar comandos
    public void ExecuteCommand(ulong clientId, string command)
    {
        Debug.Log("Ejecutando comando...");
        if (IsOwner) // Solo el propietario autorizado puede ejecutar comandos
        {
            SendCommandToServerRpc(clientId, command);  // Enviar el comando al servidor
        }
    }

    // ServerRpc que ejecuta el comando en el servidor
    [ServerRpc(RequireOwnership = false)]
    private void SendCommandToServerRpc(ulong clientId, string command, ServerRpcParams rpcParams = default)
    {
        ProcessCommand(clientId, command);  // Procesar el comando en el servidor
    }

    // Procesar y ejecutar el comando en el servidor
    private void ProcessCommand(ulong clientId, string command)
    {
        Debug.Log("Procesando comando: " + command + " por el cliente: " + clientId.ToString());

        string[] args = command.Split(' ');

        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "cmd_AsignarEquipos":
                    if (args.Length == 2 && int.TryParse(args[1], out int teamIndex))
                    {
                        Team newTeam;
                        switch (teamIndex)
                        {
                            case 0:
                                newTeam = Team.SinEquipo;
                                break;
                            case 1:
                                newTeam = Team.Policias;
                                break;
                            case 2:
                                newTeam = Team.Ladrones;
                                break;
                            default:
                                Debug.LogWarning("Índice de equipo no válido: " + teamIndex);
                                return;
                        }
                        AsignarEquipos(newTeam);
                    }
                    break;
                // Otros comandos pueden ir aquí
                default:
                    Debug.LogWarning("Comando no reconocido: " + command);
                    break;
            }
        }
    }

    // Asignar equipos a todos los jugadores
    private void AsignarEquipos(Team team)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            var playerTeamSync = playerObject.GetComponent<NetworkTeamSync>();

            if (playerTeamSync != null)
            {
                // Asignar el equipo a todos los jugadores
                playerTeamSync.EstablecerEquipoJugadorServerRpc(team);
            }
        }
    }
}