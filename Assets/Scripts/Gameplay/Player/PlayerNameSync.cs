using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerNameSync : NetworkBehaviour
{

    public NetworkVariable<FixedString128Bytes> networkPlayerName =
        new NetworkVariable<FixedString128Bytes>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

    private UIManager uiManager;
    private string playerName;

    [SerializeField]
    public TextMeshPro nombreJugadorText;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            Debug.Log("Asignando nombre de jugador");
            string playerName = ObtenerNombreJugador();
            if (networkPlayerName.Value.Value != playerName)
                networkPlayerName.Value = playerName;
        }
        ActualizarNombreUI(networkPlayerName.Value.Value);
        networkPlayerName.OnValueChanged += OnNameChanged;
    }
    private void OnDestroy()
    {
        // Desuscribirse del evento
        networkPlayerName.OnValueChanged -= OnNameChanged;
    }
    private void OnNameChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        string playerName = newValue.Value ?? string.Empty;
        ActualizarNombreUI(playerName);
    }

    private void ActualizarNombreUI(string playerName)
    {
        // Implementa la l�gica para actualizar la UI con el nombre del jugador
        // Por ejemplo:

        if (nombreJugadorText != null)
        {
            nombreJugadorText.text = playerName;
        }
        else
        {
            Debug.LogWarning("nombreJugadorText no est� asignado en " + gameObject.name);
        }
    }

    private string ObtenerNombreJugador()
    {
        // Implementa la l�gica para obtener el nombre del jugador
        // Por ejemplo:
        playerName = uiManager.GetPlayerName();
        Debug.Log("nombre jugador ingresado: " + playerName);
        if (playerName == string.Empty || playerName.ToString().Trim().Equals(""))
        {
            return "Jugador " + OwnerClientId;
        }else
        {
            return playerName;
        }
    }

}
