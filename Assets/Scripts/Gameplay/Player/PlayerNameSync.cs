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

    [SerializeField]
    public TextMeshPro nombreJugadorText;

    private void Start()
    {
        if (IsOwner)
        {
            Debug.Log("Asignando nombre de jugador");
            string playerName = ObtenerNombreJugador() ?? string.Empty;
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
        // Implementa la lógica para actualizar la UI con el nombre del jugador
        // Por ejemplo:

        if (nombreJugadorText != null)
        {
            nombreJugadorText.text = playerName;
        }
        else
        {
            Debug.LogWarning("nombreJugadorText no está asignado en " + gameObject.name);
        }
    }

    private string ObtenerNombreJugador()
    {
        // Implementa la lógica para obtener el nombre del jugador
        // Por ejemplo:
        return "Jugador " + OwnerClientId;
    }

}
