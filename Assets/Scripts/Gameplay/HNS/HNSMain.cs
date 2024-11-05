using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main class for the Hide and Seek game mode.
/// Loop for the game mode.
/// </summary>
public class HNSMain : MonoBehaviour
{
    public static HNSMain instance;

    //> Gamemode States

    enum GameState
    {
        Starting,
        Started,
        Playing,
        Ending,
        Ended
    }

    private void Start()
    {
        Console.WriteLine("HNSMain started");
        StartCoroutine(CheckForConnectedPlayers());
    }

    public IEnumerator CheckForConnectedPlayers()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            yield return new WaitForSeconds(2f);
            // Get connected player objects
            Debug.Log("Imprimiendo connected players hnsmain...");
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                PlayerNameSync playerNameSync = player.GetComponent<PlayerNameSync>();
                PlayerTeamSync playerTeamSync = player.GetComponent<PlayerTeamSync>();
                if (playerNameSync != null && playerTeamSync != null)
                {
                    Debug.Log("Player name: " + playerNameSync.networkPlayerName.Value);
                    Debug.Log("Player team: " + playerTeamSync.networkPlayerTeam.Value);
                }
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
