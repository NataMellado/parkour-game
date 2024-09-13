using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System;

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

        private bool isConnected = false;

        public event System.Action<bool> OnConnectionStateChanged;

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
                IsConnected = true;
        }
        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                IsConnected = false;
        }

        public void StartHost(){
            NetworkManager.Singleton.StartHost();
            StartCoroutine(WaitForConnection());
        }

        public void StartClient()
        {

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
        }

        private IEnumerator WaitForConnection()
        {
            // Espera hasta que el cliente esté conectado
            yield return new WaitUntil(() => IsConnected);

            Debug.Log("Conectado al servidor");
            IsConnected = true;
        }
    }
}
