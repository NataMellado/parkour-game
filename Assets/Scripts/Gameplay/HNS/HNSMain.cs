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
    int connectedPlayers = ServerManager.Instance.connectedPlayers.Count;

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
        StartCoroutine(CheckForConnectedPlayers());
    }

    public IEnumerator CheckForConnectedPlayers()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            yield return new WaitForSeconds(2f);
            Debug.Log("Conn players: " + connectedPlayers);
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
