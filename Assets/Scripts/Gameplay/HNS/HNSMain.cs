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

    private List<PlayerTeamSync> playerTeamSyncs = new List<PlayerTeamSync>();
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

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
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
        Debug.Log("[SERVER] HNSMain started");
        PreStartGame();
        //StartCoroutine(TeleportPlayers10Seconds());
    }

    private IEnumerator SubscribeToPlayers()
    {
        // Esperar a que todos los objetos estén instanciados
        yield return new WaitForSeconds(0.5f);
        Debug.Log("[SERVER] Suscribiendo a los jugadores...");
        // Suscribirse al evento OnValueChanged de networkPlayerTeam de todos los jugadores
        foreach (var playerObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (playerObject.TryGetComponent<PlayerTeamSync>(out PlayerTeamSync playerTeamSync))
            {
                if (!playerTeamSyncs.Contains(playerTeamSync))
                {
                    playerTeamSync.networkPlayerTeam.OnValueChanged += OnAnyPlayerTeamChanged;
                    playerTeamSyncs.Add(playerTeamSync);
                }
            }
        }
    }

    private void OnAnyPlayerTeamChanged(PlayerTeamSync.Team previousTeam, PlayerTeamSync.Team newTeam)
    {
        Debug.Log("[SERVER] Cambio de equipo detectado...");
        SetPlayersPerTeam();
        Debug.Log("[SERVER] Policias: " + policiasPlayers + "  ||  Ladrones: " + ladronesPlayers);
    }

    public void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[SERVER] IsServer: {IsServer}, IsHost: {IsHost}, IsClient: {IsClient}");

        Debug.Log($"[SERVER] Jugador {clientId} conectado, asignando equipo correspondiente");

        StartCoroutine(AssignTeamAfterDelay(clientId));
        StartCoroutine(SubscribeToPlayers());
        SetPlayersPerTeam();
    }

    public IEnumerator AssignTeamAfterDelay(ulong clientId)
    {
        if (!IsServer) yield break;

        IReadOnlyDictionary<ulong, NetworkClient> jugadores = NetworkManager.Singleton.ConnectedClients;
        yield return new WaitForSeconds(.1f);
        if (policiasPlayers <= ladronesPlayers)
        {
            Debug.Log("[SERVER] Asignando equipo Policias a " + jugadores[clientId].PlayerObject.GetComponent<PlayerNameSync>().networkPlayerName.Value);
            policiasPlayers++;
            PlayerTeamSync playerTeamSync = jugadores[clientId].PlayerObject.GetComponent<PlayerTeamSync>();
            playerTeamSync.networkPlayerTeam.Value = PlayerTeamSync.Team.Policias;
        }
        else
        {
            Debug.Log("[SERVER] Asignando equipo Ladrones a " + jugadores[clientId].PlayerObject.GetComponent<PlayerNameSync>().networkPlayerName.Value);
            ladronesPlayers++;
            PlayerTeamSync playerTeamSync = jugadores[clientId].PlayerObject.GetComponent<PlayerTeamSync>();
            playerTeamSync.networkPlayerTeam.Value = PlayerTeamSync.Team.Ladrones;
        }
        yield return new WaitForSeconds(1f);
    }

    public void OnClientDisconnected(ulong clientId)
    {

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
                Debug.Log("[SERVER] Start count of 10 seconds to starting...");
                currentGameState = GameState.Preparing;
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
            //Debug.Log("Esperando a más jugadores...");
        }

        // Cuando se alcanza el número mínimo, inicia el conteo regresivo
        Debug.Log("[SERVER] Se alcanzó el número mínimo de jugadores =)");
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
            SetPlayersCanvasMessage("Los Ladrones tienen " + segundosRestantes + " segundos para esconderse!");
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
        Debug.Log("[SERVER] HA COMENZADO HNS.... (SERVER)");

        StartCoroutine(StartGameLogic());
    }

    public IEnumerator StartGameLogic()
    {
        currentGameState = GameState.Starting;
        // Comenzar el juego
        // Teletranpsorta a los jugadores a sus puntos de spawn
        TeleportPlayersToSpawn();
        // Congela a los policias
        FreezePoliceTeam();
        // Cuenta regresiva de 10 segundos
        yield return StartingGameCountdown();
        // Descongela a los policias
        UnfreezePoliceTeam();
        // Comienza el juego
        Debug.Log("[SERVER] Partida iniciada!!! CORRAAAN");
        LogToPlayers("Partida iniciada!!! CORRAAAN (players)");
        SetPlayersTimedCanvasMessage("¡Policías a la caza de los ladrones!", 6f);
        currentGameState = GameState.Started;
    }

    public void FreezePoliceTeam()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            PlayerTeamSync playerTeam = player.GetComponent<PlayerTeamSync>();
            if (playerController != null && playerTeam != null)
            {
                if (playerTeam.networkPlayerTeam.Value == PlayerTeamSync.Team.Policias)
                    playerController.FreezePlayerClientRpc();
            }
        }
    }
    public void UnfreezePoliceTeam()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            if (playerController != null)
            {
                playerController.UnfreezePlayerClientRpc();
            }
        }
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
    public IEnumerator SetPlayersTimedCanvasMessage(string message, float duration)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerPresentation playerController = player.GetComponent<PlayerPresentation>();
            if (playerController != null)
            {
                playerController.SetPlayerInGameMessageClientRpc(message);
                yield return new WaitForSeconds(duration);
            }
        }
        ResetPlayersCanvasMessage();
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
        policiasPlayers = 0;
        ladronesPlayers = 0;
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
