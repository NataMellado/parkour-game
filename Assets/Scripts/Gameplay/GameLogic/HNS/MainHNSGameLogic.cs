using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainHNSGameLogic : MonoBehaviour
{

    [SerializeField]
    public int MIN_JUGADORES_INICIO = 1;

    bool isServer;
    bool minPlayers;

    private void Start()
    {
        // Verificar si se inició una instancia NetworkManager
        if (NetworkManager.Singleton != null)
        {
            isServer = NetworkManager.Singleton.IsServer;
            Debug.Log("Is server?: " + isServer.ToString());
        }
    }

    public void StartHNSGameLoop()
    {
        StartCoroutine(HNS_StartGameLoop());
        Debug.LogWarning("HNS Game Loop started");
    }

    public IEnumerator HNS_StartGameLoop()
    {
        yield return new WaitForSeconds(5f);

        while (isServer)
        {
            yield return new WaitForSeconds(5f);
            Debug.Log("HNS Game Loop");
            // Verificar si hay un mínimo de jugador para iniciar el juego
            minPlayers = VerificarMinimoJugadoresConectados();

            // Si hay un mínimo de jugadores, iniciar el juego
            if (minPlayers)
            {
                Debug.Log("Iniciar juego");
                // Si inicia el juego, tomar jugadores conectados aleatoriamente
                IReadOnlyList<NetworkClient> jugadores = NetworkManager.Singleton.ConnectedClientsList;
                //Debug.Log(jugadores.ToString());
                // Imprimir nombres de jugadores
                foreach (NetworkClient jugador in jugadores)
                {
                    Debug.Log(jugador.PlayerObject.gameObject.GetComponent<PlayerPresentation>().name + " :: " + jugador.PlayerObject.gameObject.GetComponent<NetworkPlayerSync>().networkPlayerName.Value);
                }

            }
            // Si no hay un mínimo de jugadores, esperar
            else
            {
                Debug.Log("Esperar jugadores");
            }

        }
    }

    public bool VerificarMinimoJugadoresConectados()
    {
        //TODO: Implementar lógica de verificación de jugadores que no se encuentren en modo espectador
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= MIN_JUGADORES_INICIO)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
