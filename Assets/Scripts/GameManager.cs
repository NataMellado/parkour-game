using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System;
using Unity.VisualScripting;

namespace Tbvl.GameManager
{

    public class GameManager : MonoBehaviour
    {

        [SerializeField]
        public TMP_InputField serverIpText;
        public bool IsConnected
        {
            get => isConnected;
            private set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnConnectionStateChanged?.Invoke(isConnected); // Cuando cambia el estado de la conexión, se invoca el evento
                }
            }
        }

        public float connectionTimeout = 4f;

        public bool isConnecting = false;

        private bool isConnected = false;

        public event System.Action<bool> OnConnectionStateChanged;

        public GameObject[] playerPrefabs;

        public void Start()
        {

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            IsConnected = false;
        }

        private void OnDestroy()
        {
            // Desuscribirse de los eventos cuando este objeto sea destruido
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                IsConnected = true;
                isConnecting = false;
                CharacterSelection.Instance.SelectSkin();
                //SubmitCharacterSelectionServerRpc(CharacterSelection.Instance.GetSelectedCharacterIndex());
                StartCoroutine(CallSpawnPlayer());
            }
        }

        private IEnumerator CallSpawnPlayer()
        {
            while (PlayerSpawner.Instance == null)
            {
                yield return null;
            }
            int selectedIndex = CharacterSelection.Instance.GetSelectedCharacterIndex();
            PlayerSpawner.Instance.SubmitCharacterSelectionServerRpc(selectedIndex);
        }

        //[ServerRpc]
        //private void SubmitCharacterSelectionServerRpc(int selectedIndex, ServerRpcParams rpcParams = default)
        //{
        //    ulong clientId = rpcParams.Receive.SenderClientId;
        //    GameObject playerPrefab = GetPlayerPrefab(selectedIndex);
        //    GameObject playerInstance = Instantiate(playerPrefab);
        //    playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        //}

        private GameObject GetPlayerPrefab(int index)
        {
            return playerPrefabs[index];
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                IsConnected = false;
            isConnecting = false;
        }

        public void StartHost(){
            if (isConnected || isConnecting)
                return;
            isConnecting = true;
            NetworkManager.Singleton.StartHost();
            StartCoroutine(WaitForConnection());
        }

        public void StartClient()
        {
            if (isConnected || isConnecting)
                return;

            isConnecting = true;
            //NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(SelectedCharacterIndex.ToString());

            // Obtener texto desde el objeto serverIpText (InputField)
            string serverIp = (serverIpText.text == "" || 
                serverIpText.text.Trim() == "" ||
                serverIpText.text.Trim() == null) ? "127.0.0.1" : serverIpText.text;
            ushort serverPort = 7777;

            // Configura la IP y el puerto del servidor
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetConnectionData(
                    serverIp,
                    serverPort,
                    "0.0.0.0");
            Debug.Log("Conectando a " + serverIp + ":" + serverPort);
            try
            {
                NetworkManager.Singleton.StartClient();
                StartCoroutine(WaitForConnection());
            }catch (Exception ex)
            {
                Debug.LogError($"Ocurrió un error en StartClient de GameManager!: {ex}");
            } 

        }

        public bool ValidateServerAddress(string ip)
        {
            bool valid = false;
            string[] separatedIp = ip.Split('.');

            if (separatedIp.Length == 4)
            {
                valid = true;
                foreach (string part in separatedIp)
                {
                    if (!byte.TryParse(part, out byte result))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;
            NetworkManager.Singleton.Shutdown();
            isConnecting = false;
        }

        private IEnumerator WaitForConnection()
        {
            // Espera hasta que el cliente esté conectado o como máximo 4 segundos:
            float elapsedTime = 0f;
            while (!IsConnected && elapsedTime < connectionTimeout)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Validar que haya conectado a un servidor
            if (!IsConnected)
            {
                Debug.LogWarning("No se pudo conectar al servidor");
                NetworkManager.Singleton.Shutdown();
                isConnecting = false;
                yield break;
            }
            
            // Si se conectó, se establece la conexión


            Debug.Log("Conectado al servidor");
            IsConnected = true;
        }
    }
}
