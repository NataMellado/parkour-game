using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Main class for the Hide and Seek game mode.
/// Loop for the game mode.
/// </summary>
public class HNSMain : NetworkBehaviour
{
    public static HNSMain Instance { get; private set; }

    private GameState currentGameState = GameState.Stopped;

    private void Awake()
    {
        Debug.Log("Awake servidor HNSMain");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public int roundTime = 300;
    public int minimumPlayersPerTeamToStart = 1;
    public bool minimumPlayersReached = false;
    public int policiasPlayers = 0;
    public int ladronesPlayers = 0;



    //> Gamemode States

    enum GameState
    {
        Stopped,
        Preparing,
        Starting,
        Started,
        Playing,
        Ending,
        Ended
    }

    public void StartHNSMain()
    {
        Console.WriteLine("HNSMain started");
        PreStartGame();
        //StartCoroutine(TeleportPlayers10Seconds());
    }

    public IEnumerator CheckForConnectedPlayers()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            yield return new WaitForSeconds(2f);
            // Get connected player objects
            //Debug.Log("Imprimiendo connected players hnsmain...");
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                PlayerNameSync playerNameSync = player.GetComponent<PlayerNameSync>();
                PlayerTeamSync playerTeamSync = player.GetComponent<PlayerTeamSync>();
                if (playerNameSync != null && playerTeamSync != null)
                {
                    //Debug.Log("Player name: " + playerNameSync.networkPlayerName.Value);
                    //Debug.Log("Player team: " + playerTeamSync.networkPlayerTeam.Value);
                }
            }

            SetPlayersPerTeam();

            if (minimumPlayersReached)
            {
                Debug.Log("Minimum players reached!!!!!");
                Debug.Log("Start count of 10 seconds to starting...");
            }
            else
            {
                Debug.Log("Minimum players not reached =(");
            }
        }
    }

    public void PreStartGame()
    {
        StartCoroutine(CheckMinimumPlayersToStartHNSCoroutine());
    }

    // Corrutina que verifica cada 1 segundo si se han alcanzado los jugadores mínimos
    private IEnumerator CheckMinimumPlayersToStartHNSCoroutine()
    {
        // Mientras no se alcance el número mínimo de jugadores
        while (!minimumPlayersReached)
        {
            // Espera 1 segundo
            yield return new WaitForSeconds(1f);

            // Aquí podrías actualizar el valor de minimumPlayersReached
            // Por ejemplo, verificar el número actual de jugadores
            UpdateMinimumPlayersStatus();

            // Mensaje opcional mientras se espera
            Debug.Log("Esperando a más jugadores...");
        }

        // Cuando se alcanza el número mínimo, inicia el conteo regresivo
        Debug.Log("Se alcanzó el número mínimo de jugadores =) (server)");
        StartCountdownCoroutine();

    }

    private void UpdateMinimumPlayersStatus()
    {
        SetPlayersPerTeam();
    }

    public void StartCountdownCoroutine()
    {
        // Use LogToPlayers("10 seconds remaining") then "9 seconds remaining" and so on
        StartCoroutine(CountdownCoroutine());
        currentGameState = GameState.Preparing;
    }

    private IEnumerator StartingGameCountdown()
    {
        int segundosRestantes = 10;

        while (segundosRestantes > 0)
        {
            SetPlayersCanvasMessage("Tienes " + segundosRestantes + " segundos para esconderte!");
            yield return new WaitForSeconds(1f);
            segundosRestantes--;
        }
        ResetPlayersCanvasMessage();
    }

    private IEnumerator CountdownCoroutine()
    {
        int segundosRestantes = 10;

        while (segundosRestantes > 0)
        {
            LogToPlayers(segundosRestantes + " seconds remaining");
            yield return new WaitForSeconds(1f);
            segundosRestantes--;
        }
        LogToPlayers("HA COMENZADO HNS.... (PLAYER)");
        Debug.Log("HA COMENZADO HNS.... (SERVER)");
        StartCoroutine(StartGameLogic());
    }

    public IEnumerator StartGameLogic()
    {
        // Comenzar el juego
        // Teletranpsorta a los jugadores a sus puntos de spawn
        TeleportPlayersToSpawn();
        // Congela a los policias
        // Cuenta regresiva de 10 segundos
        yield return StartingGameCountdown();
        // Descongela a los policias
        // Comienza el juego
        Debug.Log("Partida iniciada!!! CORRAAAN (server)");
        LogToPlayers("Partida iniciada!!! CORRAAAN (players)");
    }

    public void LogToPlayers(string message)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            if (playerController != null)
            {
                playerController.LogToClientRpc(message);
            }
        }
    }

    public void SetPlayersCanvasMessage(string message)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            if (playerController != null)
            {
                playerController.SetPlayerInGameMessageClientRpc(message);
            }
        }
    }

    public void ResetPlayersCanvasMessage()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            if (playerController != null)
            {
                playerController.ResetPlayerInGameMessageClientRpc();
            }
        }
    }

    //public IEnumerator TeleportPlayers10Seconds()
    //{
    //    yield return new WaitForSeconds(1f);
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(5f);
    //        // Get connected player objects
    //        Debug.Log("Teleporting players...");
    //        TeleportPlayersToSpawn();
    //    }
    //}

    //> Function to set players per team
    private void SetPlayersPerTeam()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerTeamSync playerTeamSync = player.GetComponent<PlayerTeamSync>();
            if (playerTeamSync != null)
            {
                if (playerTeamSync.networkPlayerTeam.Value == PlayerTeamSync.Team.Policias)
                {
                    policiasPlayers++;
                }
                else if (playerTeamSync.networkPlayerTeam.Value == PlayerTeamSync.Team.Ladrones)
                {
                    ladronesPlayers++;
                }
            }
        }
        if (policiasPlayers >= minimumPlayersPerTeamToStart && ladronesPlayers >= minimumPlayersPerTeamToStart)
        {
            minimumPlayersReached = true;
        }
        else
        {
            minimumPlayersReached = false;
        }
    }

    //> Function to teleport players to their spawn points depending on their team
    private void TeleportPlayersToSpawn()
    {
        Debug.Log("Teleporting players to spawn points");
        GameObject[] policiasSpawnPoints = GameObject.FindGameObjectsWithTag("PoliciasSpawnPoint");
        GameObject[] ladronesSpawnPoints = GameObject.FindGameObjectsWithTag("LadronesSpawnPoint");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerTeamSync playerTeamSync = player.GetComponent<PlayerTeamSync>();
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            if (playerTeamSync != null && playerController != null)
            {
                Vector3 spawnPoint;
                if (playerTeamSync.networkPlayerTeam.Value == PlayerTeamSync.Team.Policias)
                {
                    // Teleport to Policias spawn point
                    spawnPoint = policiasSpawnPoints[UnityEngine.Random.Range(0, policiasSpawnPoints.Length - 1)].transform.position;
                    Debug.Log("Teleporting player to Policias spawn point");
                }
                else if (playerTeamSync.networkPlayerTeam.Value == PlayerTeamSync.Team.Ladrones)
                {
                    // Teleport to Ladrones spawn point
                    spawnPoint = ladronesSpawnPoints[UnityEngine.Random.Range(0, ladronesSpawnPoints.Length - 1)].transform.position;
                    Debug.Log("Teleporting player to Ladrones spawn point");
                }
                else
                {
                    continue;
                }

                // Crear ClientRpcParams para enviar solo al cliente propietario
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { playerController.OwnerClientId }
                    }
                };

                // Llamar al TeleportClientRpc
                playerController.TeleportClientRpc(spawnPoint, clientRpcParams);
            }
        }
    }

    //> Gamemode Stats
    /// <summary>
    /// 
    /// </summary>
    private void resetPlayerStats()
    {

    }

    //> Gamemode Parameters
    // Round time
    // Max rounds per game
    // Max players per team
    // Max rounds without hiders winning

    // Any customization parameters for the game mode

    //> Gamemode Start

    //> Gamemode Loop

    //>> Start Round

    //>> Round Loop

    //>> End Round

    //> Gamemode End

}
