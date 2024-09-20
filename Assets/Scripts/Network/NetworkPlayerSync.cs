using System.Collections;
using System.Collections.Generic;
using Tbvl.GameManager;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkPlayerSync : NetworkBehaviour
{
    private GameManager gameManager;
    private string newPlayername;

    [Header("Componentes UI")]
    [SerializeField] public TextMeshPro nombreJugadorText;

    public NetworkVariable<NetworkString> networkPlayerName = new NetworkVariable<NetworkString>("Jugador", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<NetworkString> player_rconPassword = new NetworkVariable<NetworkString>("", NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);
    //NetworkVariableReadPermission.Everyone,
    //NetworkVariableWritePermission.Server 
    private void Awake()
    {

        gameManager = FindObjectOfType<GameManager>();
        newPlayername = gameManager.playernameInputField.text;
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            networkPlayerName.Value = newPlayername;
        }
        nombreJugadorText.text = networkPlayerName.Value.ToString();
        networkPlayerName.OnValueChanged += OnPlayernameChanged;
    }

    private void OnDestroy()
    {
        networkPlayerName.OnValueChanged -= OnPlayernameChanged;
    }

    private void OnPlayernameChanged(NetworkString oldName, NetworkString newName)
    {
        nombreJugadorText.text = newName;
    }

    //private void OnPlayernameChanged(FixedString128Bytes oldName, FixedString128Bytes newName)
    //{
    //    if (nombreJugadorText != null)
    //    {
    //        nombreJugadorText.text = newName.ToString();
    //    }
    //}

    //public void IngresarPlayername(string playerName)
    //{
    //    SetPlayerNameServerRpc(playerName);
    //}

    //[ServerRpc]
    //private void SetPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
    //{
    //    networkPlayerName.Value = playerName;
    //}


}

public struct NetworkString : INetworkSerializeByMemcpy
{
    private ForceNetworkSerializeByMemcpy<FixedString128Bytes> _info;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _info);
    }

    public override string ToString()
    {
        return _info.Value.ToString();
    }

    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { _info = new FixedString128Bytes(s) };
}
